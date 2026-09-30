using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Paneles de la Interfaz (UI)")]
    public GameObject mainMenuPanel;          // Panel del Menú Principal
    public GameObject scanInstructionsPanel;  // Panel de Escaneo AR
    public GameObject hudPanel;               // Panel en juego (Corazones, Paquetes, Tiempo)
    public GameObject gameOverPanel;          // Panel al perder
    public GameObject comingSoonPanel;        // Panel "Próximamente"
    public GameObject settingsPanel;          // Panel de Configuración
    public GameObject instructionsPanel;      // Panel de Instrucciones / Guía

    [Header("Componentes AR (Control de Escaneo)")]
    public ARPlaneManager planeManager;

    [Header("Generadores del Juego (Spawners)")]
    public GameObject[] gameSpawners;

    [Header("Configuración de Vidas")]
    public int maxLives = 6;
    public int currentLives;

    [Header("Estadísticas de la Partida Actual")]
    public int packagesDelivered = 0;
    public float survivalTime = 0f;

    [Header("Referencias HUD (En Juego)")]
    public Image[] heartIcons;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;

    [Header("Referencias GameOverPanel")]
    public TextMeshProUGUI gameOverScoreText;
    public TextMeshProUGUI gameOverTimeText;
    public TextMeshProUGUI bestScoreText;
    public TextMeshProUGUI bestTimeText;

    private bool isGameActive = false;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        if (isGameActive)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // --- GESTIÓN DE MENÚ Y ESTADOS ---

    public void ShowMainMenu()
    {
        isGameActive = false;
        Time.timeScale = 1f;

        ToggleSpawners(false);
        ToggleARScanning(false);

        // Limpiar elementos de la escena si volvemos desde el GameOver
        ClearSceneEntities();

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (comingSoonPanel != null) comingSoonPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    public void ShowScanInstructions()
    {
        isGameActive = false;
        ToggleSpawners(false);
        ToggleARScanning(true);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(true);
    }

    public void StartGame()
    {
        currentLives = maxLives;
        packagesDelivered = 0;
        survivalTime = 0f;
        isGameActive = true;
        Time.timeScale = 1f;

        ToggleSpawners(true);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        UpdateHeartsUI();
        UpdateScoreUI();
        UpdateTimerUI();
    }

    void ToggleSpawners(bool state)
    {
        if (gameSpawners == null) return;

        foreach (GameObject spawner in gameSpawners)
        {
            if (spawner != null)
            {
                spawner.SetActive(state);
            }
        }
    }

    void ToggleARScanning(bool state)
    {
        if (planeManager != null)
        {
            planeManager.enabled = state;

            foreach (var plane in planeManager.trackables)
            {
                plane.gameObject.SetActive(state);
            }
        }
    }

    // --- GAMEPLAY ---

    public void LoseLife()
    {
        if (!isGameActive) return;

        currentLives--;
        UpdateHeartsUI();

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void AddDelivery()
    {
        if (!isGameActive) return;

        packagesDelivered++;
        UpdateScoreUI();
    }

    void UpdateHeartsUI()
    {
        if (heartIcons == null) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                heartIcons[i].enabled = i < currentLives;
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Paquetes: " + packagesDelivered;
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60F);
            int seconds = Mathf.FloorToInt(survivalTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    void GameOver()
    {
        isGameActive = false;
        ToggleSpawners(false);

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        // --- SISTEMA DE RÉCORDS ---
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (packagesDelivered > bestScore)
        {
            bestScore = packagesDelivered;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        if (bestTime == 0f || (survivalTime < bestTime && packagesDelivered >= bestScore))
        {
            bestTime = survivalTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
        }

        PlayerPrefs.Save();

        // --- FORMATO DE TIEMPOS ---
        int curMin = Mathf.FloorToInt(survivalTime / 60F);
        int curSec = Mathf.FloorToInt(survivalTime % 60F);
        int bestMin = Mathf.FloorToInt(bestTime / 60F);
        int bestSec = Mathf.FloorToInt(bestTime % 60F);

        if (gameOverScoreText != null) gameOverScoreText.text = "Paquetes: " + packagesDelivered;
        if (gameOverTimeText != null) gameOverTimeText.text = "Tiempo: " + string.Format("{0:00}:{1:00}", curMin, curSec);
        if (bestScoreText != null) bestScoreText.text = "Mejor Puntaje: " + bestScore;
        if (bestTimeText != null) bestTimeText.text = "Mejor Tiempo: " + string.Format("{0:00}:{1:00}", bestMin, bestSec);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        ClearSceneEntities();
        StartGame();
    }

    // Método auxiliar para limpiar los elementos del juego
    private void ClearSceneEntities()
    {
        PackageBehaviour[] packages = FindObjectsOfType<PackageBehaviour>();
        foreach (PackageBehaviour p in packages)
        {
            Destroy(p.gameObject);
        }

        GameObject[] rivals = GameObject.FindGameObjectsWithTag("Rival");
        foreach (GameObject r in rivals)
        {
            Destroy(r);
        }
    }

    // --- MÉTODOS DE PANELES (CONFIGURACIÓN Y TUTORIAL) ---

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void OpenInstructions()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(true);
    }

    public void CloseInstructions()
    {
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    public void ShowComingSoon()
    {
        if (comingSoonPanel != null) comingSoonPanel.SetActive(true);
    }

    public void CloseComingSoon()
    {
        if (comingSoonPanel != null) comingSoonPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}