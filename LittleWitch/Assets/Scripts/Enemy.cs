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

        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position + attackoffset, attackRadius);
    }

    private void Update()
    {
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
        }
        nma.isStopped = true;
        ani.SetBool("走路開關", false);

    }

    /// <summary>
    /// 延遲攻擊
    /// </summary>
    private IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(attackDelay);

        // 碰觸陣列 = 物理.球體(座標+判定，半徑)
        Collider[] hits = Physics.OverlapSphere(transform.position + attackoffset, attackRadius, 1 << 9);
        print(hits[0].name);

        if (hits.Length > 0)
        {
            hits[0].GetComponent<Player>().Damage(attack);
        }

    }
}
