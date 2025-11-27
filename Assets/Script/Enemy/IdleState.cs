using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class IdleState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;


    //两个巡逻点，到地方等一段时间
    private float timer;
    //构造函数
    public IdleState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Idle");
    }

    public void OnUpdate()
    {
        timer += Time.deltaTime;

        if (timer > parameter.idleTime)
        {
            manager.TransitionState(StateType.Patrol);
        }
    }

    public void OnExit()
    {
        timer = 0;
    }
}


public class AttackState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //构造函数
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {
    }
}


public class PatrolState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;


    //下标表示巡逻点
    private int patrolPosition;
    //构造函数
    public PatrolState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Walk");
        Debug.Log("Walk");
    }

    public void OnUpdate()
    {
        //始终朝向巡逻点
        manager.FlipTo(parameter.patrolPoints[patrolPosition]);
        //从现位置到巡逻点位置，以一定速度移动的函数
        manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            parameter.patrolPoints[patrolPosition].position, parameter.moveSpeed * Time.deltaTime);
        //接近巡逻点时切换状态
        if(Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition].position) < 0.2f)
        {
            manager.TransitionState(StateType.Idle);
        }
    }

    public void OnExit()
    {
        //改变巡逻点
        patrolPosition++;

        if (patrolPosition >= parameter.patrolPoints.Length)
        {
            patrolPosition = 0;
        }
    }
}


public class ChaseState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //构造函数
    public ChaseState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}


public class ReactState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //构造函数
    public ReactState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {

    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}


