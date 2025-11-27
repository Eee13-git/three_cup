using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;



//状态枚举
public enum StateType
{
    Idle, Patrol, Chase, Attack, React
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
        states.Add(StateType.Idle, new IdleState(this));
        states.Add(StateType.Attack, new AttackState(this));
        states.Add(StateType.Patrol, new PatrolState(this));
        states.Add(StateType.Chase, new ChaseState(this));
        states.Add(StateType.React, new ReactState(this));

        //设置初始状态值
        TransitionState(StateType.Idle);

        //获取动画器组件
        Parameter.animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        currentState.OnUpdate();
    }

    //切换状态
    public void TransitionState(StateType type)
    {
        if (currentState! != null)
            currentState.OnExit();
        currentState = states[type];
        currentState.OnEnter();
        Debug.Log("进入Walk");
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
}
