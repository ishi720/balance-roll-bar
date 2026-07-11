using UnityEngine;

/// <summary>ステージのスクロール方向。現状Up/Downを実装済みで、Left/Rightは将来のステージ用に予約。</summary>
public enum ScrollDirection
{
    Up,
    Down,
    Left,
    Right,
}

/// <summary>
/// ステージ固有の属性(スクロール方向・重力・風・強制スクロール)を保持するデータ。
/// ステージごとにアセット化して差し替えられるようにScriptableObjectとして定義する。
/// </summary>
[CreateAssetMenu(fileName = "StageAttribute", menuName = "BalanceRollBar/Stage Attribute")]
public class StageAttribute : ScriptableObject
{
    [Header("表示")]
    public string displayName = "Stage";
    public string description = "";

    [Header("スクロール")]
    public ScrollDirection scrollDirection = ScrollDirection.Up;

    [Header("重力・風")]
    public float ballGravityScale = 2.5f;
    public Vector2 windForce = Vector2.zero;

    [Header("強制スクロール")]
    [Tooltip("0の場合はボール追従スクロール。0より大きい場合は自動的にこの速度でスクロールする。")]
    public float forcedScrollSpeed = 0f;

    static StageAttribute Create(string name, string desc, ScrollDirection direction, float gravityScale, Vector2 wind, float forcedScroll)
    {
        var stage = CreateInstance<StageAttribute>();
        stage.name = name;
        stage.displayName = name;
        stage.description = desc;
        stage.scrollDirection = direction;
        stage.ballGravityScale = gravityScale;
        stage.windForce = wind;
        stage.forcedScrollSpeed = forcedScroll;
        return stage;
    }

    /// <summary>現行のゲームと同一の設定を持つ「上スクロール」ステージを生成する。</summary>
    public static StageAttribute CreateDefaultUpScroll() =>
        Create("ノーマル", "標準的なバランスゲーム", ScrollDirection.Up, 2.5f, Vector2.zero, 0f);

    /// <summary>重力が弱く、ボールがふわふわ浮くステージを生成する。</summary>
    public static StageAttribute CreateLowGravity() =>
        Create("低重力", "ボールがふわふわ浮いて操作しづらい", ScrollDirection.Up, 1.1f, Vector2.zero, 0f);

    /// <summary>常に横風が吹き続けるステージを生成する。</summary>
    public static StageAttribute CreateWindy() =>
        Create("強風", "常に右向きの風でボールが流される", ScrollDirection.Up, 2.5f, new Vector2(2.2f, 0f), 0f);

    /// <summary>カメラが自動で上へスクロールし続けるステージを生成する。</summary>
    public static StageAttribute CreateForcedScroll() =>
        Create("強制スクロール", "カメラが自動で上昇し続ける。置いていかれると即落下扱い", ScrollDirection.Up, 2.5f, Vector2.zero, 2.0f);

    /// <summary>ゴールが下にあり、ボールを落としながら下へ進む「下スクロール」ステージを生成する。</summary>
    public static StageAttribute CreateDownScroll() =>
        Create("下スクロール", "ゴールは下。落ちながら隙間を縫って進む", ScrollDirection.Down, 2.5f, Vector2.zero, 0f);

    /// <summary>タイトル画面で選択可能な5ステージを生成する。</summary>
    public static StageAttribute[] CreateAllStages() => new[]
    {
        CreateDefaultUpScroll(),
        CreateLowGravity(),
        CreateWindy(),
        CreateForcedScroll(),
        CreateDownScroll(),
    };
}
