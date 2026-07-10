using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// WeatherManager
/// Latitud y Longitud del lugar del cual quiero conseguir el clima
/// </summary>
public class WeatherManager : MonoBehaviour
{
    [SerializeField] private float latitude;
    [SerializeField] private float longitude;

    // Clave de identificacion
    private string appID = "ea636cce411320823159b324fa4acdd4";

    // Aqui almacenaremos el link para llamar a la api, ya con los datos
    private string getWeatherApiCall;

    private void Start()
    {
        getWeatherApiCall = $"https://api.openweathermap.org/data/3.0/onecall?lat={latitude}&lon={longitude}&appid={appID}&units=metric&exclude=minutely,hourly,daily";
    }

    /// <summary>
    /// Request / Pedido / Peticion / Solicitud
    /// </summary>
    private IEnumerator RetrieveWeather()
    {
        // Esta linea me crea un request para llamar a la api del clima
        UnityWebRequest request = new UnityWebRequest(getWeatherApiCall);
        request.downloadHandler = new DownloadHandlerBuffer(); // Esta linea nos sirve para indicar como queremos que se guarde la informacion

        yield return request.SendWebRequest(); // La corrutina se va a esperar hasta que mi solicitud se procese en la web

        // result la usamos para referirnos al resultado, independientemente de si salio bien o mal
        if (request.result != UnityWebRequest.Result.Success) // Si el resultado de mi peticion salio mal, voy a...
        {
            Debug.Log(request.error);
        }
        else
        {
            // Aqui iria la logica para procesar el JSON de respuesta
        }
    }
}
