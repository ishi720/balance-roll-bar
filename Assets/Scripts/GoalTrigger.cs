using UnityEngine;

/// <summary>
/// ゴールゾーンのトリガーコライダーに付与し、ボールの到達を検知する。
/// </summary>
public class GoalTrigger : MonoBehaviour
{
    /// <summary>ゴール到達を通知する先のゲーム管理者。</summary>
    [HideInInspector] public GameManager gameManager;

    /// <summary>ボールが触れたらゴール到達をゲーム管理者に通知する。</summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            gameManager.OnBallReachedGoal();
    }
}
