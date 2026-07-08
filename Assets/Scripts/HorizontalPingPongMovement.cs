using UnityEngine;

// origin を中心に左右(X軸方向)へ往復移動するパターン
public class HorizontalPingPongMovement : IEnemyMovement
{
    readonly float amplitude;
    readonly float speed;

    public HorizontalPingPongMovement(float amplitude, float speed)
    {
        this.amplitude = amplitude;
        this.speed = speed;
    }

    public Vector2 GetPosition(Vector2 origin, float elapsed)
    {
        float offset = Mathf.Sin(elapsed * speed) * amplitude;
        return origin + new Vector2(offset, 0f);
    }
}
