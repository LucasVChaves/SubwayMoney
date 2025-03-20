using UnityEngine;

public class SmoothCamera : MonoBehaviour {
    public Transform player;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;
    public Vector3 rotation;

    void Start() {
        transform.rotation = Quaternion.Euler(rotation);
    }

    void Update() {
        Vector3 desiredPosition = player.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}