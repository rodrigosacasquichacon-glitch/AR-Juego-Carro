/*
 * Copyright 2021 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Clase encargada de controlar el comportamiento y la posición visual de la retícula en el entorno de Realidad Aumentada
public class ReticleBehaviour : MonoBehaviour
{
    [Header("Dependencias")]
    // Referencia al gestor de la superficie sobre la que interactúa el vehículo
    public DrivingSurfaceManager DrivingSurfaceManager;

    // Objeto visual secundario (hijo) que representa la gráfica del indicador/retículo en escena
    [SerializeField] GameObject Child;

    // Almacena la referencia del plano AR detectado que se encuentra actualmente bajo el centro de la retícula
    public ARPlane CurrentPlane;

    void Update()
    {
        // 1. Validaciones de seguridad previas:
        // Verifica que las referencias esenciales (gestor de superficies, gestor de raycast y cámara principal) existan.
        // Si falta alguna, interrumpe la ejecución para evitar errores NullReferenceException.
        if (DrivingSurfaceManager == null || DrivingSurfaceManager.RaycastManager == null || Camera.main == null)
            return;

        // 2. Obtener el centro de la pantalla:
        // Toma el punto central del viewport de la cámara (0.5, 0.5) y lo convierte a coordenadas en píxeles de la pantalla.
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));

        // 3. Lanzar el raycast en Realidad Aumentada:
        // Crea una lista para almacenar los impactos y proyecta un rayo desde el centro de la pantalla hacia los planos AR detectados.
        var hits = new List<ARRaycastHit>();
        DrivingSurfaceManager.RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

        // Reinicia las variables de plano e impacto para la evaluación del frame actual
        CurrentPlane = null;
        ARRaycastHit? hit = null;

        // 4. Seleccionar el impacto con mayor prioridad:
        if (hits.Count > 0)
        {
            var lockedPlane = DrivingSurfaceManager.LockedPlane;

            if (lockedPlane == null)
            {
                // Si no hay un plano fijo o bloqueado, asigna el primer impacto detectado por el raycast
                hit = hits[0];
            }
            else
            {
                // Si ya existe un plano bloqueado, busca el impacto que coincida con el id de ese plano (compatible con Unity 6)
                hit = hits.Find(x => x.trackableId == lockedPlane.trackableId);
            }
        }

        // 5. Mover la retícula al punto de intersección espacial de forma segura:
        if (hit.HasValue)
        {
            try
            {
                // Obtiene la posición/orientación física (Pose) del impacto y actualiza la posición del objeto en el mundo
                Pose targetPose = hit.Value.pose;
                transform.position = targetPose.position;

                // Recupera el objeto ARPlane correspondiente a través del PlaneManager usando su trackableId
                if (DrivingSurfaceManager.PlaneManager != null)
                {
                    CurrentPlane = DrivingSurfaceManager.PlaneManager.GetPlane(hit.Value.trackableId);
                }
            }
            catch (System.Exception)
            {
                // Captura excepciones si la estructura Pose no está lista en el frame actual e invalida el plano
                CurrentPlane = null;
            }
        }

        // 6. Controlar la visibilidad de la gráfica del indicador:
        // Activa el gráfico visual únicamente si hay un plano AR válido debajo de la retícula
        if (Child != null)
        {
            Child.SetActive(CurrentPlane != null);
        }
    }
}