using UnityEngine;

public class TrainMovement : MonoBehaviour {
    public float speed = 15f;
    public float trainHeight = 1f; // Altura do trem para o player ficar em cima
    public float destroyZ = -30f; // Posição Z onde o trem será destruído
    private const float COLLISION_THRESHOLD = 1f; // Margem de colisão na direção Z
    private const float MIN_GROUND_HEIGHT = 0.5f; // Altura mínima do chão
    private const float GROUND_HEIGHT = 0f; // Altura do chão (plane)

    private BoxCollider triggerCollider;
    private BoxCollider solidCollider;
    private bool isDestroyed = false;

    private void Start() {
        // Garante que o trem tem a tag correta
        gameObject.tag = "Train";
        gameObject.layer = LayerMask.NameToLayer("Ground");
        
        // Verifica se já tem os colliders
        BoxCollider[] existingColliders = GetComponents<BoxCollider>();
        
        // Se não tiver nenhum collider, adiciona os dois
        if (existingColliders.Length == 0) {
            // Adiciona dois colliders: um para trigger de dano frontal e outro para pousar
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1.8f, 1f, 0.5f); // Collider para colisão frontal
            triggerCollider.center = new Vector3(0f, 0.5f, 4.5f); // Posiciona na frente do trem

            // Collider físico para o player pousar
            solidCollider = gameObject.AddComponent<BoxCollider>();
            solidCollider.isTrigger = false;
            solidCollider.size = new Vector3(2f, 0.2f, 9f); // Collider mais grosso para superfície
            solidCollider.center = new Vector3(0f, 1f, 0f); // Posiciona no topo do trem
        } else {
            // Se já tem colliders, pega as referências e ajusta
            foreach (BoxCollider collider in existingColliders) {
                if (collider.isTrigger) {
                    triggerCollider = collider;
                    triggerCollider.size = new Vector3(1.8f, 1f, 0.5f);
                    triggerCollider.center = new Vector3(0f, 0.5f, 4.5f);
                } else {
                    solidCollider = collider;
                    solidCollider.size = new Vector3(2f, 0.2f, 9f);
                    solidCollider.center = new Vector3(0f, 1f, 0f);
                }
            }
        }

        // Debug para verificar se o trem foi criado corretamente
        Debug.Log($"Train spawned at position: {transform.position}");
    }

    void Update() {
        if (isDestroyed) return;

        // Move o trem em direção ao player (direção negativa do eixo Z)
        Vector3 movement = Vector3.back * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);

        // Debug para verificar o movimento
        Debug.DrawRay(transform.position, movement * 10f, Color.red);

        // Visualiza os colliders usando linhas
        if (triggerCollider != null) {
            DrawDebugCollider(triggerCollider, Color.yellow);
        }
        if (solidCollider != null) {
            DrawDebugCollider(solidCollider, Color.green);
        }

        // Destrói o trem quando passar muito longe
        if (transform.position.z < destroyZ) {
            Debug.Log($"Train destroyed at position: {transform.position}");
            isDestroyed = true;
            Destroy(gameObject);
        }
    }

    private void DrawDebugCollider(BoxCollider collider, Color color) {
        // Calcula os pontos do cubo baseado no centro e tamanho do collider
        Vector3 center = transform.position + collider.center;
        Vector3 size = collider.size;
        Vector3 extents = size / 2f;

        // Pontos do cubo
        Vector3 frontTopLeft = center + new Vector3(-extents.x, extents.y, extents.z);
        Vector3 frontTopRight = center + new Vector3(extents.x, extents.y, extents.z);
        Vector3 frontBottomLeft = center + new Vector3(-extents.x, -extents.y, extents.z);
        Vector3 frontBottomRight = center + new Vector3(extents.x, -extents.y, extents.z);
        Vector3 backTopLeft = center + new Vector3(-extents.x, extents.y, -extents.z);
        Vector3 backTopRight = center + new Vector3(extents.x, extents.y, -extents.z);
        Vector3 backBottomLeft = center + new Vector3(-extents.x, -extents.y, -extents.z);
        Vector3 backBottomRight = center + new Vector3(extents.x, -extents.y, -extents.z);

        // Desenha as linhas do cubo
        // Face frontal
        Debug.DrawLine(frontTopLeft, frontTopRight, color);
        Debug.DrawLine(frontTopRight, frontBottomRight, color);
        Debug.DrawLine(frontBottomRight, frontBottomLeft, color);
        Debug.DrawLine(frontBottomLeft, frontTopLeft, color);

        // Face traseira
        Debug.DrawLine(backTopLeft, backTopRight, color);
        Debug.DrawLine(backTopRight, backBottomRight, color);
        Debug.DrawLine(backBottomRight, backBottomLeft, color);
        Debug.DrawLine(backBottomLeft, backTopLeft, color);

        // Linhas conectando as faces
        Debug.DrawLine(frontTopLeft, backTopLeft, color);
        Debug.DrawLine(frontTopRight, backTopRight, color);
        Debug.DrawLine(frontBottomRight, backBottomRight, color);
        Debug.DrawLine(frontBottomLeft, backBottomLeft, color);
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player == null || player.IsInvincible) return;

            // Verifica se o trem está realmente próximo do player na direção Z
            float zDistance = Mathf.Abs(transform.position.z - other.transform.position.z);
            
            // Verifica se é uma colisão frontal (player está na frente do trem)
            bool isFrontalCollision = other.transform.position.z > transform.position.z;

            if (zDistance <= COLLISION_THRESHOLD && isFrontalCollision) {
                // Causa dano ao jogador e destrói o trem apenas em colisão frontal
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
                // Verifica se o player está realmente em cima do trem
                ContactPoint[] contacts = new ContactPoint[1];
                collision.GetContacts(contacts);
                if (contacts.Length > 0 && contacts[0].normal.y > 0.5f) {
                    // Se o player estiver em cima do trem, mantém ele na altura correta
                    float targetHeight = transform.position.y + trainHeight;
                    Vector3 playerPos = player.transform.position;
                    playerPos.y = targetHeight;
                    
                    // Move o player junto com o trem
                    playerPos.z += Vector3.back.z * speed * Time.deltaTime;
                    player.transform.position = playerPos;
                    
                    // Marca que o player está no trem
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
                // Quando o player sair do trem, começa a cair
                player.StartFalling();
                player.SetOnTrain(false); // Marca que o player não está mais no trem
                
                // Força o player a começar a cair do ponto atual
                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null) {
                    rb.linearVelocity = new Vector3(0f, -5f, 0f); // Aplica velocidade para baixo mantendo X e Z
                }
            }
        }
    }
} 