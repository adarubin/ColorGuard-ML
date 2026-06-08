using System.Collections;
using System.IO; 
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking; 

[System.Serializable]
public class WaveData
{
    public float ml_BallSpeed;
    public float ml_WaveInterval;
    public int ml_BallsPerWave;
    public float ml_WaveDelay;
    public int ml_DistractionCount;
    public float ml_DistractionSpeed;
    public int ml_CubeMode;
    public float ml_BallSize;
    public bool ml_ColorShift;
    public float ml_SpawnDistance;
    public float expected_success_rate;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; 

    [Header("Prefabs & Objects")]
    public GameObject ballPrefab; 
    public GameObject distractionPrefab; 
    public GameObject playerLeft;        
    public GameObject playerRight;       
    public Camera mainCamera;            // המשתנה החדש עבור הבהוב הרקע

    private Color[] colors = new Color[4] { Color.red, Color.blue, Color.green, Color.yellow };
    private Vector3[] spawnPoints = new Vector3[4] {
        new Vector3(0, 10f, 0), new Vector3(0, -10f, 0),
        new Vector3(10f, 0, 0), new Vector3(-10f, 0, 0)
    };
    
    // מניעת חפיפות בנקודות השיגור
    private int lastSpawnIndex = -1;

    [Header("UI Elements")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI countdownText; 
    public GameObject playAgainButton; 
    public GameObject mainMenuButton;  

    [Header("Audio")]
    public AudioSource sfxSource;       
    public AudioClip correctSoundClip;  
    public AudioClip wrongSoundClip;    
    public AudioClip gameOverSoundClip; 

    [Header("Game State")]
    public int score = 0;
    public int lives = 3;
    public bool isGameOver = false;
    private bool isGameStarted = false; 

    [Header("DDA Parameters (From API)")]
    public float ml_BallSpeed = 3f;             
    public float ml_WaveInterval = 2.5f;        
    public int ml_BallsPerWave = 1;             
    public float ml_WaveDelay = 0.5f;           
    public int ml_DistractionCount = 0;         
    public float ml_DistractionSpeed = 2f;      
    public int ml_CubeMode = 0;                 
    public float ml_BallSize = 1f;              
    public bool ml_ColorShift = false;          
    public float ml_SpawnDistance = 10f;        
    public bool simulateML = true; 

    private string csvFilePath;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        if (playAgainButton != null) playAgainButton.SetActive(false);
        if (mainMenuButton != null) mainMenuButton.SetActive(false);
        if (gameOverText != null) gameOverText.gameObject.SetActive(false);
        
        UpdateCubeVisibility();

        csvFilePath = Application.dataPath + "/ML_GameData.csv";
        if (!File.Exists(csvFilePath))
        {
            File.WriteAllText(csvFilePath, "BallSpeed,WaveInterval,BallsPerWave,WaveDelay,DistractionCount,DistractionSpeed,CubeMode,BallSize,ColorShift,SpawnDistance,SuccessTarget\n");
        }

        UpdateUI();
        StartCoroutine(StartCountdownRoutine()); 
    }

    void Update()
    {
        if (isGameStarted && !isGameOver) UpdateCubeVisibility();
    }

    void UpdateCubeVisibility()
    {
        if (playerLeft == null || playerRight == null) return;
        
        if (ml_CubeMode == 0)      { playerLeft.SetActive(true);  playerRight.SetActive(false); }
        else if (ml_CubeMode == 1) { playerLeft.SetActive(false); playerRight.SetActive(true);  }
        else if (ml_CubeMode == 2) { playerLeft.SetActive(true);  playerRight.SetActive(true);  }
    }

