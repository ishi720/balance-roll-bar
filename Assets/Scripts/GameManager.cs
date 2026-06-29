using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    bool gameOver;
    bool gameWon;

    BarController bar;
    Collider2D barCollider;

    const float worldMinY = -6f;
    const float worldMaxY = 52f;

    void Start()
    {
        Camera cam = Camera.main;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.15f);

        float hw = cam.orthographicSize * cam.aspect;
        float hh = cam.orthographicSize;

        bar = CreateBar(hw, -hh + 0.5f);
        barCollider = bar.GetComponent<Collider2D>();

        var ball = CreateBall(bar.GetCenterY());
        CreateGoal(hw, worldMaxY - 1f);
        CreateObstacles(hw);
        CreateWalls(hw);
        SetupCamera(cam, ball);
    }

    void SetupCamera(Camera cam, GameObject ball)
    {
        var ctrl = cam.gameObject.AddComponent<CameraController>();
        ctrl.ball = ball.transform;
        ctrl.Initialize(worldMinY, worldMaxY);
    }

    // ---- factory methods ----

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
        rb.gravityScale = 2.5f;
        rb.drag = 0.4f;
        rb.angularDrag = 0.6f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var bc = go.AddComponent<BallController>();
        bc.gameManager = this;

        return go;
    }

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

    void CreateWalls(float hw)
    {
        float wallHeight = worldMaxY - worldMinY;
        float wallCenterY = (worldMaxY + worldMinY) * 0.5f;
        SpawnWall(-hw - 0.5f, wallCenterY, 1f, wallHeight);
        SpawnWall( hw + 0.5f, wallCenterY, 1f, wallHeight);
    }

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

    static Sprite SquareSprite()
    {
        var tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

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

    public void OnBallFell()
    {
        if (gameOver || gameWon) return;
        gameOver = true;
    }

    public void OnBallReachedGoal()
    {
        if (gameOver || gameWon) return;
        gameWon = true;
    }

    // ---- UI ----

    void OnGUI()
    {
        var info = new GUIStyle(GUI.skin.label) { fontSize = 16 };
        GUI.Label(new Rect(10, 10, 320, 80),
            "左端: W (上) / S (下)\n右端: ↑ (上) / ↓ (下)\n赤い壁の隙間を通って上のゴールへ！", info);

        if (!gameWon && !gameOver) return;

        var big = new GUIStyle(GUI.skin.label) { fontSize = 48, alignment = TextAnchor.MiddleCenter };
        big.normal.textColor = gameWon ? Color.yellow : Color.red;

        float cx = Screen.width * 0.5f;
        float cy = Screen.height * 0.5f;
        GUI.Label(new Rect(0, cy - 60, Screen.width, 80),
            gameWon ? "クリア！" : "ゲームオーバー", big);

        if (GUI.Button(new Rect(cx - 70, cy + 40, 140, 40), "もう一度"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
