/*
 * Copyright 2021 Google LLC
 * Modificado para Cargo Defenders
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PackageSpawner : MonoBehaviour
{
    public DrivingSurfaceManager DrivingSurfaceManager;
    public PackageBehaviour Package;
    public GameObject PackagePrefab;

    public static Vector3 RandomInTriangle(Vector3 v1, Vector3 v2)
    {
        float u = Random.Range(0.0f, 1.0f);
        float v = Random.Range(0.0f, 1.0f);
        if (v + u > 1)
        {
            v = 1 - v;
            u = 1 - u;
        }

        return (v1 * u) + (v2 * v);
    }

    public static Vector3 FindRandomLocation(ARPlane plane)
    {
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

        // Selecciona un triángulo válido de manera segura
        int triangleIndex = Random.Range(0, triangles.Length / 3) * 3;

        int index0 = triangles[triangleIndex];
        int index1 = triangles[triangleIndex + 1];

        // Comprobación de índices dentro de los límites del arreglo de vértices
        if (index0 >= vertices.Length || index1 >= vertices.Length)
        {
            return plane.center;
        }

        Vector3 randomInTriangle = RandomInTriangle(vertices[index0], vertices[index1]);
        return plane.transform.TransformPoint(randomInTriangle);
    }

    public void SpawnPackage(ARPlane plane)
    {
        if (PackagePrefab == null) return;

        var packageClone = GameObject.Instantiate(PackagePrefab);
        packageClone.transform.position = FindRandomLocation(plane);

        Package = packageClone.GetComponent<PackageBehaviour>();
    }

    private void Update()
    {
        if (DrivingSurfaceManager == null) return;

        var lockedPlane = DrivingSurfaceManager.LockedPlane;
        if (lockedPlane != null)
        {
            if (Package == null)
            {
                SpawnPackage(lockedPlane);
            }
            else
            {
                // Mantiene el paquete pegado a la altura del plano
                var packagePosition = Package.gameObject.transform.position;
                packagePosition.y = lockedPlane.center.y;
                Package.gameObject.transform.position = packagePosition;
            }
        }
    }
}