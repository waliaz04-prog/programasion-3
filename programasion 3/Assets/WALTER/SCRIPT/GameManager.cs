using System.Collections;
using UnityEngine;
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
        endMessage.SetActive(false);
        UpdateScoreText();
        StartCoroutine(TimerIE());
    }

    public void AddScore(int points)
    {
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
        endMessage.SetActive(true);

        player.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
        player.enabled = false;
        camara.enabled = false;
    }
}
