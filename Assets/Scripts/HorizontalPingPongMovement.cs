using UnityEngine;

/// <summary>
/// origin を中心に左右(X軸方向)へ往復移動するパターン。
/// </summary>
public class HorizontalPingPongMovement : IEnemyMovement
{
    readonly float amplitude;
    readonly float speed;

    /// <summary>左右往復パターンを生成する。</summary>
    /// <param name="amplitude">中心からの最大振れ幅。</param>
    /// <param name="speed">振動の速さ(sin関数への角速度)。</param>
    public HorizontalPingPongMovement(float amplitude, float speed)
    {
        this.amplitude = amplitude;
        this.speed = speed;
    }

    /// <inheritdoc/>
    public Vector2 GetPosition(Vector2 origin, float elapsed)
    {
        float offset = Mathf.Sin(elapsed * speed) * amplitude;
        return origin + new Vector2(offset, 0f);
    }
}
