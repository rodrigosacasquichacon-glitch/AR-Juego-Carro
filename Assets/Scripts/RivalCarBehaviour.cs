using UnityEngine;

public class RivalCarBehaviour : MonoBehaviour
{
    public float speed = 1.0f;
    public float turnSpeed = 3.0f;

    private PackageBehaviour targetPackage;

    void Update()
    {
        // 1. Si no hay objetivo o fue destruido, busca el paquete más cercano
        if (targetPackage == null)
        {
            targetPackage = FindFirstObjectByType<PackageBehaviour>();
            return;
        }

        // 2. Obtener posición del paquete e ignorar la altura Y para rotar solo en horizontal
        Vector3 targetPos = targetPackage.transform.position;
        Vector3 currentPos = transform.position;

        Vector3 direction = new Vector3(targetPos.x - currentPos.x, 0f, targetPos.z - currentPos.z);

        if (direction.sqrMagnitude > 0.01f)
        {
            // Rotación suave corregida en el plano horizontal
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

            // Avance constante
            transform.Translate(Vector3.forward * speed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Colisión con el paquete
        PackageBehaviour package = other.GetComponentInParent<PackageBehaviour>();
        if (package != null)
        {
            Debug.Log("¡El ladrón ha robado el paquete!");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.LoseLife();
            }

            Destroy(package.gameObject);
        }
    }
}