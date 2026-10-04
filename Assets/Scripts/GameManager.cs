using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using TMPro;

// Clase principal encargada de la gestión del estado del juego, flujo de la interfaz (UI),
// persistencia de récords (PlayerPrefs), ciclo de vida de la partida y control de AR.
public class GameManager : MonoBehaviour
{
    // Patrón Singleton para acceder a esta instancia desde cualquier otro script (ej. PackageBehaviour, CarManager)
    public static GameManager Instance;

    [Header("Paneles de la Interfaz (UI)")]
    public GameObject mainMenuPanel;          // Panel del Menú Principal
    public GameObject scanInstructionsPanel;  // Panel de Escaneo AR (Instrucciones para detectar el suelo)
    public GameObject hudPanel;               // Panel en juego (Muestra Corazones, Paquetes y Tiempo)
    public GameObject gameOverPanel;          // Panel de Fin de Juego (Resumen de partida y récords)
    public GameObject comingSoonPanel;        // Panel informativo para funciones en desarrollo
    public GameObject settingsPanel;          // Panel de Ajustes / Configuración
    public GameObject instructionsPanel;      // Panel de Instrucciones / Guía de juego

    [Header("Componentes AR (Control de Escaneo)")]
    // Administrador del sistema ARFoundation para detectar y activar/desactivar planos en la escena
    public ARPlaneManager planeManager;

    [Header("Generadores del Juego (Spawners)")]
    // Arreglo con los prefabs o GameObjects encargados de instanciar paquetes y enemigos/rivales
    public GameObject[] gameSpawners;

    [Header("Configuración de Vidas")]
    public int maxLives = 6;                  // Cantidad máxima de vidas permitidas
    public int currentLives;                  // Vidas actuales del jugador durante la partida

    [Header("Estadísticas de la Partida Actual")]
    public int packagesDelivered = 0;         // Contador de paquetes recolectados con éxito
    public float survivalTime = 0f;           // Tiempo transcurrido de supervivencia en segundos

    [Header("Referencias HUD (En Juego)")]
    public Image[] heartIcons;                // Arreglo de imágenes para representar visualmente la salud/vidas
    public TextMeshProUGUI scoreText;         // Texto para mostrar el número de paquetes en pantalla
    public TextMeshProUGUI timerText;         // Texto para el cronómetro del HUD

    [Header("Referencias GameOverPanel")]
    public TextMeshProUGUI gameOverScoreText; // Texto de paquetes finales conseguidos
    public TextMeshProUGUI gameOverTimeText;  // Texto de tiempo final alcanzado
    public TextMeshProUGUI bestScoreText;     // Texto con el récord histórico de paquetes
    public TextMeshProUGUI bestTimeText;      // Texto con el mejor tiempo registrado

    // Estado interno para saber si la partida está en curso
    private bool isGameActive = false;

    void Awake()
    {
        // Configuración del patrón Singleton (Garantiza una única instancia activa)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // Al iniciar la aplicación, muestra la pantalla del menú principal
        ShowMainMenu();
    }

    void Update()
    {
        // Si la partida está activa, incrementa el tiempo de juego y actualiza el reloj del HUD
        if (isGameActive)
        {
            survivalTime += Time.deltaTime;
            UpdateTimerUI();
        }
    }

    // --- GESTIÓN DE MENÚ Y ESTADOS ---

    // Restablece el flujo al menú principal desactivando spawners y limpiando entidades
    public void ShowMainMenu()
    {
        isGameActive = false;
        Time.timeScale = 1f; // Asegura que el tiempo del juego corra a velocidad normal

        ToggleSpawners(false);
        ToggleARScanning(false);

        // Limpiar elementos de la escena si volvemos desde el GameOver o durante la navegación
        ClearSceneEntities();

        // Control de activación/desactivación de la interfaz de usuario
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (comingSoonPanel != null) comingSoonPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (instructionsPanel != null) instructionsPanel.SetActive(false);
    }

    // Muestra la pantalla de instrucciones para el escaneo de superficies AR
    public void ShowScanInstructions()
    {
        isGameActive = false;
        ToggleSpawners(false);
        ToggleARScanning(true); // Activa el buscador de planos ARCore

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(true);
    }

    // Inicializa todos los valores de la partida y da comienzo al gameplay
    public void StartGame()
    {
        currentLives = maxLives;
        packagesDelivered = 0;
        survivalTime = 0f;
        isGameActive = true;
        Time.timeScale = 1f;

        ToggleSpawners(true); // Activa los generadores de la escena

        // Muestra el HUD y oculta los menús
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (scanInstructionsPanel != null) scanInstructionsPanel.SetActive(false);
        if (hudPanel != null) hudPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        // Refresca la interfaz gráfica con los datos iniciales
        UpdateHeartsUI();
        UpdateScoreUI();
        UpdateTimerUI();
    }

    // Activa o desactiva los scripts o GameObjects generadores (Spawners) en la escena
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

    // Habilita o inhabilita el escaneo y la visualización de los planos detectados por la cámara AR
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

    // Reduce las vidas del jugador y verifica si debe terminar la partida
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

    // Aumenta el contador de entregas y actualiza la UI correspondientemente
    public void AddDelivery()
    {
        if (!isGameActive) return;

        packagesDelivered++;
        UpdateScoreUI();
    }

    // Refresca el indicador visual de corazones/vidas en la pantalla
    void UpdateHeartsUI()
    {
        if (heartIcons == null) return;

        for (int i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] != null)
            {
                // Muestra solo los íconos de corazones correspondientes a las vidas actuales
                heartIcons[i].enabled = i < currentLives;
            }
        }
    }

    // Actualiza el texto con el contador de paquetes entregados
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Paquetes: " + packagesDelivered;
        }
    }

    // Formatea y actualiza el tiempo en formato MM:SS para el HUD
    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(survivalTime / 60F);
            int seconds = Mathf.FloorToInt(survivalTime % 60F);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    // Procesa el fin del juego, evalúa/guarda los mejores récords y detiene el tiempo
    void GameOver()
    {
        isGameActive = false;
        ToggleSpawners(false);

        if (hudPanel != null)
        {
            hudPanel.SetActive(false);
        }

        // --- SISTEMA DE RÉCORDS (PlayerPrefs) ---
        // Comprueba y guarda el puntaje más alto (paquetes)
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (packagesDelivered > bestScore)
        {
            bestScore = packagesDelivered;
            PlayerPrefs.SetInt("BestScore", bestScore);
        }

        // Comprueba y guarda el mejor tiempo de supervivencia
        float bestTime = PlayerPrefs.GetFloat("BestTime", 0f);
        if (bestTime == 0f || (survivalTime < bestTime && packagesDelivered >= bestScore))
        {
            bestTime = survivalTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
        }

        PlayerPrefs.Save(); // Guarda físicamente los cambios en el almacenamiento local del dispositivo

        // --- FORMATO DE TIEMPOS PARA EL PANEL DE GAME OVER ---
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

        // Congela el tiempo del motor para pausar movimientos en la escena
        Time.timeScale = 0f;
    }

    // Reinicia la partida limpiando los objetos sobrantes y ejecutando la lógica inicial
    public void RestartGame()
    {
        ClearSceneEntities();
        StartGame();
    }

    // Método auxiliar para destruir paquetes y rivales activos al reiniciar o volver al menú
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

    // --- MÉTODOS DE PANELES (CONFIGURACIÓN, TUTORIAL Y NAVEGACIÓN) ---

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

    // Cierra la aplicación (funciona en compilaciones ejecutables o APKs)
    public void QuitGame()
    {
        Application.Quit();
    }
}