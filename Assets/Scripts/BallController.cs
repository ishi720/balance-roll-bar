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

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    /// <summary>速度がmaxSpeedを超えないようクランプする。</summary>
    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    /// <summary>カメラ下端より下に落ちたら、ゲーム管理者に落下を通知する。</summary>
    void Update()
    {
        Camera cam = Camera.main;
        float bottomLimit = cam.transform.position.y - cam.orthographicSize - 1f;
        if (transform.position.y < bottomLimit)
            gameManager.OnBallFell();
    }
}
