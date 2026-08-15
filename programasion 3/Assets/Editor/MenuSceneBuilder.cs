using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MenuSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Menu.unity";
    private const string RequestPath = "Assets/Editor/RebuildMenu.request";

    private static readonly Color Background = new Color(0.025f, 0.045f, 0.085f, 1f);
    private static readonly Color BackgroundAccent = new Color(0.04f, 0.20f, 0.28f, 0.78f);
    private static readonly Color Card = new Color(0.055f, 0.085f, 0.13f, 0.97f);
    private static readonly Color CardSecondary = new Color(0.075f, 0.115f, 0.17f, 1f);
    private static readonly Color Primary = new Color(0.10f, 0.72f, 0.78f, 1f);
    private static readonly Color Secondary = new Color(0.18f, 0.30f, 0.40f, 1f);
    private static readonly Color TextMain = new Color(0.94f, 0.98f, 1f, 1f);
    private static readonly Color TextMuted = new Color(0.60f, 0.72f, 0.80f, 1f);

    static MenuSceneBuilder()
    {
        EditorApplication.delayCall += TryAutomaticBuild;
    }

    [MenuItem("Tools/Examen/Reconstruir escena Menu")]
    public static void BuildMenuScene()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Deten Play antes de reconstruir la escena Menu.");
            return;
        }

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject canvasObject = new GameObject(
            "Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster),
            typeof(MenuInspectorController));
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.pixelPerfect = false;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        Image background = CreateImage("Fondo", canvasRect, Background);
        Stretch(background.rectTransform);
        Image accentTop = CreateImage("Brillo Superior", background.rectTransform, BackgroundAccent);
        SetRect(accentTop.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(1920f, 420f), new Vector2(0f, -100f));
        Image accentBottom = CreateImage("Brillo Inferior", background.rectTransform, new Color(0.15f, 0.055f, 0.20f, 0.42f));
        SetRect(accentBottom.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(1920f, 300f), new Vector2(0f, 30f));

        GameObject initialPanel = CreatePanel("PanelEleccion", canvasRect);
        RectTransform initialCard = CreateCard(initialPanel.transform, new Vector2(780f, 620f));
        CreateText("Titulo", initialCard, "TICKET RUSH", 54f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain,
            new Vector2(700f, 80f), new Vector2(0f, 195f));
        CreateText("Subtitulo", initialCard, "Supera tu récord en 60 segundos", 25f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted,
            new Vector2(680f, 52f), new Vector2(0f, 130f));
        CreateText("Descripcion", initialCard, "Recoge tickets, suma puntos y compite en el leaderboard de PlayFab.", 20f, FontStyles.Normal, TextAlignmentOptions.Center, TextMain,
            new Vector2(650f, 75f), new Vector2(0f, 65f));
        Button initialLoginButton = CreateButton("BotonAcceder", initialCard, "INICIAR SESIÓN / REGISTRARSE", Primary,
            new Vector2(480f, 74f), new Vector2(0f, -45f));
        Button guestButton = CreateButton("BotonJugarInvitado", initialCard, "JUGAR SIN CUENTA", Secondary,
            new Vector2(480f, 68f), new Vector2(0f, -135f));
        CreateText("Nota", initialCard, "Como invitado puedes jugar, pero tu resultado no se guardará\ny no podrás consultar el leaderboard.", 17f, FontStyles.Italic, TextAlignmentOptions.Center, TextMuted,
            new Vector2(660f, 60f), new Vector2(0f, -225f));

        GameObject loginPanel = CreatePanel("PanelLogin", canvasRect);
        RectTransform loginCard = CreateCard(loginPanel.transform, new Vector2(800f, 650f));
        CreateText("Titulo", loginCard, "INICIAR SESIÓN", 42f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain,
            new Vector2(700f, 70f), new Vector2(0f, 245f));
        CreateText("EtiquetaUsuario", loginCard, "Usuario o correo", 20f, FontStyles.Bold, TextAlignmentOptions.Left, TextMain,
            new Vector2(600f, 34f), new Vector2(0f, 155f));
        TMP_InputField loginIdentity = CreateInput("InputUsuarioCorreo", loginCard, "Escribe tu usuario o correo", TMP_InputField.ContentType.Standard,
            new Vector2(600f, 62f), new Vector2(0f, 105f));
        CreateText("EtiquetaContrasena", loginCard, "Contraseña", 20f, FontStyles.Bold, TextAlignmentOptions.Left, TextMain,
            new Vector2(600f, 34f), new Vector2(0f, 35f));
        TMP_InputField loginPassword = CreateInput("InputContrasena", loginCard, "Escribe tu contraseña", TMP_InputField.ContentType.Password,
            new Vector2(600f, 62f), new Vector2(0f, -15f));
        TMP_Text loginStatus = CreateText("TextoEstado", loginCard, "Ingresa tus datos de PlayFab", 17f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted,
            new Vector2(650f, 44f), new Vector2(0f, -80f));
        Button loginSubmit = CreateButton("BotonEntrar", loginCard, "INICIAR SESIÓN", Primary,
            new Vector2(285f, 62f), new Vector2(157f, -155f));
        Button loginRegister = CreateButton("BotonCrearCuenta", loginCard, "CREAR CUENTA", Secondary,
            new Vector2(285f, 62f), new Vector2(-157f, -155f));
        Button loginBack = CreateButton("BotonRegresar", loginCard, "REGRESAR", new Color(0.12f, 0.18f, 0.25f, 1f),
            new Vector2(250f, 54f), new Vector2(0f, -235f));

        GameObject createPanel = CreatePanel("PanelCrearCuenta", canvasRect);
        RectTransform createCard = CreateCard(createPanel.transform, new Vector2(820f, 820f));
        CreateText("Titulo", createCard, "CREAR CUENTA", 42f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain,
            new Vector2(700f, 70f), new Vector2(0f, 330f));
        CreateText("EtiquetaCorreo", createCard, "Correo", 19f, FontStyles.Bold, TextAlignmentOptions.Left, TextMain,
            new Vector2(610f, 32f), new Vector2(0f, 245f));
        TMP_InputField registerEmail = CreateInput("InputCorreo", createCard, "nombre@correo.com", TMP_InputField.ContentType.EmailAddress,
            new Vector2(610f, 60f), new Vector2(0f, 198f));
        CreateText("EtiquetaUsuario", createCard, "Usuario (3 a 20 caracteres)", 19f, FontStyles.Bold, TextAlignmentOptions.Left, TextMain,
            new Vector2(610f, 32f), new Vector2(0f, 125f));
        TMP_InputField registerUser = CreateInput("InputUsuario", createCard, "Elige un nombre de usuario", TMP_InputField.ContentType.Standard,
            new Vector2(610f, 60f), new Vector2(0f, 78f));
        CreateText("EtiquetaContrasena", createCard, "Contraseña (mínimo 6 caracteres)", 19f, FontStyles.Bold, TextAlignmentOptions.Left, TextMain,
            new Vector2(610f, 32f), new Vector2(0f, 5f));
        TMP_InputField registerPassword = CreateInput("InputContrasena", createCard, "Crea una contraseña", TMP_InputField.ContentType.Password,
            new Vector2(610f, 60f), new Vector2(0f, -42f));
        TMP_Text registerStatus = CreateText("TextoEstado", createCard, "Los datos se registrarán directamente en PlayFab", 17f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted,
            new Vector2(680f, 48f), new Vector2(0f, -112f));
        Button registerSubmit = CreateButton("BotonRegistrar", createCard, "REGISTRAR CUENTA", Primary,
            new Vector2(360f, 64f), new Vector2(0f, -190f));
        Button registerBack = CreateButton("BotonRegresar", createCard, "YA TENGO CUENTA", Secondary,
            new Vector2(320f, 56f), new Vector2(0f, -270f));

        GameObject mainPanel = CreatePanel("PanelPrincipal", canvasRect);
        RectTransform mainCard = CreateCard(mainPanel.transform, new Vector2(800f, 560f));
        CreateText("Titulo", mainCard, "MENÚ PRINCIPAL", 46f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain,
            new Vector2(700f, 72f), new Vector2(0f, 175f));
        CreateText("EstadoSesion", mainCard, "Sesión de PlayFab iniciada", 20f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted,
            new Vector2(650f, 42f), new Vector2(0f, 112f));
        Button playButton = CreateButton("BotonJugar", mainCard, "JUGAR 60 SEGUNDOS", Primary,
            new Vector2(500f, 80f), new Vector2(0f, 20f));
        Button leaderboardButton = CreateButton("BotonPuntuaciones", mainCard, "VER PUNTUACIONES", Secondary,
            new Vector2(500f, 72f), new Vector2(0f, -82f));
        CreateText("Ayuda", mainCard, "Durante la partida el clima cambiará según una de 10 ciudades.", 17f, FontStyles.Italic, TextAlignmentOptions.Center, TextMuted,
            new Vector2(680f, 55f), new Vector2(0f, -175f));

        GameObject leaderboardPanel = CreatePanel("PanelLeaderboard", canvasRect);
        RectTransform leaderboardCard = CreateCard(leaderboardPanel.transform, new Vector2(1100f, 900f));
        CreateText("Titulo", leaderboardCard, "MEJORES PUNTUACIONES", 42f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain,
            new Vector2(900f, 65f), new Vector2(0f, 380f));
        TMP_Text leaderboardStatus = CreateText("TextoEstado", leaderboardCard, "Actualizando puntuaciones...", 17f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted,
            new Vector2(900f, 36f), new Vector2(0f, 330f));

        RectTransform scrollRoot = CreateRect("ListaPuntuaciones", leaderboardCard);
        SetRect(scrollRoot, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(930f, 490f), new Vector2(0f, 55f));
        Image scrollBackground = scrollRoot.gameObject.AddComponent<Image>();
        scrollBackground.color = new Color(0.025f, 0.045f, 0.07f, 0.72f);
        scrollRoot.gameObject.AddComponent<RectMask2D>();
        ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.scrollSensitivity = 22f;

        RectTransform content = CreateRect("ContenedorPuestos", scrollRoot);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = new Vector2(0f, -12f);
        content.sizeDelta = new Vector2(-24f, 0f);
        VerticalLayoutGroup verticalLayout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        verticalLayout.padding = new RectOffset(8, 8, 8, 8);
        verticalLayout.spacing = 8f;
        verticalLayout.childAlignment = TextAnchor.UpperCenter;
        verticalLayout.childControlWidth = true;
        verticalLayout.childControlHeight = false;
        verticalLayout.childForceExpandWidth = true;
        verticalLayout.childForceExpandHeight = false;
        ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = content;
        scrollRect.viewport = scrollRoot;

        LeaderboardRowView rowTemplate = CreateLeaderboardRow("PuestoTemplate", content, false);
        rowTemplate.gameObject.SetActive(false);

        CreateText("EtiquetaJugador", leaderboardCard, "TU POSICIÓN", 17f, FontStyles.Bold, TextAlignmentOptions.Left, Primary,
            new Vector2(900f, 30f), new Vector2(0f, -215f));
        LeaderboardRowView currentPlayerRow = CreateLeaderboardRow("MiPuesto", leaderboardCard, true);
        SetRect(currentPlayerRow.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(900f, 64f), new Vector2(0f, -265f));
        Button refreshButton = CreateButton("BotonActualizar", leaderboardCard, "ACTUALIZAR", Primary,
            new Vector2(260f, 56f), new Vector2(155f, -360f));
        Button leaderboardBack = CreateButton("BotonRegresar", leaderboardCard, "REGRESAR", Secondary,
            new Vector2(260f, 56f), new Vector2(-155f, -360f));

        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        eventSystemObject.transform.SetAsLastSibling();

        MenuInspectorController controller = canvasObject.GetComponent<MenuInspectorController>();
        SerializedObject controllerObject = new SerializedObject(controller);
        SetReference(controllerObject, "panelInitial", initialPanel);
        SetReference(controllerObject, "panelLogin", loginPanel);
        SetReference(controllerObject, "panelCreateAccount", createPanel);
        SetReference(controllerObject, "panelMain", mainPanel);
        SetReference(controllerObject, "panelLeaderboard", leaderboardPanel);
        SetReference(controllerObject, "loginUserOrEmailInput", loginIdentity);
        SetReference(controllerObject, "loginPasswordInput", loginPassword);
        SetReference(controllerObject, "loginStatusText", loginStatus);
        SetReference(controllerObject, "registerEmailInput", registerEmail);
        SetReference(controllerObject, "registerUsernameInput", registerUser);
        SetReference(controllerObject, "registerPasswordInput", registerPassword);
        SetReference(controllerObject, "registerStatusText", registerStatus);
        SetReference(controllerObject, "leaderboardContent", content);
        SetReference(controllerObject, "leaderboardRowTemplate", rowTemplate);
        SetReference(controllerObject, "currentPlayerRow", currentPlayerRow);
        SetReference(controllerObject, "leaderboardStatusText", leaderboardStatus);
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(initialLoginButton.onClick, controller.ShowLoginPanel);
        UnityEventTools.AddPersistentListener(guestButton.onClick, controller.StartGuestGame);
        UnityEventTools.AddPersistentListener(loginSubmit.onClick, controller.Login);
        UnityEventTools.AddPersistentListener(loginRegister.onClick, controller.ShowCreateAccountPanel);
        UnityEventTools.AddPersistentListener(loginBack.onClick, controller.ShowInitialPanel);
        UnityEventTools.AddPersistentListener(registerSubmit.onClick, controller.CreateAccount);
        UnityEventTools.AddPersistentListener(registerBack.onClick, controller.ShowLoginPanel);
        UnityEventTools.AddPersistentListener(playButton.onClick, controller.StartGame);
        UnityEventTools.AddPersistentListener(leaderboardButton.onClick, controller.ShowLeaderboardPanel);
        UnityEventTools.AddPersistentListener(refreshButton.onClick, controller.RefreshLeaderboard);
        UnityEventTools.AddPersistentListener(leaderboardBack.onClick, controller.ShowMainPanel);

        initialPanel.SetActive(true);
        loginPanel.SetActive(false);
        createPanel.SetActive(false);
        mainPanel.SetActive(false);
        leaderboardPanel.SetActive(false);

        // La escala del Canvas raíz debe permanecer en uno incluso al crear la escena en batch mode.
        canvasRect.localPosition = Vector3.zero;
        canvasRect.localRotation = Quaternion.identity;
        canvasRect.localScale = Vector3.one;
        EditorUtility.SetDirty(canvasRect);
        Canvas.ForceUpdateCanvases();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = canvasObject;
        Debug.Log("Escena Menu reconstruida y configurada correctamente.");
    }

    private static void TryAutomaticBuild()
    {
        string absoluteRequestPath = Path.Combine(Application.dataPath, "Editor", "RebuildMenu.request");
        if (!File.Exists(absoluteRequestPath))
        {
            return;
        }

        AssetDatabase.DeleteAsset(RequestPath);
        BuildMenuScene();
    }

    private static GameObject CreatePanel(string name, RectTransform parent)
    {
        RectTransform rect = CreateRect(name, parent);
        Stretch(rect);
        Image dim = rect.gameObject.AddComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.18f);
        return rect.gameObject;
    }

    private static RectTransform CreateCard(Transform parent, Vector2 size)
    {
        Image image = CreateImage("Tarjeta", parent, Card);
        SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, Vector2.zero);
        Outline outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(Primary.r, Primary.g, Primary.b, 0.45f);
        outline.effectDistance = new Vector2(2f, -2f);
        return image.rectTransform;
    }

    private static Button CreateButton(string name, Transform parent, string label, Color color, Vector2 size, Vector2 position)
    {
        Image image = CreateImage(name, parent, color);
        SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.08f, 1.08f, 1.08f, 1f);
        colors.pressedColor = new Color(0.78f, 0.82f, 0.86f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;
        TMP_Text text = CreateText("Texto", image.rectTransform, label, 20f, FontStyles.Bold, TextAlignmentOptions.Center, TextMain, size, Vector2.zero);
        Stretch(text.rectTransform);
        return button;
    }

    private static TMP_InputField CreateInput(string name, Transform parent, string placeholderText, TMP_InputField.ContentType contentType, Vector2 size, Vector2 position)
    {
        Image image = CreateImage(name, parent, new Color(0.10f, 0.145f, 0.20f, 1f));
        SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        Outline outline = image.gameObject.AddComponent<Outline>();
        outline.effectColor = new Color(0.28f, 0.45f, 0.55f, 0.7f);
        outline.effectDistance = new Vector2(1f, -1f);

        RectTransform viewport = CreateRect("AreaTexto", image.rectTransform);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.offsetMin = new Vector2(18f, 8f);
        viewport.offsetMax = new Vector2(-18f, -8f);
        viewport.gameObject.AddComponent<RectMask2D>();

        TMP_Text placeholder = CreateText("Placeholder", viewport, placeholderText, 19f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft,
            new Color(TextMuted.r, TextMuted.g, TextMuted.b, 0.72f), Vector2.zero, Vector2.zero);
        Stretch(placeholder.rectTransform);
        TMP_Text inputText = CreateText("Texto", viewport, string.Empty, 19f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft,
            TextMain, Vector2.zero, Vector2.zero);
        Stretch(inputText.rectTransform);

        TMP_InputField input = image.gameObject.AddComponent<TMP_InputField>();
        input.textViewport = viewport;
        input.textComponent = inputText;
        input.placeholder = placeholder;
        input.contentType = contentType;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = contentType == TMP_InputField.ContentType.Password ? 100 : 120;
        input.caretColor = Primary;
        input.selectionColor = new Color(Primary.r, Primary.g, Primary.b, 0.38f);
        input.targetGraphic = image;
        input.ForceLabelUpdate();
        return input;
    }

    private static LeaderboardRowView CreateLeaderboardRow(string name, Transform parent, bool currentPlayer)
    {
        Image background = CreateImage(name, parent, currentPlayer ? new Color(0.08f, 0.24f, 0.28f, 1f) : CardSecondary);
        RectTransform rowRect = background.rectTransform;
        rowRect.sizeDelta = new Vector2(0f, 64f);
        LayoutElement layout = background.gameObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 64f;
        layout.minHeight = 64f;

        Image avatar = CreateImage("AvatarSeguro", rowRect, new Color(0.3f, 0.7f, 0.8f, 1f));
        SetRect(avatar.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(46f, 46f), new Vector2(38f, 0f));
        TMP_Text initial = CreateText("InicialAvatar", avatar.rectTransform, "J", 22f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white,
            new Vector2(46f, 46f), Vector2.zero);
        Stretch(initial.rectTransform);
        TMP_Text position = CreateText("Posicion", rowRect, "#1", 20f, FontStyles.Bold, TextAlignmentOptions.Center, Primary,
            new Vector2(75f, 50f), new Vector2(105f, 0f));
        position.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        position.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        TMP_Text player = CreateText("NombreJugador", rowRect, "Jugador", 20f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, TextMain,
            new Vector2(520f, 50f), new Vector2(415f, 0f));
        player.rectTransform.anchorMin = new Vector2(0f, 0.5f);
        player.rectTransform.anchorMax = new Vector2(0f, 0.5f);
        TMP_Text score = CreateText("Puntuacion", rowRect, "0", 22f, FontStyles.Bold, TextAlignmentOptions.MidlineRight, TextMain,
            new Vector2(140f, 50f), new Vector2(-85f, 0f));
        score.rectTransform.anchorMin = new Vector2(1f, 0.5f);
        score.rectTransform.anchorMax = new Vector2(1f, 0.5f);

        Image indicator = CreateImage("IndicadorJugadorActual", rowRect, Primary);
        SetRect(indicator.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(58f, 30f), new Vector2(-195f, 0f));
        TMP_Text indicatorText = CreateText("Texto", indicator.rectTransform, "TÚ", 14f, FontStyles.Bold, TextAlignmentOptions.Center, Background,
            new Vector2(58f, 30f), Vector2.zero);
        Stretch(indicatorText.rectTransform);
        indicator.gameObject.SetActive(currentPlayer);

        LeaderboardRowView row = background.gameObject.AddComponent<LeaderboardRowView>();
        SerializedObject rowObject = new SerializedObject(row);
        SetReference(rowObject, "positionText", position);
        SetReference(rowObject, "playerNameText", player);
        SetReference(rowObject, "scoreText", score);
        SetReference(rowObject, "avatarImage", avatar);
        SetReference(rowObject, "avatarInitialText", initial);
        SetReference(rowObject, "currentPlayerIndicator", indicator.gameObject);
        rowObject.ApplyModifiedPropertiesWithoutUndo();
        return row;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, float fontSize, FontStyles style,
        TextAlignmentOptions alignment, Color color, Vector2 size, Vector2 position)
    {
        RectTransform rect = CreateRect(name, parent);
        SetRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, position);
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.enableWordWrapping = true;
        text.raycastTarget = false;
        return text;
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        RectTransform rect = CreateRect(name, parent);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject gameObject = new GameObject(name, typeof(RectTransform));
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.localScale = Vector3.one;
        return rect;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 position)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        rect.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    private static void SetReference(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogError("No se encontró la propiedad serializada: " + propertyName);
            return;
        }

        property.objectReferenceValue = value;
    }
}
