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

public class ReticleBehaviour : MonoBehaviour
{
    [Header("Dependencias")]
    public DrivingSurfaceManager DrivingSurfaceManager;
    [SerializeField] GameObject Child; // objeto visual del retículo

    // Plano actual bajo el centro de la cámara
    public ARPlane CurrentPlane;

    void Update()
    {
        // Validaciones de seguridad previas
        if (DrivingSurfaceManager == null || DrivingSurfaceManager.RaycastManager == null || Camera.main == null)
            return;

        // 1. Centro de la pantalla en espacio de pantalla (píxeles)
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0f));

        // 2. Lanzar rayo contra planos AR detectados
        var hits = new List<ARRaycastHit>();
        DrivingSurfaceManager.RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinBounds);

        // 3. Seleccionar impacto prioritario
        CurrentPlane = null;
        ARRaycastHit? hit = null;

        if (hits.Count > 0)
        {
            var lockedPlane = DrivingSurfaceManager.LockedPlane;

            if (lockedPlane == null)
            {
                // Sin plano fijo → usar el primero detectado
                hit = hits[0];
            }
            else
            {
                // Unity 6: hits.Find() en vez de SingleOrDefault()
                hit = hits.Find(x => x.trackableId == lockedPlane.trackableId);
            }
        }

        // 4. Mover el retículo al punto de intersección de forma segura
        if (hit.HasValue)
        {
            try
            {
                // Protección para el error NullReferenceException en hit.Value.pose
                Pose targetPose = hit.Value.pose;
                transform.position = targetPose.position;

                if (DrivingSurfaceManager.PlaneManager != null)
                {
                    CurrentPlane = DrivingSurfaceManager.PlaneManager.GetPlane(hit.Value.trackableId);
                }
            }
            catch (System.Exception)
            {
                // Si la Pose de AR Foundation aún no es válida en este frame, invalida el impacto
                CurrentPlane = null;
            }
        }

        // 5. Visible solo cuando hay plano válido bajo el retículo
        if (Child != null)
        {
            Child.SetActive(CurrentPlane != null);
        }
    }
}