using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// Todas las referencias visuales se asignan manualmente desde el Inspector.
public class MenuInspectorController : MonoBehaviour
{
    [Header("Configuración de PlayFab")]
    [SerializeField] private string playFabTitleId = "2F0C7";
    [SerializeField] private string statisticName = "HighScore";
    [SerializeField, Range(10, 100)] private int leaderboardSize = 10;

    [Header("Escena del juego")]
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("Paneles del Canvas")]
    [SerializeField] private GameObject panelInitial;
    [SerializeField] private GameObject panelLogin;
    [SerializeField] private GameObject panelCreateAccount;
    [SerializeField] private GameObject panelMain;
    [SerializeField] private GameObject panelLeaderboard;

    [Header("Campos de iniciar sesión")]
    [SerializeField] private TMP_InputField loginUserOrEmailInput;
    [SerializeField] private TMP_InputField loginPasswordInput;
    [SerializeField] private TMP_Text loginStatusText;

    [Header("Campos de crear cuenta")]
    [SerializeField] private TMP_InputField registerEmailInput;
    [SerializeField] private TMP_InputField registerUsernameInput;
    [SerializeField] private TMP_InputField registerPasswordInput;
    [SerializeField] private TMP_Text registerStatusText;

    [Header("Leaderboard")]
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private LeaderboardRowView leaderboardRowTemplate;
    [SerializeField] private LeaderboardRowView currentPlayerRow;
    [SerializeField] private TMP_Text leaderboardStatusText;

    private readonly List<LeaderboardRowView> generatedRows = new List<LeaderboardRowView>();
    private bool requestInProgress;

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(playFabTitleId))
        {
            PlayFabSettings.staticSettings.TitleId = playFabTitleId;
        }

        if (leaderboardRowTemplate != null)
        {
            leaderboardRowTemplate.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        Time.timeScale = 0f;

        PlayFabLeaderboardManager manager = PlayFabLeaderboardManager.Instance;
        if (manager != null && manager.IsLoggedIn)
        {
            if (manager.ShouldShowLeaderboard)
            {
                ShowLeaderboardPanel();
            }
            else
            {
                ShowMainPanel();
            }
        }
        else
        {
            ShowInitialPanel();
        }
    }

    public void ShowInitialPanel()
    {
        ShowOnly(panelInitial);
    }

    public void ShowLoginPanel()
    {
        ShowOnly(panelLogin);
        SetStatus(loginStatusText, "Ingresa tu usuario o correo y contraseña", false);
        loginUserOrEmailInput?.Select();
    }

    public void ShowCreateAccountPanel()
    {
        ShowOnly(panelCreateAccount);
        SetStatus(registerStatusText, "Completa correo, usuario y contraseña", false);
        registerEmailInput?.Select();
    }

    public void ShowMainPanel()
    {
        ShowOnly(panelMain);
        Time.timeScale = 1f;
    }

    public void Login()
    {
        if (requestInProgress)
        {
            return;
        }

        string identity = loginUserOrEmailInput == null ? string.Empty : loginUserOrEmailInput.text.Trim();
        string password = loginPasswordInput == null ? string.Empty : loginPasswordInput.text;
        if (string.IsNullOrWhiteSpace(identity) || string.IsNullOrWhiteSpace(password))
        {
            SetStatus(loginStatusText, "Completa los dos campos", true);
            return;
        }

        requestInProgress = true;
        SetStatus(loginStatusText, "Conectando con PlayFab...", false);
        GetPlayerCombinedInfoRequestParams info = new GetPlayerCombinedInfoRequestParams
        {
            GetPlayerProfile = true
        };

        if (identity.Contains("@"))
        {
            PlayFabClientAPI.LoginWithEmailAddress(
                new LoginWithEmailAddressRequest
                {
                    TitleId = playFabTitleId,
                    Email = identity,
                    Password = password,
                    InfoRequestParameters = info
                },
                result => CompleteAuthentication(
                    result.PlayFabId,
                    result.InfoResultPayload?.PlayerProfile?.DisplayName ?? identity),
                error => HandleAuthError(error, loginStatusText));
        }
        else
        {
            PlayFabClientAPI.LoginWithPlayFab(
                new LoginWithPlayFabRequest
                {
                    TitleId = playFabTitleId,
                    Username = identity,
                    Password = password,
                    InfoRequestParameters = info
                },
                result => CompleteAuthentication(
                    result.PlayFabId,
                    result.InfoResultPayload?.PlayerProfile?.DisplayName ?? identity),
                error => HandleAuthError(error, loginStatusText));
        }
    }

    public void CreateAccount()
    {
        if (requestInProgress)
        {
            return;
        }

        string email = registerEmailInput == null ? string.Empty : registerEmailInput.text.Trim();
        string username = registerUsernameInput == null ? string.Empty : registerUsernameInput.text.Trim();
        string password = registerPasswordInput == null ? string.Empty : registerPasswordInput.text;

        if (!email.Contains("@"))
        {
            SetStatus(registerStatusText, "Escribe un correo válido", true);
            return;
        }

        if (username.Length < 3 || username.Length > 20 || username.Contains(" "))
        {
            SetStatus(registerStatusText, "El usuario debe tener entre 3 y 20 caracteres sin espacios", true);
            return;
        }

        if (password.Length < 6)
        {
            SetStatus(registerStatusText, "La contraseña debe tener al menos 6 caracteres", true);
            return;
        }

        requestInProgress = true;
        SetStatus(registerStatusText, "Creando cuenta en PlayFab...", false);
        PlayFabClientAPI.RegisterPlayFabUser(
            new RegisterPlayFabUserRequest
            {
                TitleId = playFabTitleId,
                Email = email,
                Username = username,
                Password = password,
                DisplayName = username,
                RequireBothUsernameAndEmail = true
            },
            result => CompleteAuthentication(result.PlayFabId, username),
            error => HandleAuthError(error, registerStatusText));
    }

    public void StartGame()
    {
        if (PlayFabLeaderboardManager.Instance == null || !PlayFabLeaderboardManager.Instance.IsLoggedIn)
        {
            ShowLoginPanel();
            SetStatus(loginStatusText, "Debes iniciar sesión antes de jugar", true);
            return;
        }

        PlayFabLeaderboardManager.Instance.HideLeaderboard();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void StartGuestGame()
    {
        PlayFabLeaderboardManager.Instance?.StartGuestSession();
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowLeaderboardPanel()
    {
        PlayFabLeaderboardManager manager = PlayFabLeaderboardManager.Instance;
        if (manager == null || !manager.IsLoggedIn)
        {
            ShowLoginPanel();
            SetStatus(loginStatusText, "Inicia sesión para consultar las puntuaciones", true);
            return;
        }

        ShowOnly(panelLeaderboard);
        manager.HideLeaderboard();
        SetStatus(leaderboardStatusText, "Actualizando puntuaciones...", false);
        ClearGeneratedRows();

        PlayerProfileViewConstraints profile = new PlayerProfileViewConstraints
        {
            ShowDisplayName = true
        };

        PlayFabClientAPI.GetLeaderboard(
            new GetLeaderboardRequest
            {
                StatisticName = statisticName,
                StartPosition = 0,
                MaxResultsCount = leaderboardSize,
                ProfileConstraints = profile
            },
            result =>
            {
                foreach (PlayerLeaderboardEntry entry in result.Leaderboard)
                {
                    CreateLeaderboardRow(entry, manager.CurrentPlayFabId);
                }

                LoadCurrentPlayer(profile, manager.CurrentPlayFabId);
            },
            error => HandleLeaderboardError(error));
    }

    public void RefreshLeaderboard()
    {
        ShowLeaderboardPanel();
    }

    private void CompleteAuthentication(string playFabId, string displayName)
    {
        requestInProgress = false;
        PlayFabLeaderboardManager.Instance?.SetAuthenticatedPlayer(playFabId, displayName);
        ShowMainPanel();
    }

    private void LoadCurrentPlayer(PlayerProfileViewConstraints profile, string currentPlayFabId)
    {
        PlayFabClientAPI.GetLeaderboardAroundPlayer(
            new GetLeaderboardAroundPlayerRequest
            {
                StatisticName = statisticName,
                MaxResultsCount = 1,
                ProfileConstraints = profile
            },
            result =>
            {
                if (currentPlayerRow != null)
                {
                    bool hasPlayer = result.Leaderboard.Count > 0;
                    currentPlayerRow.gameObject.SetActive(hasPlayer);
                    if (hasPlayer)
                    {
                        PlayerLeaderboardEntry player = result.Leaderboard[0];
                        currentPlayerRow.Configure(
                            player.Position,
                            player.DisplayName,
                            player.StatValue,
                            player.PlayFabId,
                            true);
                    }
                }

                SetStatus(leaderboardStatusText, "Mejores " + leaderboardSize + " puntuaciones", false);
            },
            error => HandleLeaderboardError(error));
    }

    private void CreateLeaderboardRow(PlayerLeaderboardEntry entry, string currentPlayFabId)
    {
        if (leaderboardRowTemplate == null || leaderboardContent == null)
        {
            SetStatus(leaderboardStatusText, "Falta asignar el template o contenedor en el Inspector", true);
            return;
        }

        LeaderboardRowView row = Instantiate(leaderboardRowTemplate, leaderboardContent);
        row.gameObject.SetActive(true);
        row.Configure(
            entry.Position,
            entry.DisplayName,
            entry.StatValue,
            entry.PlayFabId,
            entry.PlayFabId == currentPlayFabId);
        generatedRows.Add(row);
    }

    private void ClearGeneratedRows()
    {
        foreach (LeaderboardRowView row in generatedRows)
        {
            if (row != null)
            {
                Destroy(row.gameObject);
            }
        }

        generatedRows.Clear();
    }

    private void ShowOnly(GameObject target)
    {
        SetPanelActive(panelInitial, target);
        SetPanelActive(panelLogin, target);
        SetPanelActive(panelCreateAccount, target);
        SetPanelActive(panelMain, target);
        SetPanelActive(panelLeaderboard, target);
    }

    private static void SetPanelActive(GameObject panel, GameObject target)
    {
        if (panel != null)
        {
            panel.SetActive(panel == target);
        }
    }

    private void HandleAuthError(PlayFabError error, TMP_Text statusText)
    {
        requestInProgress = false;
        SetStatus(statusText, FriendlyError(error), true);
        Debug.LogError(error.GenerateErrorReport(), this);
    }

    private void HandleLeaderboardError(PlayFabError error)
    {
        SetStatus(leaderboardStatusText, "PlayFab: " + error.ErrorMessage, true);
        Debug.LogError(error.GenerateErrorReport(), this);
    }

    private static string FriendlyError(PlayFabError error)
    {
        switch (error.Error.ToString())
        {
            case "InvalidUsernameOrPassword":
            case "InvalidEmailOrPassword":
                return "Usuario, correo o contraseña incorrectos";
            case "UsernameNotAvailable":
                return "Ese nombre de usuario ya está ocupado";
            case "EmailAddressNotAvailable":
                return "Ese correo ya tiene una cuenta";
            case "InvalidEmailAddress":
                return "El correo no es válido";
            case "InvalidPassword":
                return "La contraseña no cumple los requisitos";
            case "InvalidUsername":
                return "El nombre de usuario no es válido";
            default:
                return "PlayFab: " + error.ErrorMessage;
        }
    }

    private static void SetStatus(TMP_Text statusText, string message, bool isError)
    {
        if (statusText == null)
        {
            return;
        }

        statusText.text = message;
        statusText.color = isError ? new Color(1f, 0.35f, 0.35f) : Color.white;
    }
}
