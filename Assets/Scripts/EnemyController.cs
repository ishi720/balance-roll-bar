using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

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
        if (collision.gameObject.CompareTag("Player"))
            gameManager.OnEnemyHit();
    }
}
