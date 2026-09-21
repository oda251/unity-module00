using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10.0f;
    [SerializeField] private float acceleration = 30.0f; // 加速の強さ（慣性の調整）
    [SerializeField] private float deceleration = 25.0f; // 減速の強さ（すべり具合の調整）
    [SerializeField] private float jumpForce = 5.0f;

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;

    [Header("Hazard Settings")]
    [SerializeField] private LayerMask lavaLayer; // Lavaレイヤーを指定する設定項目を追加

    private Rigidbody rb;
    private SphereCollider sphereCollider;
    private bool isGrounded;
    private Vector2 inputVector;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
    {
        // 地上判定
        float radius = sphereCollider.radius * transform.localScale.y;
        Vector3 rayOrigin = transform.position + Vector3.down * (radius - 0.05f);

        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, groundCheckDistance, groundLayer, QueryTriggerInteraction.Ignore);

        // キーボード入力処理
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            float x = 0f;
            float z = 0f;

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) z -= 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) z += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) x += 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) x -= 1f;

            inputVector = new Vector2(x, z).normalized;

            if (keyboard.spaceKey.wasPressedThisFrame && isGrounded)
            {
                Jump();
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(inputVector.x, 0f, inputVector.y);
        Vector3 targetVelocity = moveDirection * moveSpeed;

        // 現在の水平速度を取得
        Vector3 currentHorizontalVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // 入力の有無によって加減速の割合を切り替える
        float currentAccel = (moveDirection.magnitude > 0.01f) ? acceleration : deceleration;

        Vector3 newHorizontalVelocity = Vector3.MoveTowards(
            currentHorizontalVelocity,
            targetVelocity,
            currentAccel * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector3(newHorizontalVelocity.x, rb.linearVelocity.y, newHorizontalVelocity.z);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckLavaCollision(collision.gameObject);
    }

    private void CheckLavaCollision(GameObject hitObject)
    {
        // 衝突相手のレイヤーが lavaLayer に含まれているか判定
        if (((1 << hitObject.layer) & lavaLayer) != 0)
        {
            Debug.Log("Game Over");
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (sphereCollider == null) sphereCollider = GetComponent<SphereCollider>();
        if (sphereCollider != null)
        {
            float radius = sphereCollider.radius * transform.localScale.y;
            Vector3 rayOrigin = transform.position + Vector3.down * (radius - 0.05f);

            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * groundCheckDistance);
        }
    }
}
