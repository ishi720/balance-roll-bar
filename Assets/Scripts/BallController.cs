using UnityEngine;

public class BallController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    private float bottomLimit;

    void Start()
    {
        Camera cam = Camera.main;
        bottomLimit = cam.transform.position.y - cam.orthographicSize - 1f;
    }

    void Update()
    {
        if (transform.position.y < bottomLimit)
            gameManager.OnBallFell();
    }
}
