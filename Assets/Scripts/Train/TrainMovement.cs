using UnityEngine;

public class TrainMovement : MonoBehaviour {
    public float speed = 15f;
    public float trainHeight = 1f;
    public float destroyZ = -30f;
    private const float COLLISION_THRESHOLD = 1f;
    private bool isDestroyed = false;

    private void Start() {
        Debug.Log($"Train spawned at position: {transform.position}");
    }

    void Update() {
        if (isDestroyed) return;

        Vector3 movement = Vector3.back * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        Debug.DrawRay(transform.position, movement * 10f, Color.red);
        if (transform.position.z < destroyZ) {
            Debug.Log($"Train destroyed at position: {transform.position}");
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || player.IsInvincible) return;

            float zDistance = Mathf.Abs(transform.position.z - other.transform.position.z);
            
            bool isFrontalCollision = other.transform.position.z > transform.position.z;

            if (zDistance <= COLLISION_THRESHOLD && isFrontalCollision) {
                player.TakeDamage();
                Destroy(gameObject);
            }
        }
    }

    private void OnCollisionStay(Collision collision) {
        if (isDestroyed) return;

        if (collision.gameObject.CompareTag("Player")) {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) {
                ContactPoint[] contacts = new ContactPoint[1];
                collision.GetContacts(contacts);
                if (contacts.Length > 0 && contacts[0].normal.y > 0.5f) {
                    float targetHeight = transform.position.y + trainHeight;
                    Vector3 playerPos = player.transform.position;
                    playerPos.y = targetHeight;

                    playerPos.z += Vector3.back.z * speed * Time.deltaTime;
                    player.transform.position = playerPos;

                    player.SetOnTrain(true);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision) {
        if (isDestroyed) return;

        if (collision.gameObject.CompareTag("Player")) {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null) {
                player.StartFalling();
                player.SetOnTrain(false);

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.linearVelocity = new Vector3(0f, -5f, 0f);
                }
            }
        }
    }
} 