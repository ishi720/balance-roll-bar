using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [HideInInspector] public GameManager gameManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameManager.OnBallReachedGoal();
    }
}
