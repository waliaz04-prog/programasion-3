using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [SerializeField] private float gameTime = 60f;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject endMessage;      
    [SerializeField] private MovimientoPlayer player;
    [SerializeField] private CamaraController camara;

    private int score;
    private bool gameEnded;
    private Rigidbody playerRigidbody;
    private readonly WaitForSeconds oneSecond = new WaitForSeconds(1f); 

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        playerRigidbody = player != null ? player.GetComponent<Rigidbody>() : null;
        endMessage.SetActive(false);
        UpdateScoreText();
        StartCoroutine(TimerIE());
    }

    public void AddScore(int points)
    {
        if (gameEnded)
        {
            return;
        }

        score += points;
        UpdateScoreText(); 
    }

    private void UpdateScoreText()
    {
        scoreText.text = "Puntos: " + score;
    }

    private IEnumerator TimerIE()
    {
        float timeLeft = gameTime;
        while (timeLeft > 0)
        {
            timerText.text = "Tiempo: " + Mathf.CeilToInt(timeLeft);
            yield return oneSecond;
            timeLeft--;
        }
        timerText.text = "Tiempo: 0";
        EndGame();
    }

    private void EndGame()
    {
        gameEnded = true;
        endMessage.SetActive(true);

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
        }

        if (player != null)
        {
            player.enabled = false;
        }

        if (camara != null)
        {
            camara.enabled = false;
        }

        CollectablesSpawn.Instance?.StopSpawning();
        PlayFabLeaderboardManager leaderboardManager = PlayFabLeaderboardManager.Instance;
        bool scoreWillBeSaved = leaderboardManager != null && leaderboardManager.IsLoggedIn;
        if (scoreWillBeSaved)
        {
            leaderboardManager.SubmitScore(score);
            leaderboardManager.ShowLeaderboard();
        }
        else
        {
            leaderboardManager?.SubmitScore(score);
        }

        TMP_Text endText = endMessage.GetComponentInChildren<TMP_Text>(true);
        if (endText != null)
        {
            endText.text = scoreWillBeSaved
                ? $"Partida terminada\nPuntos: {score}\nGuardando récord en PlayFab..."
                : $"Partida terminada\nPuntos: {score}\nModo invitado: resultado no guardado";
        }

        StartCoroutine(ReturnToMenuIE());
    }

    private IEnumerator ReturnToMenuIE()
    {
        yield return new WaitForSecondsRealtime(2f);
        SceneManager.LoadScene(ExamConfiguration.MenuSceneName);
    }
}
