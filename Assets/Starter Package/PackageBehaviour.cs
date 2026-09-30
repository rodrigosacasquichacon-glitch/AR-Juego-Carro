using UnityEngine;
using TMPro;

public class PackageBehaviour : MonoBehaviour
{
    [Header("Configuración del Temporizador")]
    public float maxTime = 12f;
    private float currentTime;

    [Header("Referencias")]
    public Renderer packageRenderer;
    public TextMeshPro timerText; // Opcional: Para mostrar los segundos encima

    [Header("Efectos de Animación Flotante")]
    public float floatSpeed = 2f;     // Velocidad de oscilación (subida/bajada)
    public float floatAmount = 0.08f;  // Amplitud del movimiento vertical
    public float rotateSpeed = 35f;   // Velocidad de rotación constante

    private Vector3 startPos;
    private bool isCollected = false;

    void Start()
    {
        currentTime = maxTime;
        startPos = transform.position;

        // Si no asignaste el Renderer, lo busca automáticamente en este objeto o sus hijos
        if (packageRenderer == null)
        {
            packageRenderer = GetComponentInChildren<Renderer>();
        }
    }

    void Update()
    {
        if (isCollected) return;

        // --- ANIMACIÓN FLOTANTE Y ROTACIÓN ---
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // --- TEMPORIZADOR Y LÓGICA DE TIEMPO ---
        currentTime -= Time.deltaTime;

        // Muestra los segundos flotantes si asignaste un TextMeshPro
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString() + "s";
        }

        // Cambio progresivo de color: Verde -> Amarillo -> Rojo
        UpdateColor();

        // Si el tiempo se acaba, explota / desaparece
        if (currentTime <= 0)
        {
            ExplodePackage();
        }
    }

    void UpdateColor()
    {
        if (packageRenderer == null) return;

        float progress = currentTime / maxTime;

        if (progress > 0.5f)
        {
            packageRenderer.material.color = Color.green;
        }
        else if (progress > 0.25f)
        {
            packageRenderer.material.color = Color.yellow;
        }
        else
        {
            packageRenderer.material.color = Color.red;
        }
    }

    public void ExplodePackage()
    {
        if (isCollected) return;
        isCollected = true;

        Debug.Log("¡El paquete ha explotado por tiempo agotado!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Detecta si choca con el auto del jugador
        if (other.CompareTag("Player") || other.GetComponentInParent<CarBehaviour>() != null)
        {
            isCollected = true;
            Debug.Log("¡Paquete Recogido con éxito!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddDelivery();
            }

            Destroy(gameObject);
        }
    }
}