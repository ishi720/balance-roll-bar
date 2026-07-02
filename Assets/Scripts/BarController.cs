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
    private Transform ballTransform;
    private CircleCollider2D ballCollider;

    private static readonly RaycastHit2D[] _castBuffer = new RaycastHit2D[8];

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        cam = Camera.main;
    }

    void Start()
    {
        var ballGO = GameObject.FindWithTag("Player");
        if (ballGO != null)
        {
            ballTransform = ballGO.transform;
            ballCollider = ballGO.GetComponent<CircleCollider2D>();
        }
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

        float prevLeftY = leftY;
        float prevRightY = rightY;

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

        // 棒の上面が上昇 → ボールが棒の上にある → ボールの上が塞がれている
        // の条件が揃ったときだけ移動を止める
        if (ShouldBlockMovement(prevLeftY, prevRightY, leftY, rightY))
        {
            leftY = prevLeftY;
            rightY = prevRightY;
        }

        Apply();
    }

    // 棒の傾きを考慮した、ボールのX座標における棒上面のY座標
    float BarTopYAtX(float lY, float rY, float x)
    {
        float hw = barHalfWidth;
        float dy = rY - lY;
        float barLen = Mathf.Sqrt(4f * hw * hw + dy * dy);
        float spineY = lY + (x + hw) / (2f * hw) * dy;
        return spineY + 0.09f * (2f * hw / barLen); // 半厚み × cos(angle)
    }

    bool ShouldBlockMovement(float prevL, float prevR, float newL, float newR)
    {
        if (ballTransform == null) return false;

        float ballX = Mathf.Clamp(ballTransform.position.x, -barHalfWidth, barHalfWidth);
        float ballY = ballTransform.position.y;

        // 条件1: ボールのいるX位置で棒の上面が上昇しているか
        float prevTop = BarTopYAtX(prevL, prevR, ballX);
        float newTop = BarTopYAtX(newL, newR, ballX);
        if (newTop <= prevTop) return false;

        // 条件2: ボールが棒の上にあるか（棒の上面付近またはそれより上）
        float ballRadius = ballCollider != null
            ? ballCollider.radius * ballTransform.localScale.x
            : 0.175f;
        if (ballY < prevTop - ballRadius) return false;

        // 条件3: ボールの真上に障害物があるか
        return IsBallBlockedAbove((Vector2)ballTransform.position, ballRadius);
    }

    bool IsBallBlockedAbove(Vector2 ballPos, float radius)
    {
        int count = Physics2D.CircleCastNonAlloc(
            ballPos,
            radius * 0.8f,
            Vector2.up,
            _castBuffer,
            radius * 1.5f
        );

        for (int i = 0; i < count; i++)
        {
            var col = _castBuffer[i].collider;
            if (col == null) continue;
            if (col.CompareTag("Player")) continue;   // ボール自身は除外
            if (col.gameObject == gameObject) continue; // 棒自身は除外
            if (col.isTrigger) continue;               // ゴール等のトリガーは除外
            return true;
        }
        return false;
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
