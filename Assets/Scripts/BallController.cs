using UnityEngine;

/// <summary>
/// ボールに付与し、速度上限のクランプと画面外落下の検知を行う。
/// </summary>
public class BallController : MonoBehaviour
{
    /// <summary>落下通知を送る先のゲーム管理者。</summary>
    [HideInInspector] public GameManager gameManager;

    /// <summary>ボールの最大速度。</summary>
    public float maxSpeed = 15f;

    /// <summary>無敵時間中の点滅速度(1秒あたりの点滅回数)。</summary>
    public float invincibleBlinkSpeed = 12f;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Color baseColor;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
    }

    /// <summary>風の影響を加えたうえで、速度がmaxSpeedを超えないようクランプする。</summary>
    void FixedUpdate()
    {
        if (gameManager != null && gameManager.Stage != null && gameManager.Stage.windForce != Vector2.zero)
            rb.AddForce(gameManager.Stage.windForce);

        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    /// <summary>カメラ下端より下に落ちたら、ゲーム管理者に落下を通知する。無敵時間中はボールを点滅させる。</summary>
    void Update()
    {
        Camera cam = Camera.main;
        float bottomLimit = cam.transform.position.y - cam.orthographicSize - 1f;
        if (transform.position.y < bottomLimit)
            gameManager.OnBallFell();

        UpdateInvincibleEffect();
    }

    /// <summary>無敵中はアルファ値を点滅させ、無敵でなければ元の色に戻す。</summary>
    void UpdateInvincibleEffect()
    {
        if (gameManager != null && gameManager.IsInvincible)
        {
            float alpha = Mathf.Lerp(0.25f, 1f, 0.5f + 0.5f * Mathf.Sin(Time.time * invincibleBlinkSpeed));
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
        }
        else if (sr.color != baseColor)
        {
            sr.color = baseColor;
        }
    }
}
