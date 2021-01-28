using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("追蹤速度"), Range(0, 300)]
    public float speed = 10;

    /// <summary>
    /// 目標
    /// </summary>
    private Transform target;

    private void Awake()
    {
        target = GameObject.Find("玩家").transform;
    }

    // 延遲更新：在 Update 每一幀之後執行
    private void LateUpdate()
    {
        Track();
    }

    /// <summary>
    /// 追蹤
    /// </summary>
    private void Track()
    {
        Vector3 posTarget = target.position;    // 目標的座標
        Vector3 posCamera = transform.position; // 攝影機的座標

        // 攝影機的新座標 = 三維向量.往前移動(攝影機的座標，目標的座標，速度 * 1/60)
        posCamera = Vector3.MoveTowards(posCamera, posTarget, speed * Time.deltaTime);
        // 攝影機的座標 = 攝影機的新座標
        transform.position = posCamera;
    }
}
