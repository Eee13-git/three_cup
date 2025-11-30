using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
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
        parameter.idleTime = 4 * Random.value;
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
        if(Mathf.Abs(manager.transform.position.x - parameter.patrolPoints[patrolPosition].x) < 0.2f)
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

    //炸弹释放间隔计时器
    private float timer = 0f;

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
        timer += Time.deltaTime;

        //如果受伤
        if (parameter.getHit == true)
        {
            manager.TransitionState(StateType.Hurt);
        }

        if (parameter.target == null ||
            parameter.target.transform.position.x < parameter.chasePoints[0].x ||
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

            //远程攻击
            if (timer >= 3)
            {
                timer = 0;
                if (parameter.is_Ranged_Attack && parameter.enemyType == EnemyType.Goblin)
                {
                    manager.TransitionState(StateType.RangedAttack);
                    //Debug.Log("进入远程攻击");
                    //调用扔炸弹

                    return;
                }
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
        if (parameter.is_Shield == true && parameter.enemyType == EnemyType.Skeleton2) 
        {
            parameter.animator.Play("Shield");
            Debug.Log("弹反");
        }
        else
        {
            parameter.animator.Play("Hurt");
            Debug.Log("扣血");
        }
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
        parameter.is_Shield = false;
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
        Object.Destroy(manager.gameObject, 5f);
    }

    public void OnUpdate()
    {

    }

    public void OnExit()
    {

    }
}




public class RangedAttackState : IState
{
    //添加状态机的引用
    private FSM manager;
    //获取设置的属性
    private Parameter parameter;

    //存储 Animator（动画控制器）中当前状态的关键信息
    private AnimatorStateInfo info;

    private bool hasSpawnedBomb = false; // 避免重复生成炸弹


    //构造函数
    public RangedAttackState(FSM manager)
    {
        this.manager = manager;
        this.parameter = manager.Parameter;
    }

    public void OnEnter()
    {
        hasSpawnedBomb = false;
        parameter.animator.Play("Attack_Boom");
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
            if (!hasSpawnedBomb)
            {
                //调用生成炸弹逻辑
                SpawnBomb();
                hasSpawnedBomb = true;
            }

            manager.TransitionState(StateType.Chase);
        }
    }

    public void OnExit()
    {

    }

    private void SpawnBomb()
    {
        // 空引用校验（避免报错）
        if (parameter.bombPrefab == null)
        {
            Debug.LogError("哥布林未配置炸弹预制体！", manager.gameObject);
            return;
        }
        if (parameter.bombSpawnPoint == null)
        {
            Debug.LogError("哥布林未配置炸弹生成点！", manager.gameObject);
            return;
        }
        if (parameter.target == null)
        {
            Debug.LogError("哥布林未检测到玩家！", manager.gameObject);
            return;
        }

        // 1. 生成炸弹预制体
        GameObject boomObj = Object.Instantiate(
            parameter.bombPrefab,                // 炸弹预制体
            parameter.bombSpawnPoint.position,   // 生成位置（哥布林手部）
            Quaternion.identity                  // 无旋转
        );

        // 2. 获取Boom组件并调用Init初始化
        Boom boom = boomObj.GetComponent<Boom>();
        if (boom != null)
        {
            // 计算炸弹朝向：从生成点指向玩家
            Vector3 directionToPlayer = parameter.target.position - parameter.bombSpawnPoint.position;
            // 调用Init：传入方向、速度、伤害（适配简化后的Boom脚本）
            boom.Init(
                directionToPlayer,
                parameter.bombMoveSpeed,
                parameter.bombDamage
            );
        }
        else
        {
            Debug.LogError("炸弹预制体未挂载Boom脚本！", boomObj);
            boomObj.GetComponent<Boom>().DestroySelf();
        }
    }

}
