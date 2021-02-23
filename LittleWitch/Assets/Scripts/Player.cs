using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Player : MonoBehaviour
{
    #region 基本
    [Header("移動速度"), Range(0, 1000)]
    public float speed = 10;
    [Header("走路移動速度"), Range(0, 1000)]
    public float speedwalk = 10;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump = 10;
    [Header("攝影機旋轉速度"), Range(0, 1000)]
    public float turn = 10;
    [Header("攝影機角度限制")]
    public Vector2 camLimit = new Vector2(-30, 0);
    [Header("角色旋轉速度"), Range(0, 1000)]
    public float turnSpeed = 30;
    [Header("檢查地板球體半徑")]
    public float radius = 1f;
    [Header("檢查地板球體位移")]
    public Vector3 offset;

    [Header("跳躍次數限制")]
    public int jumpCountLimit = 2;

    [Header("血量"), Range(0, 5000)]
    public float hp = 100;
    private float hpmax;
    [Header("魔力"), Range(0, 5000)]
    public float mp = 500;
    private float mpmax;
    [Header("體力"), Range(0, 5000)]
    public float ps = 200;
    private float psmax;

    [Header("吧條")]
    public Image barHp;
    public Image barMp;
    public Image barPS;

    [Header("扣除體力"), Range(0, 5000)]
    public float psMove = 1;
    [Header("jump扣除體力"), Range(0, 5000)]
    public float psjump = 15;
    [Header("停止體力"), Range(0, 5000)]
    public float psR = 1;

    private int jumpCount;
    /// <summary>
    /// 是否在地面上
    /// </summary>
    private bool isGround = true;
    private Animator ani;
    private Rigidbody rig;
    private Transform cam;
    private float x;
    private float y;
    #endregion

    #region 攻擊
    [Header("生成位置")]
    public Transform attackPoint;
    [Header("攻擊特效")]
    public GameObject attackPS;
    [Header("攻擊特效速度"), Range(0, 2000)]
    public float attackSpeed = 1000;
    [Header("攻擊力"), Range(0, 500)]
    public float attack = 50;
    [Header("攻擊消耗"), Range(0, 500)]
    public float attackCOST = 10;
    [Header("攻擊延遲"), Range(0f, 1f)]
    public float attackPSDELAY = 0.3f;
    [Header("下次攻擊"), Range(0f, 5f)]
    public float attackDELAY= 1f;
    /// <summary>
    /// 是否攻擊中
    /// </summary>
    private bool attacking;
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position + offset, radius);
    }

    //喚醒事件：在 Start 前執行一次
    private void Awake()
    {
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        cam = GameObject.Find("攝影機根物件").transform;

        hpmax = hp;
        mpmax = mp;
        psmax = ps;
    }

    private void Update()
    {
        if (attacking) return;

        Move();
        TurnCamera();
        Jump();
        PSSystem();
        Attack();
    }

    /// <summary>
    /// 固定更新事件：50FPS
    /// </summary>
    private void FixedUpdate()
    {
        
    }
    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && mp >= attackCOST && !attacking)                                   //按左鍵
        {
            StartCoroutine(AttackTimeControl());
        }
    }

    private IEnumerator AttackTimeControl()
    {
        rig.velocity = Vector3.zero;
        attacking = true;                                                                       //攻擊中
        mp -= attackCOST;                                                                       //扣除MP
        barMp.fillAmount = mp / mpmax;                                                          //更新介面
        ani.SetTrigger("攻擊觸發");

        yield return new WaitForSeconds(attackPSDELAY);                                         //延遲生成

        GameObject temp = Instantiate(attackPS, attackPoint.position, attackPoint.rotation);    //生成攻擊位置
        temp.GetComponent<Rigidbody>().AddForce(transform.forward * attackSpeed);               //取得攻擊加推力

        yield return new WaitForSeconds(attackDELAY);                                           //延遲攻擊
        attacking = false;                                                                      //不是攻擊
    }

    /// <summary>
    /// 移動方法
    /// </summary>
    private void Move()
    {
        float v = Input.GetAxis("Vertical");                                // 取得 前後軸 值 W S 上 下
        float h = Input.GetAxis("Horizontal");                              // 取得 前後軸 值 A D 左 右

        Transform camNew = cam;                                             // 新攝影機座標資訊
        camNew.eulerAngles = new Vector3(0, cam.eulerAngles.y, 0);          // 去掉 X 與 Z 角度

        transform.rotation = Quaternion.Lerp(transform.rotation, camNew.rotation, 0.5f * turnSpeed * Time.deltaTime);               // 角色的角度 = 角色，攝影機 角度的插值

        if (ps > 0)
        {
            rig.velocity = ((camNew.forward * v + camNew.right * h) * speed * Time.deltaTime) + transform.up * rig.velocity.y;          // 加速度 = 前方 * 前後值 + 右方 * 左右值 * 速度 * 1/60 + 上方 * 加速度上下值
            ani.SetBool("跑步開關", rig.velocity.magnitude > 0.1f);             // 動畫.設定布林值("參數名稱"，剛體.加速度.值 > 0)
        }
        else
        {
            rig.velocity = ((camNew.forward * v + camNew.right * h) * speedwalk * Time.deltaTime) + transform.up * rig.velocity.y;          // 加速度 = 前方 * 前後值 + 右方 * 左右值 * 速度 * 1/60 + 上方 * 加速度上下值
            ani.SetBool("走路開關", rig.velocity.magnitude > 0.1f);
            ani.SetBool("跑步開關", false);
        }

    }

    /// <summary>
    /// 旋轉攝影機
    /// </summary>
    private void TurnCamera()
    {
        x += Input.GetAxis("Mouse X") * turn * Time.deltaTime;  // 取得滑鼠 X 值
        y -= Input.GetAxis("Mouse Y") * turn * Time.deltaTime;  // 取得滑鼠 Y 值
        y = Mathf.Clamp(y, camLimit.x, camLimit.y);             // 限制 Y
        cam.localEulerAngles = new Vector3(y , x, 0);           // 攝影機.角度 = (Y 值，X 值，0) * 旋轉速度 * 1/60
    }

    /// <summary>
    /// 跳躍
    /// </summary>
    private void Jump()
    {
        // 碰撞陣列 = 物理碰撞範圍(中心點，半點，圖層)
        Collider[] hit = Physics.OverlapSphere(transform.position + offset, radius, 1 << 8);

        if (hit.Length > 0 && hit[0])
        {
            isGround = true;
            jumpCount = 0;
        }
        else isGround = false;

        ani.SetBool("是否在地面上", isGround);

        if (ps < psjump) return;

        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < jumpCountLimit-1)    // 如果 按下 空白建 並且 在地面上
        {
            jumpCount++;
            rig.Sleep();
            rig.WakeUp();
            rig.AddForce(Vector3.up * jump);                // 推力
            ani.SetTrigger("跳躍觸發");

            //扣除體力
            ps -= psjump;
            barPS.fillAmount = ps / psmax;
        }

        
    }

    /// <summary>
    /// 體力
    /// </summary>
    private void PSSystem()
    {
        if (ani.GetBool("跑步開關"))
        {
            ps -= psMove * Time.deltaTime;
            barPS.fillAmount = ps / psmax;
        }
        else if(!ani.GetBool("走路開關"))
        {
            ps += psR * Time.deltaTime;
            barPS.fillAmount = ps / psmax;
        }

        ps = Mathf.Clamp(ps, 0, psmax);
        
    }
}
