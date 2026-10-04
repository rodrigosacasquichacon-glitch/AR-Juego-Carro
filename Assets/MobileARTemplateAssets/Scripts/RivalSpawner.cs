using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Clase encargada de gestionar la reaparición periódica y controlada de vehículos rivales sobre el plano AR
public class RivalSpawner : MonoBehaviour
{
    [Header("Configuración de Referencias y Prefabs")]
    // Referencia al gestor del plano AR donde se desplazarán los rivales
    public DrivingSurfaceManager drivingSurfaceManager;

    // Prefab del vehículo enemigo/rival que se instanciará en la escena
    public GameObject rivalCarPrefab;

    [Header("Parámetros de Generación")]
    // Intervalo de tiempo en segundos entre cada intento de reaparición de un rival
    public float spawnInterval = 8f;

    // Cantidad máxima permitida de vehículos rivales en pantalla de forma simultánea
    public int maxRivals = 2; // Límite para no saturar la pantalla

    // Temporizador interno para medir el tiempo transcurrido desde el último spawn
    private float timer = 0f;

    void Update()
    {
        // 1. Validaciones de seguridad:
        // Verifica si el gestor de superficies o el plano AR bloqueado/fijado son nulos antes de continuar
        if (drivingSurfaceManager == null || drivingSurfaceManager.LockedPlane == null)
            return;

        // 2. Control de densidad/límite de rivales:
        // Comprueba cuántos ladrones activos hay actualmente buscando todas las instancias de RivalCarBehaviour
        int currentRivals = FindObjectsByType<RivalCarBehaviour>(FindObjectsSortMode.None).Length;
        if (currentRivals >= maxRivals) return;

        // 3. Acumulación del tiempo transcurrido en cada frame
        timer += Time.deltaTime;

        // 4. Generación por intervalo:
        // Si el temporizador supera el tiempo de espera configurado, crea un nuevo rival y reinicia el contador
        if (timer >= spawnInterval)
        {
            SpawnRival();
            timer = 0f;
        }
    }

    // Método encargado de instanciar e inicializar un nuevo auto rival en una posición aleatoria del plano
    void SpawnRival()
    {
        if (rivalCarPrefab == null) return;

        ARPlane plane = drivingSurfaceManager.LockedPlane;
        if (plane == null) return;

        // Calcula una posición aleatoria y segura dentro de los límites de la malla del plano AR
        Vector3 spawnPosition = PackageSpawner.FindRandomLocation(plane);

        // Instancia el prefab del auto ladrón en la posición calculada con una rotación inicial por defecto
        GameObject rival = Instantiate(rivalCarPrefab, spawnPosition, Quaternion.identity);

        // Ajusta la posición en el eje Y exactamente a la altura central del plano para prevenir temblores de física
        Vector3 pos = rival.transform.position;
        pos.y = plane.center.y;
        rival.transform.position = pos;
    }
}