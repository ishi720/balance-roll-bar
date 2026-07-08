using UnityEngine;

/// <summary>
/// 敵の移動パターンを表すインターフェース。
/// パターンごとにこれを実装したクラスを作り、EnemyController に差し込む。
/// </summary>
public interface IEnemyMovement
{
    /// <summary>
    /// 基準点(origin)と経過時間(elapsed)から、現在のワールド座標を計算する。
    /// </summary>
    Vector2 GetPosition(Vector2 origin, float elapsed);
}
