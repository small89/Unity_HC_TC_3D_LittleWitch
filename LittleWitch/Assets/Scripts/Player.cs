using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump = 10;
    [Header("旋轉速度"), Range(0, 1000)]
    public float turn = 10;

    /// <summary>
    /// 是否在地面上
    /// </summary>
    private bool isGround;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;

    //喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        cam = GameObject.Find("攝影機根物件").transform;
    }

    private void Update()
    {
        Move();
        TurnCamera();
    }

    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");            // 取得 前後軸 值 W S 上 下
        float h = Input.GetAxis("Horizontal");          // 取得 前後軸 值 A D 左 右

        // 加速度 = 前方 * 前後值 + 右方 * 左右值 * 速度 * 1/60 + 上方 * 加速度上下值
        rig.velocity = ((transform.forward * v + transform.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;
        // 動畫.設定布林值("參數名稱"，剛體.加速度.值 > 0)
        ani.SetBool("跑步開關", rig.velocity.magnitude > 0);
    }

    /// <summary>
    /// 旋轉攝影機
    /// </summary>
    private void TurnCamera()
    {

    }
}
