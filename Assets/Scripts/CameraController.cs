using UnityEngine;

public class CameraController : MonoBehaviour
{
    [HideInInspector] public Transform ball;

    private Camera cam;
    private float maxCamY;

    public void Initialize(float worldMinY, float worldMaxY)
    {
        cam = GetComponent<Camera>();
        maxCamY = worldMaxY - cam.orthographicSize;
    }

    void LateUpdate()
    {
        if (ball == null) return;
        // カメラは上方向にのみスクロール（下には戻らない）
        float targetY = Mathf.Clamp(ball.position.y, transform.position.y, maxCamY);
        float y = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * 3f);
        transform.position = new Vector3(transform.position.x, y, transform.position.z);
    }
}
