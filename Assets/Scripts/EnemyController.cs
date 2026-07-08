using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

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

    public void Initialize(Vector2 origin, IEnemyMovement movement)
    {
        this.origin = origin;
        this.movement = movement;
        transform.position = movement.GetPosition(origin, 0f);
    }

    void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        rb.MovePosition(movement.GetPosition(origin, elapsed));
    }

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
