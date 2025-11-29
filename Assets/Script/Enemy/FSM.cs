using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Unity.Mathematics;
using UnityEngine;



//状态枚举
public enum StateType
{
    Idle, Patrol, Chase, Attack, Hurt, Die,RangedAttack
}

public enum EnemyType
{
    Skeleton, Goblin, Skeleton2
}

//让编译器序列化这个类,作用是在监视面板看到并编辑参数
[Serializable]
//声明敌人参数
public class Parameter
{
    public float health = 3f;
    public float attack = 1f;
    public float moveSpeed = 0.5f;
    public float chaseSpeed = 1.5f;
    public float idleTime = 2f;

    //怪物种类
    public EnemyType enemyType = EnemyType.Skeleton;

    //巡逻范围
    public Vector3[] patrolPoints;
    //追击范围
    public Vector3[] chasePoints;

    // 巡逻点距离（左右3单位）
    public float patrolDistance = 1f;
    // 追击点距离（左右5单位）
    public float chaseDistance = 1.5f;

    //储存角色位置
    public Transform target;

    //用于检测攻击距离
    public LayerMask targetLayer;   
    public Transform attackPoint;   //圆心检测位置
    public float attackArea;        //圆的半径参数


    //用于模拟敌人受到攻击
    public bool getHit;
    //用于指示是否可以远程攻击
    public bool is_Ranged_Attack;
    //用于指示是否弹反了
    public bool is_Shield;



    //获取动画器组件
    public Animator animator;


    // 新增：炸弹核心参数（适配简化后的Boom脚本）
    [Header("哥布林炸弹参数")] // 分组显示，方便Inspector编辑
    public GameObject bombPrefab;        // 炸弹预制体（拖入你的炸弹Prefab）
    public Transform bombSpawnPoint;     // 炸弹生成点（哥布林手部空物体）
    public float bombMoveSpeed = 6f;     // 炸弹移动速度（默认6单位/秒）
    public int bombDamage = 1;          // 炸弹伤害（默认10点）
}


//有限状态机
public class FSM : MonoBehaviour
{
    public Parameter Parameter;

    private IState currentState;
    //字典映射
    private Dictionary<StateType, IState> states = new Dictionary<StateType, IState>();

    // Start is called before the first frame update
    void Start()
    {

        Vector3 enemySpawnPos = transform.position; // 记录敌人生成时的世界坐标
                                                    // 巡逻点：生成位置左右3单位（世界坐标固定）
        Parameter.patrolPoints = new Vector3[]
        {
        enemySpawnPos + Vector3.left * Parameter.patrolDistance,
        enemySpawnPos + Vector3.right * Parameter.patrolDistance
        };
        // 追击点：生成位置左右5单位（世界坐标固定）
        Parameter.chasePoints = new Vector3[]
        {
        enemySpawnPos + Vector3.left * Parameter.chaseDistance,
        enemySpawnPos + Vector3.right * Parameter.chaseDistance
        };


        //获取动画器组件
        Parameter.animator = GetComponent<Animator>();


        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.Hurt, new HurtState(this));
        states.Add(StateType.Die, new DieState(this)); 
        states.Add(StateType.RangedAttack, new RangedAttackState(this));


        //设置初始状态值
        TransitionState(StateType.Idle);

    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate();
        
        //测试受伤
        if (Input.GetKeyDown(KeyCode.Return))
        {
            GetHurt(1f);
        }
        
    }

    //切换状态
    public void TransitionState(StateType type)
    {
        if (currentState! != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter();
    }

    //朝向目标位置
    public void FlipTo(Vector3 target)
    {
        if(target != null)
        {
            if(transform.position.x > target.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if(transform.position.x < target.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }


    //受伤函数
    public void GetHurt(float Attack) //输入攻击力
    {
        if (Parameter.enemyType == EnemyType.Skeleton2)
        {
            float x = UnityEngine.Random.value;
            if(x <= 0.4)
            {
                Parameter.is_Shield = true;
                Parameter.getHit = true;
            }
            else
            {
                Parameter.getHit = true;
                Parameter.health -= Attack;
            }
        }
        else
        {
            Parameter.getHit = true;
            Parameter.health -= Attack;
        }
    }

    //检测攻击距离绘画图像
    private void OnDrawGizmos()
    {
        if (Parameter != null && Parameter.attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(Parameter.attackPoint.position, Parameter.attackArea);
        }


        // 绘制巡逻点（蓝色实心球 + 连线）
        if (Parameter != null && Parameter.patrolPoints != null && Parameter.patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            foreach (var point in Parameter.patrolPoints)
            {
                Gizmos.DrawSphere(point, 0.05f); // 巡逻点大小
            }
        }

        // 绘制追击点（红色实心球）
        if (Parameter != null && Parameter.chasePoints != null && Parameter.chasePoints.Length > 0)
        {
            Gizmos.color = Color.red;
            foreach (var point in Parameter.chasePoints)
            {
                Gizmos.DrawSphere(point, 0.05f); // 追击点
            }
        }
    }

}
