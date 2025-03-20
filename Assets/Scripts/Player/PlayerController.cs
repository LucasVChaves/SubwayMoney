using UnityEngine;
using UnityEngine.InputSystem;

enum PlayerState {
    Running,
    Jumping,
    Rolling
}

public class PlayerController : MonoBehaviour {
    [Header("Faixas")]
    public float laneDist = 3f;
    private int currLane = 1; // 0 = esquerda, 1 = centro, 2 = direita
    private float[] lanePos;
    
    [Header("Movimento do Player")]
    public float jumpForce = 1000f;
    public float gravity = -9.81f;
    public float fastFallMultiplier = 2f;
    public float hLerpSpeed = 10f;
    public float groundLevel = 0f;

    private float vVelocity = 0f;
    private bool isGrounded = true;

    private Vector3 targetPos;

    private PlayerState state = PlayerState.Running;
    void Start() {
        lanePos = new float[3];
        lanePos[0] = -laneDist;
        lanePos[1] = 0f;
        lanePos[2] = laneDist;

        // inicia o player centralizado
        targetPos = new Vector3(lanePos[currLane], transform.position.y, transform.position.z);
    }

    void Update() {
        ProcessInput();
    }

    void FixedUpdate() {
        HandleMovement();
    }
    void ProcessInput() {
        // Movimentação lateral
        if (Keyboard.current.aKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame) {
            if (currLane > 0) {
                currLane--;
                targetPos.x = lanePos[currLane];
            }
        }
        if (Keyboard.current.dKey.wasPressedThisFrame || Keyboard.current.rightArrowKey.wasPressedThisFrame) {
            if (currLane < 2) {
                currLane++;
                targetPos.x = lanePos[currLane];
            }
        }

        // Pulo
        if (isGrounded && (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)) {
            Debug.Log("Jump");
            vVelocity = Mathf.Sqrt(jumpForce * -2f * gravity);
            isGrounded = false;
            state = PlayerState.Jumping;
            Debug.Log("State = " + state);
        }

        // Cancelamento do pulo (fast fall)
        if (!isGrounded && (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)) {
            if (vVelocity != 0) {
                vVelocity = gravity;
            }
            state = PlayerState.Rolling;
        }
    }

    void HandleMovement() {
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(newPosition.x, targetPos.x, hLerpSpeed * Time.fixedDeltaTime);

        float currentGravity = gravity;
        if (state == PlayerState.Rolling && vVelocity <= 0) {
            currentGravity *= fastFallMultiplier;
        }

        vVelocity += currentGravity * Time.fixedDeltaTime;

        newPosition.y += vVelocity * Time.fixedDeltaTime;

        if (newPosition.y <= groundLevel) {
            newPosition.y = groundLevel;
            isGrounded = true;
            vVelocity = 0f;
            state = PlayerState.Running;
        } else {
            isGrounded = false;
        }

        transform.position = newPosition;
    }
}
