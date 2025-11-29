using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;



//状态枚举
public enum StateType
{
    Idle, Patrol, Chase, Attack, Hurt, Die
}

//让编译器序列化这个类,作用是在监视面板看到并编辑参数
[Serializable]
//声明敌人参数
public class Parameter
{
    public int health;
    public float moveSpeed;
    public float chaseSpeed;
    public float idleTime;

    //巡逻范围
    public Transform[] patrolPoints;
    //追击范围
    public Transform[] chasePoints;
    //储存角色位置
    public Transform target;

    //用于检测攻击距离
    public LayerMask targetLayer;   
    public Transform attackPoint;   //圆心检测位置
    public float attackArea;        //圆的半径参数
    //用于模拟敌人受到攻击
    public bool getHit;
    //


    //获取动画器组件
    public Animator animator;
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
        //获取动画器组件
        Parameter.animator = GetComponent<Animator>();


        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.Hurt, new HurtState(this));
        states.Add(StateType.Die, new DieState(this));

        //设置初始状态值
        TransitionState(StateType.Idle);

    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate();

        if (Input.GetKeyDown(KeyCode.Return))
        {
            Parameter.getHit = true;
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
    public void FlipTo(Transform target)
    {
        if(target != null)
        {
            if(transform.position.x > target.position.x)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if(transform.position.x < target.position.x)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }


    //受伤函数
    public void GetHurt(int Attack) //输入攻击力
    {
        TransitionState(StateType.Hurt);
        Parameter.health -= Attack;
    }


    //检测攻击距离绘画图像
    private void OnDrawGizmos()
    {
        //在攻击位置画圆
        Gizmos.DrawWireSphere(Parameter.attackPoint.position, Parameter.attackArea);
    }
}
