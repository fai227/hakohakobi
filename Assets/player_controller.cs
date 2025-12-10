using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class player_controller : MonoBehaviour
{
    public float moveSpeed = 5f;  // 移動速度

    private Rigidbody rb;
    private Vector3 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // 回転しないように固定
    }

    void Update()
    {
        moveInput = Vector3.zero;

        // --- WASDキー入力 ---
        if (Input.GetKey(KeyCode.W)) moveInput += Vector3.forward;
        if (Input.GetKey(KeyCode.S)) moveInput += Vector3.back;
        if (Input.GetKey(KeyCode.A)) moveInput += Vector3.left;
        if (Input.GetKey(KeyCode.D)) moveInput += Vector3.right;

        moveInput = moveInput.normalized; // 対角移動でも速度一定
    }

    void FixedUpdate()
    {
        // Rigidbodyを使って物理的に移動
        Vector3 targetPos = rb.position + moveInput * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPos);
    }
}
