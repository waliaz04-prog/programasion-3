public static class ExamConfiguration
{
    // El Title ID no es secreto. Se obtiene en PlayFab > Game Manager > Settings.
    public const string PlayFabTitleId = "";

    // Se obtiene en https://openweathermap.org/api.
    // Para una entrega pública conviene restringir o renovar esta clave.
    public const string OpenWeatherApiKey = "";

    public const string LeaderboardStatisticName = "HighScore";
    public const int LeaderboardSize = 10;
    public const float WeatherChangeIntervalSeconds = 30f;
}
