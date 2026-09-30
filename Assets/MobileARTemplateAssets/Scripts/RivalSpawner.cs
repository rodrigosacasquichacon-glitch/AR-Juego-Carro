using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class RivalSpawner : MonoBehaviour
{
    public DrivingSurfaceManager drivingSurfaceManager;
    public GameObject rivalCarPrefab;
    public float spawnInterval = 8f;
    public int maxRivals = 2; // Límite para no saturar la pantalla

    private float timer = 0f;

    void Update()
    {
        if (drivingSurfaceManager == null || drivingSurfaceManager.LockedPlane == null)
            return;

        // Comprueba cuántos ladrones activos hay actualmente
        int currentRivals = FindObjectsByType<RivalCarBehaviour>(FindObjectsSortMode.None).Length;
        if (currentRivals >= maxRivals) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRival();
            timer = 0f;
        }
    }

    void SpawnRival()
    {
        if (rivalCarPrefab == null) return;

        ARPlane plane = drivingSurfaceManager.LockedPlane;
        if (plane == null) return;

        // Posición segura sobre el plano
        Vector3 spawnPosition = PackageSpawner.FindRandomLocation(plane);

        // Instancia el auto ladrón a la altura del plano
        GameObject rival = Instantiate(rivalCarPrefab, spawnPosition, Quaternion.identity);

        // Ajusta la altura exactamente al plano para evitar que tiemble
        Vector3 pos = rival.transform.position;
        pos.y = plane.center.y;
        rival.transform.position = pos;
    }
}