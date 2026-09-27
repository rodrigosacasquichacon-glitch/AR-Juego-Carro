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
using UnityEngine;
using UnityEngine.XR.ARFoundation;

/**
 * Spawns a <see cref="CarBehaviour"/> when a plane is tapped.
 */
public class CarManager : MonoBehaviour
{
    public GameObject CarPrefab;
    public ReticleBehaviour Reticle;
    public DrivingSurfaceManager DrivingSurfaceManager;

    public CarBehaviour Car;

    private void Update()
    {
        if (Car == null && WasTapped())
        {
            // Comprobación de seguridad para depurar en consola si el plano es nulo
            if (Reticle == null || Reticle.CurrentPlane == null)
            {
                Debug.LogWarning("Se tocó la pantalla, pero la Retícula no está detectando un plano activo.");
                return;
            }

            // Instanciar el coche en la posición de la retícula
            var obj = GameObject.Instantiate(CarPrefab);

            // Forzar activación por si el Prefab está desactivado en la carpeta del proyecto
            obj.SetActive(true);

            Car = obj.GetComponent<CarBehaviour>();
            if (Car != null)
            {
                Car.Reticle = Reticle;
            }

            Car.transform.position = Reticle.transform.position;

            if (DrivingSurfaceManager != null)
            {
                DrivingSurfaceManager.LockPlane(Reticle.CurrentPlane);
            }
        }
    }

    private bool WasTapped()
    {
        // Detección de toque táctil en dispositivos móviles
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        // Detección de clic de ratón (útil para pruebas en el Editor de Unity)
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return false;
    }
}