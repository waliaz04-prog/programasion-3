using System;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayFabLeaderboardManager : MonoBehaviour
{
    public static PlayFabLeaderboardManager Instance { get; private set; }
    public bool IsLoggedIn => loggedIn;
    public bool IsGuestSession => guestSession;
    public string CurrentPlayFabId => playFabId;
    public bool ShouldShowLeaderboard => showLeaderboard;

    private readonly List<PlayerLeaderboardEntry> leaderboard = new List<PlayerLeaderboardEntry>();
    private PlayerLeaderboardEntry currentPlayer;
    private bool loggedIn;
    private bool guestSession;
    private bool showLeaderboard;
    private bool loading;
    private string status = "Conectando con PlayFab...";
    private string playFabId;

    private GUIStyle titleStyle;
    private GUIStyle rowStyle;
    private GUIStyle currentRowStyle;
    private GUIStyle statusStyle;
    private GUIStyle avatarStyle;
    private Texture2D panelTexture;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!string.IsNullOrWhiteSpace(ExamConfiguration.PlayFabTitleId))
        {
            PlayFabSettings.staticSettings.TitleId = ExamConfiguration.PlayFabTitleId;
        }
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(PlayFabSettings.staticSettings.TitleId))
        {
            status = "Falta configurar PlayFabTitleId en ExamConfiguration.cs";
            Debug.LogError(status, this);
            return;
        }

        status = "Esperando inicio de sesión";
    }

    private void Update()
    {
        if (FindAnyObjectByType<MenuInspectorController>() != null)
        {
            return;
        }

        if (loggedIn
            && SceneManager.GetActiveScene().name == ExamConfiguration.MenuSceneName
            && Keyboard.current?.lKey.wasPressedThisFrame == true)
        {
            ToggleLeaderboard();
        }
    }

    public void SetAuthenticatedPlayer(string authenticatedPlayFabId, string displayName)
    {
        loggedIn = true;
        guestSession = false;
        loading = false;
        playFabId = authenticatedPlayFabId;
        status = string.IsNullOrWhiteSpace(displayName)
            ? "PlayFab conectado"
            : "Sesión iniciada: " + displayName;

        if (string.IsNullOrWhiteSpace(displayName))
        {
            string suffix = playFabId.Length > 4 ? playFabId.Substring(playFabId.Length - 4) : playFabId;
            PlayFabClientAPI.UpdateUserTitleDisplayName(
                new UpdateUserTitleDisplayNameRequest { DisplayName = "Jugador-" + suffix },
                _ => FetchLeaderboard(),
                OnPlayFabError);
        }
        else
        {
            FetchLeaderboard();
        }

    }

    public void SubmitScore(int score)
    {
        if (guestSession)
        {
            status = "Modo invitado: el puntaje no se guardó";
            Debug.Log(status, this);
            return;
        }

        if (!loggedIn)
        {
            status = "Sin sesión: el puntaje no se guardó";
            Debug.Log(status, this);
            return;
        }

        PlayFabClientAPI.GetPlayerStatistics(
            new GetPlayerStatisticsRequest(),
            result =>
            {
                int previousBest = 0;
                StatisticValue statistic = result.Statistics?.Find(
                    value => value.StatisticName == ExamConfiguration.LeaderboardStatisticName);

                if (statistic != null)
                {
                    previousBest = statistic.Value;
                }

                if (score <= previousBest)
                {
                    status = $"Récord conservado: {previousBest}";
                    FetchLeaderboard();
                    return;
                }

                PlayFabClientAPI.UpdatePlayerStatistics(
                    new UpdatePlayerStatisticsRequest
                    {
                        Statistics = new List<StatisticUpdate>
                        {
                            new StatisticUpdate
                            {
                                StatisticName = ExamConfiguration.LeaderboardStatisticName,
                                Value = score
                            }
                        }
                    },
                    _ =>
                    {
                        status = $"Nuevo récord guardado: {score}";
                        FetchLeaderboard();
                    },
                    OnPlayFabError);
            },
            OnPlayFabError);
    }

    public void StartGuestSession()
    {
        guestSession = true;
        loggedIn = false;
        showLeaderboard = false;
        loading = false;
        playFabId = string.Empty;
        status = "Jugando como invitado: los resultados no se guardarán";
    }

    public void ShowLeaderboard()
    {
        showLeaderboard = true;
        FetchLeaderboard();
    }

    public void ToggleLeaderboard()
    {
        showLeaderboard = !showLeaderboard;
        if (showLeaderboard)
        {
            FetchLeaderboard();
        }
    }

    // Se oculta al salir de Menu para que las puntuaciones no invadan la partida.
    public void HideLeaderboard()
    {
        showLeaderboard = false;
    }

    private void FetchLeaderboard()
    {
        if (!loggedIn || SceneManager.GetActiveScene().name != ExamConfiguration.MenuSceneName)
        {
            return;
        }

        loading = true;
        PlayerProfileViewConstraints profile = new PlayerProfileViewConstraints
        {
            ShowDisplayName = true
        };

        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName = ExamConfiguration.LeaderboardStatisticName,
                StartPosition = 0,
                MaxResultsCount = ExamConfiguration.LeaderboardSize,
                ProfileConstraints = profile
            },
            result =>
            {
                leaderboard.Clear();
                leaderboard.AddRange(result.Leaderboard);
                RequestCurrentPlayer(profile);
            },
            OnPlayFabError);
    }

    private void RequestCurrentPlayer(PlayerProfileViewConstraints profile)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = ExamConfiguration.LeaderboardStatisticName,
                MaxResultsCount = 1,
                ProfileConstraints = profile
            },
            result =>
            {
                currentPlayer = result.Leaderboard.Count > 0 ? result.Leaderboard[0] : null;
                loading = false;
            },
            OnPlayFabError);
    }

    private void OnPlayFabError(PlayFabError error)
    {
        loading = false;
        status = "PlayFab: " + error.ErrorMessage;
        Debug.LogError(error.GenerateErrorReport(), this);
    }

    private void OnGUI()
    {
        if (FindAnyObjectByType<MenuInspectorController>() != null)
        {
            return;
        }

        if (!loggedIn)
        {
            return;
        }

        EnsureStyles();

        if (GUI.Button(new Rect(Screen.width - 190f, 15f, 175f, 38f), "Leaderboard [L]"))
        {
            ToggleLeaderboard();
        }

        if (!showLeaderboard)
        {
            return;
        }

        float width = Mathf.Min(620f, Screen.width - 30f);
        float height = Mathf.Min(660f, Screen.height - 80f);
        Rect panel = new Rect((Screen.width - width) * 0.5f, 60f, width, height);
        GUI.DrawTexture(panel, panelTexture);

        GUI.Label(new Rect(panel.x + 20f, panel.y + 15f, panel.width - 40f, 42f), "MEJORES PUNTAJES", titleStyle);
        GUI.Label(new Rect(panel.x + 20f, panel.y + 55f, panel.width - 40f, 25f), loading ? "Actualizando..." : status, statusStyle);

        float y = panel.y + 90f;
        for (int i = 0; i < leaderboard.Count; i++)
        {
            DrawEntry(leaderboard[i], panel.x + 20f, y, panel.width - 40f, leaderboard[i].PlayFabId == playFabId);
            y += 45f;
        }

        bool playerIsInTop = currentPlayer != null && leaderboard.Exists(entry => entry.PlayFabId == currentPlayer.PlayFabId);
        if (currentPlayer != null && !playerIsInTop)
        {
            GUI.Label(new Rect(panel.x + 20f, y + 4f, panel.width - 40f, 24f), "TU POSICIÓN", statusStyle);
            DrawEntry(currentPlayer, panel.x + 20f, y + 30f, panel.width - 40f, true);
        }
    }

    private void DrawEntry(PlayerLeaderboardEntry entry, float x, float y, float width, bool isCurrent)
    {
        GUIStyle style = isCurrent ? currentRowStyle : rowStyle;
        string name = entry.DisplayName;
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Jugador";
        }

        Rect avatarRect = new Rect(x + 4f, y + 4f, 36f, 36f);
        Color previousColor = GUI.color;
        GUI.color = AvatarColor(entry.PlayFabId);
        GUI.DrawTexture(avatarRect, Texture2D.whiteTexture);
        GUI.color = previousColor;

        GUI.Label(avatarRect, name.Substring(0, 1).ToUpperInvariant(), avatarStyle);

        GUI.Label(new Rect(x + 50f, y, width - 150f, 44f), $"#{entry.Position + 1}   {name}", style);
        GUI.Label(new Rect(x + width - 100f, y, 95f, 44f), entry.StatValue.ToString(), style);
    }

    private static Color AvatarColor(string id)
    {
        int hash = string.IsNullOrEmpty(id) ? 1 : id.GetHashCode();
        float hue = Mathf.Abs(hash % 1000) / 1000f;
        return Color.HSVToRGB(hue, 0.65f, 0.95f);
    }

    private void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        panelTexture = new Texture2D(1, 1);
        panelTexture.SetPixel(0, 0, new Color(0.03f, 0.05f, 0.08f, 0.96f));
        panelTexture.Apply();

        titleStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        rowStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleLeft,
            fontSize = 18,
            normal = { textColor = new Color(0.88f, 0.9f, 0.95f) }
        };
        currentRowStyle = new GUIStyle(rowStyle)
        {
            fontStyle = FontStyle.Bold,
            normal = { textColor = new Color(1f, 0.82f, 0.2f) }
        };
        statusStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14,
            normal = { textColor = new Color(0.65f, 0.75f, 0.85f) }
        };
        avatarStyle = new GUIStyle(rowStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
    }

    private void OnDestroy()
    {
        if (panelTexture != null)
        {
            Destroy(panelTexture);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
