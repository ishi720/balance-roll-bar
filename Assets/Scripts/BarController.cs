using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class BarController : MonoBehaviour
{
    public float moveSpeed = 4f;

    [HideInInspector] public float leftY;
    [HideInInspector] public float rightY;
    [HideInInspector] public float barHalfWidth;

    private Rigidbody2D rb;
    private Camera cam;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        cam = Camera.main;
    }

    public void Initialize(float halfWidth, float startY)
    {
        barHalfWidth = halfWidth;
        leftY = startY;
        rightY = startY;
        transform.localScale = new Vector3(halfWidth * 2f, 0.18f, 1f);
        // MovePosition は物理ステップまで反映されないため直接セット
        transform.position = new Vector3(0f, startY, 0f);
        transform.rotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        if (Input.GetKey(KeyCode.W)) leftY += moveSpeed * dt;
        if (Input.GetKey(KeyCode.S)) leftY -= moveSpeed * dt;
        if (Input.GetKey(KeyCode.UpArrow)) rightY += moveSpeed * dt;
        if (Input.GetKey(KeyCode.DownArrow)) rightY -= moveSpeed * dt;

        float camCenterY = cam.transform.position.y;
        float camHH = cam.orthographicSize;
        float dynMinY = camCenterY - camHH + 0.3f;
        float dynMaxY = camCenterY + camHH - 0.3f;

        leftY = Mathf.Clamp(leftY, dynMinY, dynMaxY);
        rightY = Mathf.Clamp(rightY, dynMinY, dynMaxY);

        Apply();
    }

    void Apply()
    {
        float centerY = (leftY + rightY) * 0.5f;
        float dx = barHalfWidth * 2f;
        float dy = rightY - leftY;
        // 端点間の実距離でスケールを更新し、画面端から端まで常に届かせる
        float barLength = Mathf.Sqrt(dx * dx + dy * dy);
        transform.localScale = new Vector3(barLength, 0.18f, 1f);
        // atan2(dy, dx) が正 → CCW → 右端が上がる（符号そのまま）
        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
        rb.MovePosition(new Vector2(0f, centerY));
        rb.MoveRotation(angle);
    }

    public float GetCenterY() => (leftY + rightY) * 0.5f;
}
