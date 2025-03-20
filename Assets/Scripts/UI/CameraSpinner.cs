using UnityEngine;

public class CameraSpinner : MonoBehaviour
{
    public Transform target;
    public float distance = 10.0f;
    public float orbitSpeed = 10.0f;

    private float currentAngle = 0f;

    void Update()
    {
        if (target == null) {
            Debug.LogWarning("Nenhum alvo definido para a câmera orbitar.");
            return;
        }

        currentAngle += orbitSpeed * Time.deltaTime;

        float x = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distance;
        float z = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distance;
        transform.position = target.position + new Vector3(x, 0, z);

        transform.LookAt(target);
    }
}