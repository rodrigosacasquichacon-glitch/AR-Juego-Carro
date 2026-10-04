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
 * Instancia un objeto <see cref="CarBehaviour"/> cuando el usuario toca un plano detectado.
 */
public class CarManager : MonoBehaviour
{
    [Header("Referencias de AR y Prefabs")]
    // Prefab del auto del jugador que se instanciará al tocar la pantalla
    public GameObject CarPrefab;

    // Referencia al comportamiento de la retícula para obtener la posición e información del plano activo
    public ReticleBehaviour Reticle;

    // Referencia al gestor del plano sobre el que conducirá el auto
    public DrivingSurfaceManager DrivingSurfaceManager;

    [Header("Ajustes de Posición")]
    // Offset para elevar el auto sobre la superficie de la retícula
    [Tooltip("Altura adicional en metros para que el auto no aparezca dentro del suelo.")]
    public float heightOffset = 0.2f;

    [Header("Instancia del Vehículo")]
    // Referencia al componente CarBehaviour del vehículo instanciado en la escena
    public CarBehaviour Car;

    private void Update()
    {
        // Si el auto aún no se ha instanciado en la escena y se detecta un toque táctil o clic
        if (Car == null && WasTapped())
        {
            // Comprobación de seguridad para depurar en consola si la retícula o el plano actual son nulos
            if (Reticle == null || Reticle.CurrentPlane == null)
            {
                Debug.LogWarning("Se tocó la pantalla, pero la Retícula no está detectando un plano activo.");
                return;
            }

            // Instanciar el coche en la escena a partir del Prefab asignado
            var obj = GameObject.Instantiate(CarPrefab);

            // Forzar activación del GameObject por si el Prefab está desactivado en la carpeta del proyecto
            obj.SetActive(true);

            // Obtiene el componente CarBehaviour del nuevo objeto
            Car = obj.GetComponent<CarBehaviour>();
            if (Car != null)
            {
                // Asigna la retícula al auto para su control posicional inicial
                Car.Reticle = Reticle;
            }

            // Asigna la posición del auto agregando la elevación en el eje Y
            Car.transform.position = Reticle.transform.position + new Vector3(0, heightOffset, 0);

            // Bloquea/fija el plano AR actual en el gestor de superficies para centrar la jugabilidad sobre él
            if (DrivingSurfaceManager != null)
            {
                DrivingSurfaceManager.LockPlane(Reticle.CurrentPlane);
            }
        }
    }

    // Método auxiliar para detectar entradas de interacción (pantalla táctil o ratón)
    private bool WasTapped()
    {
        // Detección de toque táctil inicial en dispositivos móviles (Android/iOS)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        // Detección del primer clic del ratón (útil para pruebas de desarrollo dentro del Editor de Unity)
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        return false;
    }
}