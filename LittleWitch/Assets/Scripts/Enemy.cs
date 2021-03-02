using UnityEngine;
using UnityEngine.AI;       //引用API
using System.Collections;

public class Enemy : MonoBehaviour
{
    #region 基本
    [Header("追蹤範圍"), Range(0,100)]
    public float rangeTrack = 10;
    [Header("移動速度"), Range(0, 100)]
    public float speed = 5;
    [Header("攻擊範圍"), Range(0, 100)]
    public float rangeAttack = 4;
    [Header("攻擊冷卻"), Range(0, 10)]
    public float attackCD = 2.5f;
    [Header("攻擊判定球"), Range(0, 10)]
    public float attackRadius = 1.2f;
    [Header("攻擊球體")]
    public Vector3 attackoffset;
    [Header("攻擊延遲"), Range(0, 2)]
    public float attackDelay = 1f;
    [Header("攻擊力"), Range(0, 2000)]
    public float attack = 10;
    [Header("血量"), Range(0, 10000)]
    public float hp = 500;


    private Animator ani;
    private Transform player;
    private NavMeshAgent nma;
    private float timer;
    #endregion

    private void Awake()
    {
        ani = GetComponent<Animator>();
        nma = GetComponent<NavMeshAgent>();

        nma.stoppingDistance = rangeAttack;
        player = GameObject.Find("玩家").transform;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeTrack);

        Gizmos.color = new Color(0, 0, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, rangeAttack);

        //攻擊判定
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position + attackoffset.z * transform.forward +transform.right * attackoffset.x + transform.up * attackoffset.y, attackRadius);

    }

    private void Update()
    {
        //如果正在攻擊 跳出
        if (ani.GetCurrentAnimatorStateInfo(0).IsName("Attack02")) return;

       //if (ani.GetCurrentAnimatorStateInfo(0).IsName("GetHit")) return;

        Track();
    }

    /// <summary>
    /// 追蹤
    /// </summary> 
    private void Track()
    {
        //距離 = 三維向量(A點，B點)
        float dis = Vector3.Distance(player.position, transform.position);

        if (dis <= rangeAttack)
        {
            Attack();
        }
        else if(dis <= rangeTrack)
        {
            //代理氣 追蹤玩家
            nma.isStopped = false;
            nma.SetDestination(player.position);
            ani.SetBool("走路開關", true);

        }
        else
        {
            nma.isStopped = true;
            ani.SetBool("走路開關", false);
        }
    }

    /// <summary>
    /// 攻擊
    /// </summary>
    private void Attack()
    {
        if (timer >= attackCD)              //計時器>=冷卻
        {
            timer = 0;
            ani.SetTrigger("攻擊觸發");
            StartCoroutine(DelayAttack());   //累加時間
        }
        else
        {
            timer += Time.deltaTime;        //累加時間

            Vector3 pos = player.position;  //取得PLAYER座標
            pos.y = transform.position.y;   //將Y軸改怪物Y軸
            transform.LookAt(pos);          //面向(修改
        }
        nma.isStopped = true;
        ani.SetBool("走路開關", false);

    }/// <summary>

     /// 受傷
     /// </summary>
     /// <param name="getDAMAGE">接受到受傷直</param>
    public void Damage(float getDAMAGE)
    {
        ani.SetTrigger("受傷觸發");
        hp -= getDAMAGE;

        if (hp <= 0) Dead();
    }


    /// <summary>
    /// 死亡
    /// </summary>
    private void Dead()
    {
        hp = 0;
        ani.SetBool("死亡開關", true);
        enabled = false;
    }

    /// <summary>
    /// 延遲攻擊
    /// </summary>
    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        // 碰觸陣列 = 物理.球體(座標+判定，半徑)
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * attackoffset.z + transform.right * attackoffset.x + transform.up * attackoffset.y, attackRadius, 1 << 9);
        //print(hits[0].name);

        if (hits.Length > 0)
        {
            hits[0].GetComponent<Player>().Damage(attack);
        }

    }
}
