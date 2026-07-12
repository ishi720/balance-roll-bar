using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の管理者。ステージ上のオブジェクトをすべてコードから生成し、
/// ライフ・タイマー・勝敗判定・UI表示を担う。
/// </summary>
public class GameManager : MonoBehaviour
{
    bool gameOver;
    bool gameWon;
    bool timeUp;
    bool paused;

    BarController bar;
    Collider2D barCollider;

    StageAttribute stage;
    StageAttribute[] stageOptions;
    bool started;
    bool showHowTo;

    Texture2D pauseOverlayTexture;

    /// <summary>現在選択中のステージ属性。タイトル画面でステージを選ぶまではnull。</summary>
    public StageAttribute Stage => stage;

    const float worldMinY = -6f;
    const float worldMaxY = 52f;

    const float timeLimit = 180f; // 3分
    float timeRemaining;

    const int maxLife = 3;
    const float invincibleDuration = 1.5f; // 被弾後の無敵時間
    int life;
    float invincibleTimer;

    /// <summary>現在ボールが無敵状態かどうか。</summary>
    public bool IsInvincible => invincibleTimer > 0f;

    /// <summary>タイトル画面用に選択可能なステージ一覧を用意する。ステージ生成はBeginStageまで行わない。</summary>
    void Start()
    {
        stageOptions = StageAttribute.CreateAllStages();

        Camera cam = Camera.main;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.15f);
    }

    /// <summary>選択されたステージでライフ・タイマーを初期化し、バー・ボール・ゴール・障害物・敵・壁・カメラを生成する。</summary>
    void BeginStage(StageAttribute chosen)
    {
        stage = chosen;
        timeRemaining = timeLimit;
        life = maxLife;
        invincibleTimer = 0f;
        gameOver = false;
        gameWon = false;
        timeUp = false;
        started = true;
        paused = false;
        Time.timeScale = 1f;

        Camera cam = Camera.main;
        float hw = cam.orthographicSize * cam.aspect;
        float hh = cam.orthographicSize;
        bool isDown = stage.scrollDirection == ScrollDirection.Down;

        float barStartY;
        float goalY;
        if (isDown)
        {
            // 障害物コースの最上段(46)より上に開始位置を置き、ゴールは最下段の下に置く
            barStartY = worldMaxY - 3f;
            goalY = worldMinY + 1f;
            // バーが画面上部付近に見えるようカメラの初期位置を合わせる
            cam.transform.position = new Vector3(cam.transform.position.x, barStartY - hh + 0.5f, cam.transform.position.z);
        }
        else
        {
            barStartY = -hh + 0.5f;
            goalY = worldMaxY - 1f;
        }

        bar = CreateBar(hw, barStartY);
        barCollider = bar.GetComponent<Collider2D>();

        var ball = CreateBall(bar.GetCenterY());
        CreateGoal(hw, goalY);
        CreateObstacles(hw);
        CreateEnemies();
        CreateWalls(hw);
        SetupCamera(cam, ball);
    }

    /// <summary>Escキーでポーズを切り替え、無敵時間を減少させ、残り時間を減らして時間切れならゲームオーバーにする。タイトル画面中は何もしない。</summary>
    void Update()
    {
        if (!started) return;

        if (!gameOver && !gameWon && Input.GetKeyDown(KeyCode.Escape))
            SetPaused(!paused);

        if (paused) return;

        if (invincibleTimer > 0f)
            invincibleTimer -= Time.deltaTime;

        if (gameOver || gameWon) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timeUp = true;
            gameOver = true;
        }
    }

    /// <summary>ポーズ状態を切り替え、Time.timeScaleで物理・タイマーの進行を止める/再開する。</summary>
    void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
    }

    /// <summary>ポーズ中に破棄された場合でもTime.timeScaleを元に戻す(エディタで再生停止した場合など)。</summary>
    void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    /// <summary>メインカメラにCameraControllerを追加し、追従対象・強制スクロール速度・スクロール範囲を設定する。</summary>
    void SetupCamera(Camera cam, GameObject ball)
    {
        var ctrl = cam.gameObject.AddComponent<CameraController>();
        ctrl.ball = ball.transform;
        ctrl.forcedScrollSpeed = stage.forcedScrollSpeed;
        ctrl.direction = stage.scrollDirection;
        ctrl.Initialize(worldMinY, worldMaxY);
    }

    // ---- factory methods ----

    /// <summary>バーを生成し、幅と初期位置を設定する。</summary>
    BarController CreateBar(float hw, float startY)
    {
        var go = new GameObject("Bar");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SquareSprite();
        sr.color = new Color(0.3f, 0.85f, 0.4f);
        sr.sortingOrder = 2;
        go.AddComponent<BoxCollider2D>();

        var ctrl = go.AddComponent<BarController>();
        ctrl.Initialize(hw, startY);
        return ctrl;
    }

    /// <summary>ボールを生成し、バーの中心の少し上に配置する。</summary>
    GameObject CreateBall(float barCenterY)
    {
        const float barHalfH = 0.18f / 2f;   // バー厚みの半分
        const float ballRadius = 0.35f / 2f;  // ボール半径 (scale * collider default radius 0.5)
        float spawnY = barCenterY + barHalfH + ballRadius + 0.05f;

        var go = new GameObject("Ball");
        go.tag = "Player";
        go.transform.position = new Vector3(0f, spawnY, 0f);
        go.transform.localScale = Vector3.one * 0.35f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CircleSprite();
        sr.color = new Color(1f, 0.55f, 0.1f);
        sr.sortingOrder = 3;

        var col = go.AddComponent<CircleCollider2D>();
        var mat = new PhysicsMaterial2D { friction = 0.7f, bounciness = 0.05f };
        col.sharedMaterial = mat;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = stage.ballGravityScale;
        rb.drag = 0.4f;
        rb.angularDrag = 0.6f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var bc = go.AddComponent<BallController>();
        bc.gameManager = this;

        return go;
    }

    /// <summary>ゴールゾーンを生成する。</summary>
    void CreateGoal(float hw, float goalY)
    {
        var go = new GameObject("Goal");
        go.transform.position = new Vector2(0f, goalY);
        go.transform.localScale = new Vector3(hw * 2f, 0.5f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SquareSprite();
        sr.color = new Color(1f, 0.95f, 0.1f, 0.6f);
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;

        var gt = go.AddComponent<GoalTrigger>();
        gt.gameManager = this;
    }

    // Obstacle rows: each entry is (centerY, gapCenterX, gapHalfWidth)
    static readonly (float y, float gx, float ghw)[] ObstacleData =
    {
        (-2f,  -2.5f, 1.1f),
        ( 1f,   2.5f, 1.1f),
        ( 4f,   0.0f, 1.1f),
        ( 7f,  -2.5f, 1.0f),
        (10f,   2.5f, 1.0f),
        (13f,   0.0f, 1.0f),
        (16f,  -2.5f, 1.0f),
        (19f,   2.5f, 0.95f),
        (22f,   0.0f, 0.95f),
        (25f,  -2.5f, 0.95f),
        (28f,   2.5f, 0.9f),
        (31f,   0.0f, 0.9f),
        (34f,  -2.5f, 0.9f),
        (37f,   2.5f, 0.9f),
        (40f,   0.0f, 0.85f),
        (43f,  -2.5f, 0.85f),
        (46f,   2.5f, 0.85f),
    };

    /// <summary>ObstacleDataの各行について、隙間を空けた左右の障害物を生成する。</summary>
    void CreateObstacles(float hw)
    {
        foreach (var (oy, gx, ghw) in ObstacleData)
        {
            float thickness = 0.28f;
            float leftWidth = hw + gx - ghw;
            if (leftWidth > 0.1f)
                SpawnObstaclePiece(-(hw - leftWidth * 0.5f), oy, leftWidth, thickness);
            float rightStart = gx + ghw;
            float rightWidth = hw - rightStart;
            if (rightWidth > 0.1f)
                SpawnObstaclePiece(rightStart + rightWidth * 0.5f, oy, rightWidth, thickness);
        }
    }

    /// <summary>障害物1個(左右どちらか片方)を生成し、バーとの衝突を無視させる。</summary>
    void SpawnObstaclePiece(float cx, float cy, float w, float h)
    {
        var go = new GameObject("Obstacle");
        go.transform.position = new Vector2(cx, cy);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SquareSprite();
        sr.color = new Color(0.85f, 0.2f, 0.2f);
        sr.sortingOrder = 1;

        var col = go.AddComponent<BoxCollider2D>();
        // bar passes through obstacles; ball does not
        Physics2D.IgnoreCollision(barCollider, col);
    }

    // Circle enemy spawns: each entry is (originX, originY, radius, angularSpeed deg/sec)
    static readonly (float ox, float oy, float radius, float angSpeed)[] CircleEnemyData =
    {
        (-2.5f,  6f, 1.0f,  80f),
        ( 2.5f, 15f, 1.2f, -60f),
        ( 0.0f, 24f, 1.4f,  70f),
        (-2.5f, 33f, 1.2f, -90f),
        ( 2.5f, 42f, 1.0f, 100f),
    };

    // Horizontal ping-pong enemy spawns: each entry is (originX, originY, amplitude, speed)
    static readonly (float ox, float oy, float amplitude, float speed)[] HorizontalEnemyData =
    {
        ( 0.0f,  9f, 1.5f, 2.0f),
        (-1.0f, 18f, 1.8f, 1.5f),
        ( 1.0f, 27f, 1.5f, 2.2f),
        ( 0.0f, 36f, 1.8f, 1.8f),
        (-1.0f, 45f, 1.5f, 2.0f),
    };

    /// <summary>CircleEnemyData・HorizontalEnemyDataに従って全ての敵を生成する。</summary>
    void CreateEnemies()
    {
        foreach (var (ox, oy, radius, angSpeed) in CircleEnemyData)
            SpawnEnemy(new Vector2(ox, oy), new CircleMovement(radius, angSpeed));

        foreach (var (ox, oy, amplitude, speed) in HorizontalEnemyData)
            SpawnEnemy(new Vector2(ox, oy), new HorizontalPingPongMovement(amplitude, speed));
    }

    /// <summary>敵1体を生成し、指定の移動パターンを適用する。</summary>
    void SpawnEnemy(Vector2 origin, IEnemyMovement movement)
    {
        var go = new GameObject("Enemy");
        go.transform.localScale = Vector3.one * 0.6f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = CircleSprite();
        sr.color = new Color(0.75f, 0.1f, 0.9f);
        sr.sortingOrder = 3;

        go.AddComponent<CircleCollider2D>();

        var enemy = go.AddComponent<EnemyController>();
        enemy.gameManager = this;
        enemy.Initialize(origin, movement);
    }

    /// <summary>左右の壁を生成する。</summary>
    void CreateWalls(float hw)
    {
        float wallHeight = worldMaxY - worldMinY;
        float wallCenterY = (worldMaxY + worldMinY) * 0.5f;
        SpawnWall(-hw - 0.5f, wallCenterY, 1f, wallHeight);
        SpawnWall( hw + 0.5f, wallCenterY, 1f, wallHeight);
    }

    /// <summary>壁1枚を生成する。</summary>
    void SpawnWall(float x, float y, float w, float h)
    {
        var go = new GameObject("Wall");
        go.transform.position = new Vector2(x, y);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SquareSprite();
        sr.color = new Color(0.25f, 0.25f, 0.4f);
        sr.sortingOrder = 0;

        go.AddComponent<BoxCollider2D>();
    }

    // ---- sprite helpers ----

    /// <summary>1x1の単色スプライトを生成する。</summary>
    static Sprite SquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary>塗りつぶし円形のスプライトを生成する。</summary>
    static Sprite CircleSprite()
    {
        const int S = 64;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        float r = S * 0.5f - 1f;
        var center = new Vector2(S * 0.5f, S * 0.5f);
        for (int x = 0; x < S; x++)
            for (int y = 0; y < S; y++)
                tex.SetPixel(x, y, Vector2.Distance(new Vector2(x, y), center) <= r
                    ? Color.white : Color.clear);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), S);
    }

    // ---- game events ----

    /// <summary>ボールが画面外に落ちた際に呼ばれ、ゲームオーバーにする。</summary>
    public void OnBallFell()
    {
        if (gameOver || gameWon) return;
        gameOver = true;
    }

    /// <summary>ボールがゴールに到達した際に呼ばれ、ゲームクリアにする。</summary>
    public void OnBallReachedGoal()
    {
        if (gameOver || gameWon) return;
        gameWon = true;
    }

    /// <summary>
    /// ボールが敵に接触した際に呼ばれる。無敵時間中は無視し、そうでなければライフを1減らして
    /// 無敵時間を開始する。ライフが尽きるとゲームオーバーにする。
    /// </summary>
    public void OnEnemyHit()
    {
        if (gameOver || gameWon) return;
        if (invincibleTimer > 0f) return;

        life--;
        invincibleTimer = invincibleDuration;
        if (life <= 0)
            gameOver = true;
    }

    // ---- UI ----

    /// <summary>タイトル画面・操作説明・ライフ・タイマー・勝敗メッセージをIMGUIで描画する。</summary>
    void OnGUI()
    {
        if (showHowTo)
        {
            DrawHowToScreen();
            return;
        }

        if (!started)
        {
            DrawTitleScreen();
            return;
        }

        var info = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        string goalHint = stage.scrollDirection == ScrollDirection.Down ? "下のゴールへ" : "上のゴールへ";
        GUI.Label(new Rect(10, 10, 320, 50),
            "赤い壁の隙間を通って" + goalHint + "！\nEsc: ポーズ", info);

        var lifeStyle = new GUIStyle(GUI.skin.label) { fontSize = 20 };
        GUI.Label(new Rect(10, 90, 200, 30), "ライフ: " + new string('♥', Mathf.Max(life, 0)), lifeStyle);

        var timerStyle = new GUIStyle(GUI.skin.label) { fontSize = 28, alignment = TextAnchor.UpperRight };
        timerStyle.normal.textColor = timeRemaining <= 30f ? Color.red : Color.white;
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        GUI.Label(new Rect(Screen.width - 160 - 10, 10, 160, 40),
            string.Format("{0:00}:{1:00}", minutes, seconds), timerStyle);

        if (paused)
        {
            DrawPauseMenu();
            return;
        }

        if (!gameWon && !gameOver) return;

        var big = new GUIStyle(GUI.skin.label) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
        big.normal.textColor = gameWon ? Color.yellow : Color.red;

        string message = gameWon ? "クリア！" : (timeUp ? "タイムアップ" : "ゲームオーバー");

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.Label(new Rect(0, cy - 60, Screen.width, 80), message, big);

        if (GUI.Button(new Rect(cx - 70, cy + 40, 140, 40), "タイトルへ戻る"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>ポーズ中に半透明の背景とメニューを描画し、プレイ再開・タイトルへ戻る操作を提供する。</summary>
    void DrawPauseMenu()
    {
        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;

        if (pauseOverlayTexture == null)
        {
            pauseOverlayTexture = new Texture2D(1, 1);
            pauseOverlayTexture.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.6f));
            pauseOverlayTexture.Apply();
        }
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), pauseOverlayTexture);

        var big = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
        big.normal.textColor = Color.white;
        GUI.Label(new Rect(0, cy - 100, Screen.width, 60), "ポーズ中", big);

        if (GUI.Button(new Rect(cx - 70, cy - 20, 140, 40), "プレイ画面に戻る"))
            SetPaused(false);

        if (GUI.Button(new Rect(cx - 70, cy + 30, 140, 40), "操作方法"))
            showHowTo = true;

        if (GUI.Button(new Rect(cx - 70, cy + 80, 140, 40), "タイトルへ戻る"))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>タイトル画面を描画し、ステージ選択ボタンからBeginStageを呼び出す。</summary>
    void DrawTitleScreen()
    {
        float cx = Screen.width * 0.5f;

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 40, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 30, Screen.width, 50), "Balance Roll Bar", titleStyle);

        var subStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleCenter };
        subStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 80, Screen.width, 26), "ステージを選んでください", subStyle);

        if (GUI.Button(new Rect(Screen.width - 130, 10, 120, 36), "操作方法"))
            showHowTo = true;

        const float buttonW = 280f;
        const float buttonH = 46f;
        const float descH = 18f;
        const float gap = 14f;
        float startY = 116f;

        var nameStyle = new GUIStyle(GUI.skin.button) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
        var descStyle = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleCenter, wordWrap = true };
        descStyle.normal.textColor = new Color(0.85f, 0.85f, 0.85f);

        for (int i = 0; i < stageOptions.Length; i++)
        {
            var option = stageOptions[i];
            float y = startY + i * (buttonH + descH + gap);
            if (GUI.Button(new Rect(cx - buttonW * 0.5f, y, buttonW, buttonH), option.displayName, nameStyle))
                BeginStage(option);
            GUI.Label(new Rect(cx - buttonW * 0.5f, y + buttonH + 2f, buttonW, descH), option.description, descStyle);
        }
    }

    /// <summary>操作方法ページを描画し、タイトル画面へ戻るボタンを提供する。</summary>
    void DrawHowToScreen()
    {
        float cx = Screen.width * 0.5f;

        var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 32, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;
        GUI.Label(new Rect(0, 30, Screen.width, 44), "操作方法", titleStyle);

        var bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, alignment = TextAnchor.UpperLeft, wordWrap = true };
        bodyStyle.normal.textColor = Color.white;

        string body =
            "・左端のバー: W キーで上げる / S キーで下げる\n" +
            "・右端のバー: ↑ キーで上げる / ↓ キーで下げる\n" +
            "・Esc キーでポーズ / 再開";

        float boxW = Mathf.Min(560f, Screen.width - 80f);
        GUI.Label(new Rect(cx - boxW * 0.5f, 100, boxW, 120), body, bodyStyle);

        if (GUI.Button(new Rect(cx - 70, 240, 140, 40), "戻る"))
            showHowTo = false;
    }
}
