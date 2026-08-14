using UnityEngine;

public static class ExamServicesBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateServices()
    {
        GameObject services = new GameObject("Exam Services");
        Object.DontDestroyOnLoad(services);
        services.AddComponent<PlayFabLeaderboardManager>();
        services.AddComponent<WeatherManager>();
    }
}
