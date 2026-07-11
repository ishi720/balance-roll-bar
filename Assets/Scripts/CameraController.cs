using UnityEngine;

/// <summary>
/// ボールを追従してカメラを上方向にのみスクロールさせる。
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>追従対象のボール。</summary>
    [HideInInspector] public Transform ball;

    /// <summary>0より大きい場合、ボール追従ではなくこの速度で自動的に上へスクロールし続ける。</summary>
    [HideInInspector] public float forcedScrollSpeed;

    private Camera cam;
    private float maxCamY;

    /// <summary>スクロール範囲を設定する。</summary>
    /// <param name="worldMinY">ワールドの下限Y座標。</param>
    /// <param name="worldMaxY">ワールドの上限Y座標。</param>
    public void Initialize(float worldMinY, float worldMaxY)
    {
        cam = GetComponent<Camera>();
        maxCamY = worldMaxY - cam.orthographicSize;
    }

    /// <summary>上方向のみ滑らかにスクロールする。強制スクロール指定時はボール位置を無視して一定速度で上昇する。</summary>
    void LateUpdate()
    {
        float y;
        if (forcedScrollSpeed > 0f)
        {
            y = Mathf.Min(transform.position.y + forcedScrollSpeed * Time.deltaTime, maxCamY);
        }
        else
        {
            if (ball == null) return;
            // カメラは上方向にのみスクロール（下には戻らない）
            float targetY = Mathf.Clamp(ball.position.y, transform.position.y, maxCamY);
            y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f);
        }
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
