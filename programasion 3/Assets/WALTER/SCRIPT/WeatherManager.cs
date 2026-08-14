using System.Collections;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherManager : MonoBehaviour
{
    private readonly City[] cities =
    {
        new City("Ciudad de México", 19.4326f, -99.1332f),
        new City("Monterrey", 25.6866f, -100.3161f),
        new City("Guadalajara", 20.6597f, -103.3496f),
        new City("Cancún", 21.1619f, -86.8515f),
        new City("Londres", 51.5074f, -0.1278f),
        new City("Tokio", 35.6762f, 139.6503f),
        new City("Nueva York", 40.7128f, -74.0060f),
        new City("El Cairo", 30.0444f, 31.2357f),
        new City("Sídney", -33.8688f, 151.2093f),
        new City("París", 48.8566f, 2.3522f)
    };

    private Camera mainCamera;
    private Light directionalLight;
    private int lastCityIndex = -1;
    private string weatherStatus = "Preparando clima...";

    private void Start()
    {
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
            weatherStatus = "Falta OpenWeatherApiKey en ExamConfiguration.cs";
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
                new Color(0.08f, 0.09f, 0.18f), new Color(0.22f, 0.24f, 0.35f),
                new Color(0.45f, 0.5f, 0.65f), new Color(0.6f, 0.65f, 0.9f), 0.35f, true, 0.035f);
        }

        if (weatherId >= 300 && weatherId < 600)
        {
            return new WeatherVisuals(
                new Color(0.18f, 0.24f, 0.32f), new Color(0.3f, 0.36f, 0.45f),
                new Color(0.48f, 0.55f, 0.62f), new Color(0.7f, 0.78f, 0.9f), 0.5f, true, 0.025f);
        }

        if (weatherId >= 600 && weatherId < 700)
        {
            return new WeatherVisuals(
                new Color(0.72f, 0.82f, 0.92f), new Color(0.78f, 0.86f, 0.92f),
                new Color(0.82f, 0.88f, 0.95f), new Color(0.82f, 0.9f, 1f), 0.9f, true, 0.015f);
        }

        if (weatherId >= 700 && weatherId < 800)
        {
            return new WeatherVisuals(
                new Color(0.45f, 0.47f, 0.5f), new Color(0.55f, 0.56f, 0.58f),
                new Color(0.55f, 0.57f, 0.6f), Color.white, 0.55f, true, 0.045f);
        }

        if (weatherId == 800)
        {
            Color warmLight = temperature >= 28f ? new Color(1f, 0.82f, 0.58f) : new Color(1f, 0.95f, 0.84f);
            return new WeatherVisuals(
                new Color(0.28f, 0.62f, 0.92f), new Color(0.45f, 0.62f, 0.78f),
                new Color(0.65f, 0.75f, 0.85f), warmLight, 1.2f, false, 0f);
        }

        return new WeatherVisuals(
            new Color(0.42f, 0.52f, 0.64f), new Color(0.48f, 0.53f, 0.6f),
            new Color(0.58f, 0.63f, 0.7f), new Color(0.85f, 0.88f, 0.92f), 0.72f, true, 0.008f);
    }

    private void OnGUI()
    {
        GUI.Box(new Rect(15f, 15f, Mathf.Min(420f, Screen.width - 220f), 38f), "Clima: " + weatherStatus);
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
