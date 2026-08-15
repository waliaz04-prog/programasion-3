using UnityEngine;

public static class ExamConfiguration
{
    // El Title ID no es secreto. Se obtiene en PlayFab > Game Manager > Settings.
    public const string PlayFabTitleId = "2F0C7";

    // Se obtiene en https://openweathermap.org/api.
    // Para una entrega pública conviene restringir o renovar esta clave.
    public static string OpenWeatherApiKey
    {
        get
        {
            TextAsset keyFile = Resources.Load<TextAsset>("OpenWeatherApiKey");
            return keyFile == null ? string.Empty : keyFile.text.Trim();
        }
    }

    public const string LeaderboardStatisticName = "HighScore";
    public const int LeaderboardSize = 10;
    public const float WeatherChangeIntervalSeconds = 30f;
    public const string MenuSceneName = "Menu";
    public const string GameSceneName = "SampleScene";
}
