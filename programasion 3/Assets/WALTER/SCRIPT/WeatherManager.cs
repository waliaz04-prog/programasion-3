using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Json

public class WeatherManager : MonoBehaviour
{
    [SerializeField] private float latitude;
    [SerializeField] private float longitude;

    private string appID = "ea636cce411320823159b324fa4acdd4";

    private string getWeatherApiCall;

    private void Start()
    {
        getWeatherApiCall = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&appid={appID}&units=metric&exclude=minutely,hourly,daily";
    }

    private IEnumerator RetrieveWeather()
    {
        UnityWebRequest request = new UnityWebRequest(getWeatherApiCall);
        request.downloadHandler = new DownloadHandlerBuffer(); 

        yield return request.SendWebRequest(); 

        if (request.result != UnityWebRequest.Result.Success) 
        {
            Debug.Log(request.error);
        }
        else
        {
          
        }
    }

    private void ReadTson()
    {

    }
}
