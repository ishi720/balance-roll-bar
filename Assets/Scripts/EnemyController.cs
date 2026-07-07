using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    Vector2 origin;
    IEnemyMovement movement;
    float elapsed;

    public void Initialize(Vector2 origin, IEnemyMovement movement)
    {
        this.origin = origin;
        this.movement = movement;
        transform.position = movement.GetPosition(origin, 0f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        transform.position = movement.GetPosition(origin, elapsed);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameManager.OnEnemyHit();
    }
}
