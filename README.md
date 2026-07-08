# Balance Roll Bar

バーを傾けてボールを転がし、障害物の隙間を通り抜けてゴールを目指す2Dバランスゲームです。

## ゲーム概要

画面下部にある緑のバーの左右を独立して上下に動かし、ボールをコントロールします。  
赤い障害物の隙間を通り抜け、画面上部の黄色いゴールゾーンにボールを届けてください。  
ボールが画面外に落ちるとゲームオーバーです。

## 操作方法

| キー | 動作 |
|------|------|
| `W` | バーの**左端**を上げる |
| `S` | バーの**左端**を下げる |
| `↑` | バーの**右端**を上げる |
| `↓` | バーの**右端**を下げる |

## 開発環境

- **エンジン**: Unity 2D
- **言語**: C#

## プロジェクト構成

```
Assets/
├── Scenes/
│   └── SampleScene.unity           # メインシーン
└── Scripts/
    ├── GameManager.cs              # ゲーム全体の管理・オブジェクト生成・ライフ/タイマー管理
    ├── BarController.cs            # バーの移動・傾き制御
    ├── BallController.cs           # ボールの落下検知
    ├── CameraController.cs         # ボールを追従するカメラ制御
    ├── GoalTrigger.cs              # ゴール判定
    ├── EnemyController.cs          # 敵本体(移動パターンの適用・被弾処理・ノックバック)
    ├── IEnemyMovement.cs           # 敵の移動パターンのインターフェース
    ├── CircleMovement.cs           # 移動パターン: 円運動
    └── HorizontalPingPongMovement.cs  # 移動パターン: 左右往復移動
```

## 実行方法

1. Unity (2022.x 以上推奨) でプロジェクトを開く
2. `Assets/Scenes/SampleScene.unity` を開く
3. Playボタンを押してゲームを開始する
