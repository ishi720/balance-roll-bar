using UnityEngine;

/// <summary>
/// ボールを追従してカメラをスクロール方向にのみスクロールさせる（逆方向には戻らない）。
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>追従対象のボール。</summary>
    [HideInInspector] public Transform ball;

    /// <summary>0より大きい場合、ボール追従ではなくこの速度で自動的にスクロールし続ける。</summary>
    [HideInInspector] public float forcedScrollSpeed;

    /// <summary>スクロール方向。</summary>
    [HideInInspector] public ScrollDirection direction = ScrollDirection.Up;

    private Camera cam;
    private float maxCamY;
    private float minCamY;

    /// <summary>スクロール範囲を設定する。</summary>
    /// <param name="worldMinY">ワールドの下限Y座標。</param>
    /// <param name="worldMaxY">ワールドの上限Y座標。</param>
    public void Initialize(float worldMinY, float worldMaxY)
    {
        cam = GetComponent<Camera>();
        maxCamY = worldMaxY - cam.orthographicSize;
        minCamY = worldMinY + cam.orthographicSize;
    }

    /// <summary>スクロール方向にのみ滑らかにスクロールする。強制スクロール指定時はボール位置を無視して一定速度で進む。</summary>
    void LateUpdate()
    {
        bool isDown = direction == ScrollDirection.Down;
        float y;
        if (forcedScrollSpeed > 0f)
        {
            float step = forcedScrollSpeed * Time.deltaTime;
            y = isDown
                ? Mathf.Max(transform.position.y - step, minCamY)
                : Mathf.Min(transform.position.y + step, maxCamY);
        }
        else
        {
            if (ball == null) return;
            // カメラはスクロール方向にのみ進む（逆方向には戻らない）
            float targetY = isDown
                ? Mathf.Clamp(ball.position.y, minCamY, transform.position.y)
                : Mathf.Clamp(ball.position.y, transform.position.y, maxCamY);
            y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f);
        }
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
