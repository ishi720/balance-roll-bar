using UnityEngine;

// origin を中心に円を描くように移動するパターン
public class CircleMovement : IEnemyMovement
{
    readonly float radius;
    readonly float angularSpeed; // deg/sec (負の値で反時計回り)
    readonly float startAngleDeg;

    public CircleMovement(float radius, float angularSpeed, float startAngleDeg = 0f)
    {
        this.radius = radius;
        this.angularSpeed = angularSpeed;
        this.startAngleDeg = startAngleDeg;
    }

    public Vector2 GetPosition(Vector2 origin, float elapsed)
    {
        float angle = (startAngleDeg + angularSpeed * elapsed) * Mathf.Deg2Rad;
        return origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }
}
