using UnityEngine;

public class BallController : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    void Update()
    {
        Camera cam = Camera.main;
        float bottomLimit = cam.transform.position.y - cam.orthographicSize - 1f;
        if (transform.position.y < bottomLimit)
            gameManager.OnBallFell();
    }
}
