using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

enum PlayerState {
    Running,
    Jumping,
    Rolling,
    Dead
}

public class PlayerController : MonoBehaviour {
    [Header("Vida")]
    public int maxHealth = 3;
    private int currentHealth;
    public float invincibilityTime = 2f;
    private float invincibilityTimer = 0f;
    private bool isInvincible = false;

    // Propriedades públicas
    public int CurrentHealth => currentHealth;
    public bool IsInvincible => isInvincible;
    public bool IsFalling => vVelocity < 0;

    [Header("Faixas")]
    public float laneDist = 3f;
    private int currLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private float[] lanePos;
    
    [Header("Movimento do Player")]
    public float jumpForce = 1000f;
    public float gravity = -9.81f;
    public float fastFallMultiplier = 2f;
    public float laneTransitionSpeed = 10f; // Velocidade da transição entre faixas
    public float groundLevel = 0f;
    public float groundCheckDistance = 0.2f; // Distância para verificar o chão
    public LayerMask groundLayer; // Layer do chão e trens

    private float vVelocity = 0f;
    private bool isGrounded = true;
    private bool isOnTrain = false;
    private float currentXPos; // Posição atual em X para interpolação
    private PlayerState state = PlayerState.Running;

    [Header("Efeitos")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    private AudioSource audioSource;

    private Animator animator;

    void Start() {
        lanePos = new float[3];
        lanePos[0] = -laneDist;
        lanePos[1] = 0f;
        lanePos[2] = laneDist;

        // Configura o LayerMask para incluir a layer Ground
        groundLayer = LayerMask.GetMask("Ground");

        // inicia o player centralizado e no chão
        currentXPos = lanePos[currLane];
        transform.position = new Vector3(currentXPos, groundLevel + 0.1f, transform.position.z);
        isGrounded = true;
        state = PlayerState.Running;

        animator = GetComponentInChildren<Animator>();
        animator.SetBool("IsGameStarted", true);

        // Inicializa a vida
        currentHealth = maxHealth;

        // Configura o collider do player
        BoxCollider playerCollider = GetComponent<BoxCollider>();
        if (playerCollider == null) {
            playerCollider = gameObject.AddComponent<BoxCollider>();
        }
        playerCollider.size = new Vector3(0.5f, 1f, 0.5f); // Tamanho do collider do player
        playerCollider.center = new Vector3(0f, 0.5f, 0f); // Centro do collider
        playerCollider.isTrigger = false; // Collider sólido para colisões físicas

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        Debug.Log($"Ground Layer Mask: {groundLayer.value}");
    }

    void Update() {
        if (state == PlayerState.Dead) return;
        
        if (isInvincible) {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0) {
                isInvincible = false;
            }
        }

        ProcessInput();
        ProcessAnimations();
    }

    void FixedUpdate() {
        HandleMovement();
        CheckGround();
        
        // Se não estiver no chão e não estiver em cima de um trem, aplica gravidade
        if (!isGrounded) {
            vVelocity += gravity * Time.fixedDeltaTime;
        }
    }

