using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Main Menu Elements")]
    public TMP_InputField nameInputField; 
    
    [Header("Leaderboard Elements")]
    public GameObject leaderboardPanel; 
    public TextMeshProUGUI leftScoresText;  
    public TextMeshProUGUI rightScoresText; 

    private void Start()
    {
        leaderboardPanel.SetActive(false);
    }

    public void StartGame()
    {
        string playerName = nameInputField.text;
        if (string.IsNullOrEmpty(playerName)) playerName = "Guest";

        PlayerPrefs.SetString("PlayerName", playerName);
        PlayerPrefs.Save(); 

        SceneManager.LoadScene("SampleScene"); 
    }

    public void OpenLeaderboard()
    {
        leaderboardPanel.SetActive(true);
        
        string leftText = "TOP 1-5\n\n";
        string rightText = "TOP 6-10\n\n";
        
        for(int i = 1; i <= 10; i++) 
        {
            string name = PlayerPrefs.GetString("HighScoreName" + i, "---");
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            
            if (i <= 5) {
                leftText += i + ". " + name + " - " + score + "\n";
            } else {
                rightText += i + ". " + name + " - " + score + "\n";
            }
        }
        
        leftScoresText.text = leftText;
        rightScoresText.text = rightText;
    }

    public void CloseLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }
}