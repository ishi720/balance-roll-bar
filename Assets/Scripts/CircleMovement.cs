using UnityEngine;

/// <summary>
/// origin を中心に円を描くように移動するパターン。
/// </summary>
public class CircleMovement : IEnemyMovement
{
    readonly float radius;
    readonly float angularSpeed; // deg/sec (負の値で反時計回り)
    readonly float startAngleDeg;

    /// <summary>円運動パターンを生成する。</summary>
    /// <param name="radius">円運動の半径。</param>
    /// <param name="angularSpeed">角速度(deg/sec)。負の値で反時計回りになる。</param>
    /// <param name="startAngleDeg">開始角度(deg)。</param>
    public CircleMovement(float radius, float angularSpeed, float startAngleDeg = 0f)
    {
        this.radius = radius;
        this.angularSpeed = angularSpeed;
        this.startAngleDeg = startAngleDeg;
    }

    /// <inheritdoc/>
    public Vector2 GetPosition(Vector2 origin, float elapsed)
    {
        float angle = (startAngleDeg + angularSpeed * elapsed) * Mathf.Deg2Rad;
        return origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