    IEnumerator StartCountdownRoutine()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "3"; yield return new WaitForSeconds(1f); 
            countdownText.text = "2"; yield return new WaitForSeconds(1f);
            countdownText.text = "1"; yield return new WaitForSeconds(1f);
            countdownText.text = "GO!"; yield return new WaitForSeconds(1f);
            countdownText.gameObject.SetActive(false);
        }
        else yield return new WaitForSeconds(3f);
        
        isGameStarted = true; 
        StartCoroutine(WaveSpawner());
    }

    IEnumerator WaveSpawner()
    {
        while (!isGameOver)
        {
            if (simulateML) 
            {
                yield return StartCoroutine(GetWaveFromAPI());
                UpdateCubeVisibility(); 
            }

            if (distractionPrefab != null)
            {
                GameObject[] oldDistractions = GameObject.FindGameObjectsWithTag("Distraction");
                foreach (GameObject dist in oldDistractions) Destroy(dist);

                for (int i = 0; i < ml_DistractionCount; i++)
                {
                    Instantiate(distractionPrefab, new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f), 0), Quaternion.identity);
                }
            }

            lastSpawnIndex = -1;

            for (int i = 0; i < ml_BallsPerWave; i++)
            {
                if (isGameOver) break;
                SpawnBall();
                yield return new WaitForSeconds(ml_WaveDelay); 
            }
            
            yield return new WaitForSeconds(ml_WaveInterval);
            
            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Ball").Length == 0);
        }
    }

    IEnumerator GetWaveFromAPI()
    {
        string url = "http://127.0.0.1:8000/get_wave";
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("API Error: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                WaveData wave = JsonUtility.FromJson<WaveData>(jsonResponse);

                ml_BallSpeed = wave.ml_BallSpeed;
                ml_WaveInterval = wave.ml_WaveInterval;
                ml_BallsPerWave = wave.ml_BallsPerWave;
                ml_WaveDelay = wave.ml_WaveDelay;
                ml_DistractionCount = wave.ml_DistractionCount;
                ml_DistractionSpeed = wave.ml_DistractionSpeed;
                ml_CubeMode = wave.ml_CubeMode;
                ml_BallSize = wave.ml_BallSize;
                ml_ColorShift = wave.ml_ColorShift;
                ml_SpawnDistance = wave.ml_SpawnDistance;

                Debug.Log($"API Decision -> Speed: {ml_BallSpeed:F1}, Balls: {ml_BallsPerWave}, Expected Success: {wave.expected_success_rate:F2}");
            }
        }
    }

    void SpawnBall()
    {
        Transform targetCube = null;

        if      (ml_CubeMode == 2) targetCube = Random.value > 0.5f ? playerLeft.transform : playerRight.transform;
        else if (ml_CubeMode == 0) targetCube = playerLeft.transform;
        else if (ml_CubeMode == 1) targetCube = playerRight.transform;

        if (targetCube == null) return;

        int newSpawnIndex;
        do
        {
            newSpawnIndex = Random.Range(0, spawnPoints.Length);
        } while (newSpawnIndex == lastSpawnIndex);

        lastSpawnIndex = newSpawnIndex;

        Vector3 spawnPos = targetCube.position + (spawnPoints[newSpawnIndex].normalized * ml_SpawnDistance);
        GameObject newBall = Instantiate(ballPrefab, spawnPos, Quaternion.identity);
        
        var ballMovement = newBall.GetComponent<BallMovement>();
        if (ballMovement != null)
        {
            ballMovement.speed = ml_BallSpeed;
            ballMovement.targetPosition = targetCube.position;
            ballMovement.canShiftColor = ml_ColorShift; 
        }
        
        newBall.transform.localScale = new Vector3(ml_BallSize, ml_BallSize, 1f);
        var sr = newBall.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = colors[Random.Range(0, colors.Length)];
    }

    void SaveDataToCSV(bool success)
    {
        File.AppendAllText(csvFilePath, 
            $"{ml_BallSpeed},{ml_WaveInterval},{ml_BallsPerWave},{ml_WaveDelay}," +
            $"{ml_DistractionCount},{ml_DistractionSpeed},{ml_CubeMode},{ml_BallSize}," +
            $"{(ml_ColorShift ? 1 : 0)},{ml_SpawnDistance},{(success ? 1 : 0)}\n");
    }

    public void AddScore(int amount)
    {
        if (isGameOver) return;
        score += amount;
        UpdateUI();
        PlaySound(correctSoundClip);
        SaveDataToCSV(true);
    }

    public void LoseLife()
    {
        if (isGameOver) return;
        lives--;
        UpdateUI();
        PlaySound(wrongSoundClip);
        SaveDataToCSV(false);
        
        // אפקט הבהוב המסך כשיורד חיים
        if (mainCamera != null)
        {
            StartCoroutine(FlashBackground());
        }

        if (lives <= 0) GameOver();
    }

    // קורוטינה שמהבהבת את המסך לאדום באפקט קצר
    IEnumerator FlashBackground()
    {
        Color originalColor = mainCamera.backgroundColor;
        mainCamera.backgroundColor = new Color(0.5f, 0f, 0f); // אדום כהה
        yield return new WaitForSeconds(0.1f);
        mainCamera.backgroundColor = originalColor;
    }

    void UpdateUI()
    {
        if (scoreText != null) scoreText.text = "Score: " + score;
        if (livesText  != null) livesText.text  = "Lives: "  + lives;
    }

    void GameOver()
    {
        isGameOver = true;
        if (gameOverText    != null) { gameOverText.gameObject.SetActive(true); gameOverText.text = "GAME OVER\nScore: " + score; }
        if (playAgainButton != null) playAgainButton.SetActive(true);
        if (mainMenuButton  != null) mainMenuButton.SetActive(true);
        
        PlaySound(gameOverSoundClip);
    }

    public void PlayAgain()    { SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
    public void LoadMainMenu() { SceneManager.LoadScene("MainMenu"); }
    
    // פונקציית צלילים מעודכנת עם שינוי קל ואקראי בגובה הצליל בכל פגיעה
    void PlaySound(AudioClip clip) 
    { 
        if (sfxSource != null && clip != null) 
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip); 
        }
    }
}