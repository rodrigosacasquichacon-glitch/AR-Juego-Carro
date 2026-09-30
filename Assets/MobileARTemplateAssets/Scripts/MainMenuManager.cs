using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // Necesario si usas componentes de TextMeshPro

public class MainMenuManager : MonoBehaviour
{
    [Header("Nombre de la Escena de Juego")]
    public string gameSceneName = "SampleScene"; // Asegúrate de que coincida exactamente con el nombre de tu escena de juego

    [Header("Paneles")]
    public GameObject comingSoonPanel; // Panel emergente para los botones "Próximamente"

    void Start()
    {
        // Asegurar que el tiempo del juego corra a velocidad normal (por si venimos de un pause)
        Time.timeScale = 1f;

        // Ocultar el panel emergente al iniciar la escena
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(false);
        }
    }

    // Método para el botón "Jugar"
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Método para los botones "Configuración" e "Instrucciones"
    public void ShowComingSoon()
    {
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(true);
        }
        else
        {
            Debug.Log("Esta función estará disponible próximamente.");
        }
    }

    // Método para el botón de cerrar en el panel "Próximamente"
    public void CloseComingSoon()
    {
        if (comingSoonPanel != null)
        {
            comingSoonPanel.SetActive(false);
        }
    }

    // Método para el botón "Salir"
    public void QuitGame()
    {
        Debug.Log("Cerrando el juego...");
        Application.Quit();
    }
}