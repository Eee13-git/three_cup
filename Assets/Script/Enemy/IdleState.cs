using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
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

        //如果受伤
        if(parameter.getHit == true)
        {
            manager.TransitionState(StateType.Hurt);
        }


        //如果看到玩家
        if (parameter.target != null &&
            parameter.target.position.x >= parameter.chasePoints[0].x &&
            parameter.target.position.x <= parameter.chasePoints[1].x)
        {
            manager.TransitionState(StateType.Chase);
        }
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

    //存储 Animator（动画控制器）中当前状态的关键信息
    private AnimatorStateInfo info;
    //构造函数
    public AttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        float x = Random.value;
        if (x >= 0.5)
            parameter.animator.Play("Attack_1");
        else
            parameter.animator.Play("Attack_2");
    }

    public void OnUpdate()
    {
        //如果受伤
        if (parameter.getHit == true)
        {
            manager.TransitionState(StateType.Hurt);
        }

        info = parameter.animator.GetCurrentAnimatorStateInfo(0);

        if(info.normalizedTime >= 0.95f)
        {
            manager.TransitionState(StateType.Chase);
        }
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
        //Debug.Log("Walk");
    }

    public void OnUpdate()
    {
        //如果受伤
        if (parameter.getHit == true)
        {
            manager.TransitionState(StateType.Hurt);
        }

        //如果看到玩家
        if (parameter.target != null &&
        parameter.target.position.x >= parameter.chasePoints[0].x &&
        parameter.target.position.x <= parameter.chasePoints[1].x)
        {
            manager.TransitionState(StateType.Chase);
        }

        //始终朝向巡逻点
        manager.FlipTo(parameter.patrolPoints[patrolPosition]);
        //从现位置到巡逻点位置，以一定速度移动的函数
        manager.transform.position = Vector2.MoveTowards(manager.transform.position,
            parameter.patrolPoints[patrolPosition], parameter.moveSpeed * Time.deltaTime);
        //接近巡逻点时切换状态
        if(Vector2.Distance(manager.transform.position, parameter.patrolPoints[patrolPosition]) < 0.2f)
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
        parameter.animator.Play("Walk");
    }

    public void OnUpdate()
    {
        //如果受伤
        if (parameter.getHit == true)
        {
            manager.TransitionState(StateType.Hurt);
        }

        if (parameter.target == null ||
            parameter.target.position.x < parameter.chasePoints[0].x ||
            parameter.target.transform.position.x > parameter.chasePoints[1].x)
        {
            parameter.target = null;
            manager.TransitionState(StateType.Idle);
        }
        else
        {

            manager.FlipTo(parameter.target.position);

            if (parameter.target)
                manager.transform.position = Vector2.MoveTowards(manager.transform.position,
                parameter.target.position, parameter.chaseSpeed * Time.deltaTime);

            if (Physics2D.OverlapCircle(parameter.attackPoint.position, parameter.attackArea, parameter.targetLayer))
            {
                manager.TransitionState(StateType.Attack);
            }
        }
    }
    public void OnExit()
    {

    }
}


public class HurtState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //存储 Animator（动画控制器）中当前状态的关键信息
    private AnimatorStateInfo info;

    //构造函数
    public HurtState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Hurt");
        Debug.Log("扣血");
    }

    public void OnUpdate()
    {
        info = parameter.animator.GetCurrentAnimatorStateInfo(0);
        if (info.normalizedTime >= 0.95f)
        {
            if (parameter.health <= 0)
            {
                manager.TransitionState(StateType.Die);
            }
            else
            {
                parameter.target = GameObject.FindWithTag("Player").transform;

                manager.TransitionState(StateType.Chase);
            }
        }
    }
    public void OnExit()
    {
        parameter.getHit = false;
    }
}



public class DieState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //构造函数
    public DieState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        parameter.animator.Play("Die");
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}
