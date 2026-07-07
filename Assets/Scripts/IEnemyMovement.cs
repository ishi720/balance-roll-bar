using UnityEngine;

// 敵の移動パターンを表すインターフェース
// パターンごとにこれを実装したクラスを作り、EnemyController に差し込む
public interface IEnemyMovement
{
    Vector2 GetPosition(Vector2 origin, float elapsed);
}
