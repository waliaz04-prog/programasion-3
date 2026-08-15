using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardRowView : MonoBehaviour
{
    [Header("Textos de la fila")]
    [SerializeField] private TMP_Text positionText;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Avatar seguro")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text avatarInitialText;
    [SerializeField] private GameObject currentPlayerIndicator;

    public void Configure(int position, string playerName, int score, string playFabId, bool isCurrentPlayer)
    {
        string safeName = string.IsNullOrWhiteSpace(playerName) ? "Jugador" : playerName;

        if (positionText != null)
        {
            positionText.text = "#" + (position + 1);
        }

        if (playerNameText != null)
        {
            playerNameText.text = safeName;
        }

        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }

        if (avatarImage != null)
        {
            avatarImage.color = CreateAvatarColor(playFabId);
        }

        if (avatarInitialText != null)
        {
            avatarInitialText.text = safeName.Substring(0, 1).ToUpperInvariant();
        }

        if (currentPlayerIndicator != null)
        {
            currentPlayerIndicator.SetActive(isCurrentPlayer);
        }
    }

    private static Color CreateAvatarColor(string id)
    {
        int hash = string.IsNullOrWhiteSpace(id) ? 1 : id.GetHashCode();
        float hue = Mathf.Abs(hash % 1000) / 1000f;
        return Color.HSVToRGB(hue, 0.62f, 0.92f);
    }
}
