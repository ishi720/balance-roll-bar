using UnityEngine;

public class BallController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    public float maxSpeed = 15f;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }

    void Update()
    {
        Camera cam = Camera.main;
        float bottomLimit = cam.transform.position.y - cam.orthographicSize - 1f;
        if (transform.position.y < bottomLimit)
            gameManager.OnBallFell();
    }
}
