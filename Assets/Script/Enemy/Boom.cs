using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boom : MonoBehaviour
{

    private float moveSpeed;       // 移动速度
    private int damage;            // 伤害值
    private Vector3 moveDirection; // 移动方向

    private int targetGridCount = 2; // 移动3格后消失
    private float movedDistance;   // 已移动距离
    private bool isMoving = false; // 是否开始移动

    private Animator anim; 


    public void Init(Vector3 direction, float speed, int dmg)
    {
        // 异常值保底（避免传0/负数导致逻辑出错）
        moveSpeed = speed <= 0 ? 1.5f : speed;
        damage = dmg <= 0 ? 1 : dmg;

        // 归一化方向（避免斜向移动速度异常），忽略Y轴（2D横向游戏）
        moveDirection = new Vector3(direction.x, 0, direction.z).normalized;

        movedDistance = 0;
        isMoving = true; // 启动移动逻辑
    }


    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        anim.Play("Boom_1");
    }

    // Update is called once per frame
    void Update()
    {
        if (!isMoving) return; // 未初始化则不执行

        // 1. 计算帧移动距离（帧独立，避免帧率影响速度）
        float step = moveSpeed * Time.deltaTime;

        // 2. 限制单次移动距离，避免超过3格
        float remainingDistance = targetGridCount - movedDistance;
        step = Mathf.Min(step, remainingDistance);

        // 3. 移动炸弹
        transform.Translate(moveDirection * step);
        movedDistance += step;

        // 4. 移动满3格后销毁
        if (movedDistance >= targetGridCount)
        {
            DestroySelf();
        }
    }


    // 碰撞玩家触发伤害（需炸弹碰撞体勾选Is Trigger）
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            
            // 调用玩家受伤逻辑（需PlayerController有PlayerHurt方法）
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.PlayerHurt(damage);
            }

            

            Debug.Log("炸弹碰到了玩家");
            DestroySelf(); // 碰到玩家立即销毁
        }
    }

    // 销毁炸弹（统一逻辑，方便扩展特效/音效）
    private void DestroyBoom()
    {
        isMoving = false; // 停止移动
        Destroy(gameObject, 0.5f); // 延迟销毁（适配特效播放）
    }

    // 调试用：绘制炸弹移动路径（场景视图可见）
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + moveDirection * targetGridCount);
    }


    public void DestroySelf()
    {
        isMoving = false; // 停止移动逻辑
        if (gameObject != null && gameObject.activeInHierarchy)
        {
            anim.Play("Boom_2");

            Destroy(gameObject, 1.5f); // 由MonoBehaviour脚本执行Destroy
        }
    }
    
}
