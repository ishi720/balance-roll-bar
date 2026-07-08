using UnityEngine;

/// <summary>
/// ボールを追従してカメラを上方向にのみスクロールさせる。
/// </summary>
public class CameraController : MonoBehaviour
{
    /// <summary>追従対象のボール。</summary>
    [HideInInspector] public Transform ball;

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

    /// <summary>ボール追従位置へ上方向のみ滑らかにスクロールする。</summary>
    void LateUpdate()
    {
        if (ball == null) return;
        // カメラは上方向にのみスクロール（下には戻らない）
        float targetY = Mathf.Clamp(ball.position.y, transform.position.y, maxCamY);
        float y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
