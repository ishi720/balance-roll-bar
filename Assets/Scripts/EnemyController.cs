using UnityEngine;

/// <summary>
/// 敵本体。IEnemyMovement で指定された移動パターンに従って動き、
/// ボールと衝突するとダメージ通知とノックバックを行う。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    /// <summary>被弾通知を送る先のゲーム管理者。</summary>
    [HideInInspector] public GameManager gameManager;

    /// <summary>ボールに衝突した際に加えるノックバックの速度。</summary>
    public float knockbackForce = 12f;

    Vector2 origin;
    IEnemyMovement movement;
    float elapsed;
    Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    /// <summary>基準点と移動パターンを設定し、初期位置に配置する。</summary>
    /// <param name="origin">移動パターンの基準となる座標。</param>
    /// <param name="movement">適用する移動パターン。</param>
    public void Initialize(Vector2 origin, IEnemyMovement movement)
    {
        this.origin = origin;
        this.movement = movement;
        transform.position = movement.GetPosition(origin, 0f);
    }

    /// <summary>経過時間から移動パターンの座標を計算し、Rigidbody2Dへ反映する。</summary>
    void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        rb.MovePosition(movement.GetPosition(origin, elapsed));
    }

    /// <summary>ボールと衝突したらダメージを通知し、衝突方向へボールを吹き飛ばす。</summary>
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        gameManager.OnEnemyHit();

        var ballRb = collision.rigidbody;
        if (ballRb != null)
        {
            Vector2 dir = ((Vector2)ballRb.transform.position - (Vector2)transform.position).normalized;
            ballRb.velocity = dir * knockbackForce;
        }
    }
}