    void ProcessInput() {
        // Movimentação lateral - só muda se não estiver rolando
        if (state != PlayerState.Rolling) {
            if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) {
                if (currLane > 0) {
                    currLane--;
                }
            }
            if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) {
                if (currLane < 2) {
                    currLane++;
                }
            }
        }

        // Pulo
        if (isGrounded && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)) {
            vVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            isGrounded = false;
            state = PlayerState.Jumping;
        }

        // Rolamento (no chão) ou Fast Fall (no ar)
        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame) {
            if (isGrounded) {
                // Rolamento no chão
                state = PlayerState.Rolling;
                // Reduz temporariamente o tamanho do collider
                BoxCollider playerCollider = GetComponent<BoxCollider>();
                if (playerCollider != null) {
                    playerCollider.size = new Vector3(0.5f, 0.5f, 0.5f);
                    playerCollider.center = new Vector3(0f, 0.25f, 0f);
                }
                // Volta ao estado normal após um tempo
                Invoke("EndRoll", 1f);
            } else {
                // Fast fall no ar
                if (vVelocity > 0) {
                    vVelocity = 0f;
                }
                state = PlayerState.Rolling;
            }
        }
    }

    private void EndRoll() {
        if (state == PlayerState.Rolling && isGrounded) {
            state = PlayerState.Running;
            // Restaura o tamanho original do collider
            BoxCollider playerCollider = GetComponent<BoxCollider>();
            if (playerCollider != null) {
                playerCollider.size = new Vector3(0.5f, 1f, 0.5f);
                playerCollider.center = new Vector3(0f, 0.5f, 0f);
            }
        }
    }

    void HandleMovement() {
        Vector3 newPosition = transform.position;
        
        // Movimento lateral com Lerp para transição suave
        currentXPos = Mathf.Lerp(currentXPos, lanePos[currLane], laneTransitionSpeed * Time.fixedDeltaTime);
        newPosition.x = currentXPos;

        // Aplica multiplicador de queda rápida se estiver rolando no ar
        float currentGravity = gravity;
        if (state == PlayerState.Rolling && !isGrounded) {
            currentGravity *= fastFallMultiplier;
        }

        // Aplica gravidade e movimento vertical se não estiver no chão
        if (!isGrounded || state == PlayerState.Jumping) {
            vVelocity += currentGravity * Time.fixedDeltaTime;
            newPosition.y += vVelocity * Time.fixedDeltaTime;
        }

        transform.position = newPosition;
    }

    void CheckGround() {
        // Se estiver subindo, não verifica o chão
        if (vVelocity > 0) {
            isGrounded = false;
            isOnTrain = false;
            return;
        }

        // Verifica se está no chão
        RaycastHit hit;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 2f;

        // Debug visual do raycast
        Debug.DrawRay(rayStart, rayDirection * rayDistance, Color.red);

        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, groundLayer)) {
            float targetHeight = hit.point.y;
            
            // Verifica se está em cima do trem
            if (hit.collider.CompareTag("Train")) {
                targetHeight += 1f;
                
                // Se não estava no trem antes, só considera colisão se estiver caindo
                if (!isOnTrain && vVelocity < 0) {
                    isOnTrain = true;
                    isGrounded = true;
                    transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);
                    vVelocity = 0f;
                    state = PlayerState.Running;
                }
            } else {
                // Se estava no trem e agora não está mais
                if (isOnTrain) {
                    isOnTrain = false;
                    isGrounded = false;
                    state = PlayerState.Jumping;
                    if (vVelocity >= 0) {
                        vVelocity = -5f;
                    }
                } 
                // Se está tocando o chão normal
                else if (transform.position.y <= targetHeight + 0.1f) {
                    isGrounded = true;
                    transform.position = new Vector3(transform.position.x, groundLevel, transform.position.z);
                    vVelocity = 0f;
                    state = PlayerState.Running;
                }
            }
        } else {
            // Não encontrou nenhuma superfície embaixo
            if (isOnTrain) {
                isOnTrain = false;
                isGrounded = false;
                state = PlayerState.Jumping;
                if (vVelocity >= 0) {
                    vVelocity = -5f;
                }
            } else if (isGrounded) {
                isGrounded = false;
                state = PlayerState.Jumping;
                if (vVelocity >= 0) {
                    vVelocity = -5f;
                }
            }
        }

        // Failsafe: se de alguma forma o player cair muito abaixo do chão
        if (transform.position.y < groundLevel - 5f) {
            transform.position = new Vector3(transform.position.x, groundLevel + 0.1f, transform.position.z);
            vVelocity = 0f;
            isGrounded = true;
            state = PlayerState.Running;
            Debug.LogWarning("Player caiu muito abaixo do chão! Reposicionando...");
        }
    }

    private void OnTriggerEnter(Collider other) {
        // Primeiro verifica se é uma moeda
        if (other.CompareTag("Coin")) {
            CollectCoin(other.gameObject);
            return; // Sai da função após coletar a moeda
        }

        // Se não for moeda, verifica se é trem
        if (other.CompareTag("Train")) {
            HandleTrainCollision(other);
        }
    }

    private void HandleTrainCollision(Collider trainCollider) {
        // Se já estiver no trem, ignora a colisão
        if (!isGrounded) return;

        // Causa dano ao player
        TakeDamage();
        
        // Destrói o trem
        Destroy(trainCollider.gameObject);
    }

    public void CollectCoin(GameObject coin) {
        // Destrói a moeda
        Destroy(coin);
        
        // Adiciona pontos
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager != null) {
            uiManager.AddScore();
        }
    }

    public void TakeDamage() {
        if (isInvincible) return;
        
        currentHealth--;
        
        // Atualiza a UI
        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager != null) {
            uiManager.UpdateHealth(currentHealth);
        }
        
        // Efeitos visuais e sonoros
        if (hitEffect != null) {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        if (hitSound != null && audioSource != null) {
            audioSource.PlayOneShot(hitSound);
        }

        // Ativa invencibilidade temporária
        isInvincible = true;
        invincibilityTimer = invincibilityTime;
        Debug.Log("Player hit! Health: " + currentHealth);

        // Verifica game over
        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        state = PlayerState.Dead;
        Debug.Log("Game Over!");
        Invoke("RestartGame", 2f);
    }

    private void RestartGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void StartFalling() {
        isGrounded = false;
        isOnTrain = false;
        state = PlayerState.Jumping;
        if (vVelocity >= 0) {
            vVelocity = -5f;
        }
    }

    public void SetOnTrain(bool onTrain) {
        if (isOnTrain && !onTrain) {
            StartFalling();
        }
        isOnTrain = onTrain;
    }

    void ProcessAnimations() {
        // Atualiza as animações baseado no estado atual
        switch (state) {
            case PlayerState.Running:
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsRolling", false);
                break;
            
            case PlayerState.Jumping:
                animator.SetBool("IsJumping", true);
                animator.SetBool("IsRolling", false);
                break;
            
            case PlayerState.Rolling:
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsRolling", true);
                break;
            
            case PlayerState.Dead:
                animator.SetBool("IsJumping", false);
                animator.SetBool("IsRolling", false);
                // Aqui você pode adicionar uma animação de morte se tiver
                break;
        }
    }
}
