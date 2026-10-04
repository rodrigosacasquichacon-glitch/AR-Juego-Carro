/*
 * Copyright 2021 Google LLC
 * Modificado para Cargo Defenders
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

// Clase encargada de calcular posiciones aleatorias dentro del plano AR detectado e instanciar los paquetes
public class PackageSpawner : MonoBehaviour
{
    [Header("Referencias de AR y Prefabs")]
    // Referencia al gestor del plano/superficie de conducción
    public DrivingSurfaceManager DrivingSurfaceManager;

    // Instancia del paquete activo actualmente en la escena
    public PackageBehaviour Package;

    // Prefab del paquete que se va a instanciar
    public GameObject PackagePrefab;

    // --- MÉTODOS ESTÁTICOS DE CÁLCULO GEOMÉTRICO ---

    // Calcula una posición aleatoria dentro de un triángulo definido por dos vectores (v1 y v2) usando Coordenadas Baricéntricas
    public static Vector3 RandomInTriangle(Vector3 v1, Vector3 v2)
    {
        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);

        // Si la suma de los puntos excede 1 (sale del triángulo), los invierte para reubicarlos dentro de los límites
        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }

        return (v1 * u) + (v2 * v);
    }

    // Encuentra un punto aleatorio dentro de la malla poligonal (mesh) de un plano AR detectado
    public static Vector3 FindRandomLocation(ARPlane plane)
    {
        // Obtiene la malla visual del plano AR
        var visualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
        if (visualizer == null || visualizer.mesh == null)
        {
            return plane.center; // Retorno seguro al centro del plano si no hay malla
        }

        var mesh = visualizer.mesh;
        var triangles = mesh.triangles;
        var vertices = mesh.vertices;

        // VALIDACIÓN DE SEGURIDAD: Evita el crash si no hay suficientes triángulos o vértices
        if (triangles == null || triangles.Length < 3 || vertices == null || vertices.Length < 3)
        {
            return plane.center;
        }

        // Selecciona un triángulo válido de la malla de manera aleatoria
        int triangleIndex = Random.Range(0, triangles.Length / 3) * 3;

        int index0 = triangles[triangleIndex];
        int index1 = triangles[triangleIndex + 1];

        // Comprobación de seguridad para asegurar que los índices estén dentro de los límites del arreglo de vértices
        if (index0 >= vertices.Length || index1 >= vertices.Length)
        {
            return plane.center;
        }

        // Obtiene una coordenada local aleatoria dentro del triángulo seleccionado
        Vector3 randomInTriangle = RandomInTriangle(vertices[index0], vertices[index1]);

        // Convierte el punto de espacio local de la malla a coordenadas globales en el mundo 3D
        return plane.transform.TransformPoint(randomInTriangle);
    }

    // --- MÉTODOS DE INSTANCIACIÓN Y CICLO DE VIDA ---

    // Instancia el prefab del paquete en una posición válida dentro del plano AR indicado
    public void SpawnPackage(ARPlane plane)
    {
        if (PackagePrefab == null) return;

        // Crea el clon del paquete en la escena
        var packageClone = GameObject.Instantiate(PackagePrefab);

        // Asigna la posición calculada aleatoriamente
        packageClone.transform.position = FindRandomLocation(plane);

        // Guarda la referencia al componente PackageBehaviour del paquete recién creado
        Package = packageClone.GetComponent<PackageBehaviour>();
    }

    private void Update()
    {
        // Si no hay un gestor de superficie asignado, detiene la ejecución del frame
        if (DrivingSurfaceManager == null) return;

        // Obtiene el plano bloqueado/fijado por el usuario
        var lockedPlane = DrivingSurfaceManager.LockedPlane;
        if (lockedPlane != null)
        {
            // Si no existe un paquete activo en escena, lo genera sobre el plano
            if (Package == null)
            {
                SpawnPackage(lockedPlane);
            }
            else
            {
                // Si el paquete ya existe, ajusta constantemente su altura (eje Y) para mantenerlo alineado al nivel del plano AR
                var packagePosition = Package.gameObject.transform.position;
                packagePosition.y = lockedPlane.center.y;
                Package.gameObject.transform.position = packagePosition;
            }
        }
    }
}