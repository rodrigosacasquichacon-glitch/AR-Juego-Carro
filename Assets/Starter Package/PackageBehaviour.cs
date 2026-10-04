using UnityEngine;
using TMPro;

// Clase encargada de controlar la mecánica, animación, temporizador y colisiones de los paquetes de carga
public class PackageBehaviour : MonoBehaviour
{
    [Header("Configuración del Temporizador")]
    // Tiempo máximo en segundos antes de que el paquete explote por no ser recogido a tiempo
    public float maxTime = 12f;
    // Contador interno del tiempo restante
    private float currentTime;

    [Header("Referencias")]
    // Referencia al componente Renderer para modificar el color del material dinámicamente
    public Renderer packageRenderer;
    // Componente de texto en 3D para mostrar el tiempo en segundos sobre el paquete
    public TextMeshPro timerText; // Opcional: Para mostrar los segundos encima

    [Header("Efectos de Animación Flotante")]
    // Velocidad a la que el paquete sube y baja
    public float floatSpeed = 2f;     // Velocidad de oscilación (subida/bajada)
    // Distancia máxima vertical del movimiento flotante
    public float floatAmount = 0.08f;  // Amplitud del movimiento vertical
    // Velocidad de giro continuo sobre su propio eje Y
    public float rotateSpeed = 35f;   // Velocidad de rotación constante

    // Posición inicial de origen en el mundo para calcular la oscilación vertical
    private Vector3 startPos;
    // Flag de control para evitar que el paquete ejecute interacciones múltiples una vez destruido o recogido
    private bool isCollected = false;

    void Start()
    {
        // Inicialización del temporizador con el tiempo máximo asignado
        currentTime = maxTime;
        // Guarda la posición inicial donde fue instanciado el paquete
        startPos = transform.position;

        // Si no se asignó manualmente el Renderer en el Inspector, busca el componente en este GameObject o sus hijos
        if (packageRenderer == null)
        {
            packageRenderer = GetComponentInChildren<Renderer>();
        }
    }

    void Update()
    {
        // Si el paquete ya fue recogido o explotó, detiene el procesamiento de este frame
        if (isCollected) return;

        // --- ANIMACIÓN FLOTANTE Y ROTACIÓN ---
        // 1. Calcula la nueva posición en el eje Y usando una onda senoidal para un movimiento fluido de subida y bajada
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        // 2. Aplica una rotación continua sobre el eje Y (vertical) independiente de la tasa de cuadros (Time.deltaTime)
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);

        // --- TEMPORIZADOR Y LÓGICA DE TIEMPO ---
        // Reduciendo el tiempo restante según los segundos transcurridos desde el último frame
        currentTime -= Time.deltaTime;

        // Muestra los segundos flotantes formateados en texto si la referencia TextMeshPro está asignada
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString() + "s";
        }

        // Cambio progresivo del color visual del paquete según el tiempo restante (Verde -> Amarillo -> Rojo)
        UpdateColor();

        // Si el tiempo llega a cero o menos, ejecuta la lógica de explosión/destrucción
        if (currentTime <= 0)
        {
            ExplodePackage();
        }
    }

    // Método encargado de cambiar el color del paquete según el porcentaje de tiempo restante
    void UpdateColor()
    {
        if (packageRenderer == null) return;

        // Calcula el porcentaje de tiempo restante (de 1.0 a 0.0)
        float progress = currentTime / maxTime;

        // Mas del 50% del tiempo restante: Verde
        if (progress > 0.5f)
        {
            packageRenderer.material.color = Color.green;
        }
        // Entre 25% y 50% del tiempo restante: Amarillo
        else if (progress > 0.25f)
        {
            packageRenderer.material.color = Color.yellow;
        }
        // Menos del 25% del tiempo restante: Rojo (Peligro)
        else
        {
            packageRenderer.material.color = Color.red;
        }
    }

    // Método ejecutado cuando el tiempo se agota antes de que el jugador recoja la carga
    public void ExplodePackage()
    {
        if (isCollected) return;
        isCollected = true;

        Debug.Log("¡El paquete ha explotado por tiempo agotado!");

        // Resta una vida al jugador en el administrador del juego (GameManager)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoseLife();
        }

        // Destruye el objeto del paquete de la escena
        Destroy(gameObject);
    }

    // Detección de colisiones mediante trigger físico
    private void OnTriggerEnter(Collider other)
    {
        if (isCollected) return;

        // Detecta si el objeto colisionado tiene el Tag "Player" o el componente CarBehaviour en sus padres
        if (other.CompareTag("Player") || other.GetComponentInParent<CarBehaviour>() != null)
        {
            isCollected = true;
            Debug.Log("¡Paquete Recogido con éxito!");

            // Suma un envío entregado con éxito en el GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddDelivery();
            }

            // Destruye el objeto del paquete recolectado
            Destroy(gameObject);
        }
    }
}