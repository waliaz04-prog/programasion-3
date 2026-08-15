using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class WeatherManager : MonoBehaviour
{
    private readonly City[] cities =
    {
        new City("Reikiavik", 64.1466f, -21.9426f),
        new City("Vancouver", 49.2827f, -123.1207f),
        new City("Nairobi", -1.2921f, 36.8219f),
        new City("Ciudad del Cabo", -33.9249f, 18.4241f),
        new City("Buenos Aires", -34.6037f, -58.3816f),
        new City("Santiago de Chile", -33.4489f, -70.6693f),
        new City("Singapur", 1.3521f, 103.8198f),
        new City("Mumbai", 19.0760f, 72.8777f),
        new City("Seúl", 37.5665f, 126.9780f),
        new City("Oslo", 59.9139f, 10.7522f)
    };

    private Camera mainCamera;
    private Light directionalLight;
    private int lastCityIndex = -1;
    private string weatherStatus = "Preparando clima...";

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        ConfigureForScene(SceneManager.GetActiveScene());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ConfigureForScene(scene);
    }

    private void ConfigureForScene(Scene scene)
    {
        StopAllCoroutines();
        mainCamera = null;
        directionalLight = null;

        if (scene.name != ExamConfiguration.GameSceneName)
        {
            weatherStatus = "El clima se activará al comenzar la partida";
            return;
        }

        mainCamera = Camera.main;
        Light[] lights = FindObjectsByType<Light>();
        foreach (Light sceneLight in lights)
        {
            if (sceneLight.type == LightType.Directional)
            {
                directionalLight = sceneLight;
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(ExamConfiguration.OpenWeatherApiKey))
        {
            weatherStatus = "Falta Assets/Resources/OpenWeatherApiKey.txt";
            Debug.LogError(weatherStatus, this);
            return;
        }

        StartCoroutine(WeatherCycle());
    }

    private IEnumerator WeatherCycle()
    {
        yield return RequestRandomCityWeather();
        yield return new WaitForSeconds(ExamConfiguration.WeatherChangeIntervalSeconds);
        yield return RequestRandomCityWeather();
    }

    private IEnumerator RequestRandomCityWeather()
    {
        int cityIndex;
        do
        {
            cityIndex = Random.Range(0, cities.Length);
        }
        while (cities.Length > 1 && cityIndex == lastCityIndex);

        lastCityIndex = cityIndex;
        City city = cities[cityIndex];
        string url = "https://api.openweathermap.org/data/2.5/weather"
            + $"?lat={city.Latitude}&lon={city.Longitude}"
            + $"&appid={ExamConfiguration.OpenWeatherApiKey}&units=metric&lang=es";

        weatherStatus = "Consultando clima de " + city.Name + "...";

        using UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 15;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            weatherStatus = "Error Weather API: " + request.error;
            Debug.LogError(weatherStatus, this);
            yield break;
        }

        ApplyWeather(city, request.downloadHandler.text);
    }

    private void ApplyWeather(City city, string json)
    {
        JSONNode root = JSON.Parse(json);
        JSONNode weather = root?["weather"]?[0];
        JSONNode main = root?["main"];

        if (weather == null || main == null)
        {
            weatherStatus = "Weather API devolvió datos incompletos";
            Debug.LogWarning(weatherStatus, this);
            return;
        }

        int weatherId = weather["id"].AsInt;
        float temperature = main["temp"].AsFloat;
        string description = weather["description"].Value;

        WeatherVisuals visuals = GetVisuals(weatherId, temperature);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = visuals.AmbientColor;
        RenderSettings.fog = visuals.UseFog;
        RenderSettings.fogColor = visuals.FogColor;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = visuals.FogDensity;

        if (mainCamera != null)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = visuals.BackgroundColor;
        }

        if (directionalLight != null)
        {
            directionalLight.color = visuals.LightColor;
            directionalLight.intensity = visuals.LightIntensity;
        }

        weatherStatus = $"{city.Name}: {description}, {temperature:0.#} °C";
        Debug.Log("Clima aplicado al escenario: " + weatherStatus, this);
    }

    private static WeatherVisuals GetVisuals(int weatherId, float temperature)
    {
        if (weatherId >= 200 && weatherId < 300)
        {
            return new WeatherVisuals(
                new Color(0.12f, 0.03f, 0.2f), new Color(0.2f, 0.08f, 0.28f),
                new Color(0.3f, 0.16f, 0.42f), new Color(0.72f, 0.55f, 1f), 0.42f, true, 0.04f);
        }

        if (weatherId >= 300 && weatherId < 600)
        {
            return new WeatherVisuals(
                new Color(0.02f, 0.2f, 0.25f), new Color(0.08f, 0.28f, 0.3f),
                new Color(0.12f, 0.4f, 0.42f), new Color(0.55f, 0.9f, 0.88f), 0.58f, true, 0.028f);
        }

        if (weatherId >= 600 && weatherId < 700)
        {
            return new WeatherVisuals(
                new Color(0.48f, 0.62f, 0.86f), new Color(0.62f, 0.7f, 0.9f),
                new Color(0.72f, 0.8f, 1f), new Color(0.72f, 0.84f, 1f), 1.05f, true, 0.018f);
        }

        if (weatherId >= 700 && weatherId < 800)
        {
            return new WeatherVisuals(
                new Color(0.34f, 0.27f, 0.18f), new Color(0.4f, 0.34f, 0.24f),
                new Color(0.55f, 0.47f, 0.34f), new Color(1f, 0.78f, 0.5f), 0.62f, true, 0.05f);
        }

        if (weatherId == 800)
        {
            Color warmLight = temperature >= 28f ? new Color(1f, 0.82f, 0.58f) : new Color(1f, 0.95f, 0.84f);
            return new WeatherVisuals(
                new Color(0.18f, 0.55f, 0.78f), new Color(0.38f, 0.58f, 0.68f),
                new Color(0.62f, 0.72f, 0.78f), warmLight, 1.28f, false, 0f);
        }

        return new WeatherVisuals(
            new Color(0.26f, 0.34f, 0.48f), new Color(0.34f, 0.4f, 0.52f),
            new Color(0.42f, 0.48f, 0.58f), new Color(0.78f, 0.84f, 0.95f), 0.76f, true, 0.01f);
    }

    private void OnGUI()
    {
        if (SceneManager.GetActiveScene().name != ExamConfiguration.GameSceneName)
        {
            return;
        }

        GUI.Box(new Rect(15f, 15f, Mathf.Min(420f, Screen.width - 220f), 38f), "Clima: " + weatherStatus);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private readonly struct City
    {
        public readonly string Name;
        public readonly float Latitude;
        public readonly float Longitude;

        public City(string name, float latitude, float longitude)
        {
            Name = name;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    private readonly struct WeatherVisuals
    {
        public readonly Color BackgroundColor;
        public readonly Color AmbientColor;
        public readonly Color FogColor;
        public readonly Color LightColor;
        public readonly float LightIntensity;
        public readonly bool UseFog;
        public readonly float FogDensity;

        public WeatherVisuals(
            Color backgroundColor,
            Color ambientColor,
            Color fogColor,
            Color lightColor,
            float lightIntensity,
            bool useFog,
            float fogDensity)
        {
            BackgroundColor = backgroundColor;
            AmbientColor = ambientColor;
            FogColor = fogColor;
            LightColor = lightColor;
            LightIntensity = lightIntensity;
            UseFog = useFog;
            FogDensity = fogDensity;
        }
    }
}
