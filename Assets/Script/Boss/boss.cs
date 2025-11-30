using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boss : MonoBehaviour
{
    [Header("缩放")]
    [Tooltip("等比放大系数（1 = 原始大小）")]
    public float sizeMultiplier = 1f;
    [Tooltip("是否同时按比例放大感知/判定距离（巡逻、追击、攻击判定等）")]
    public bool scaleRanges = true;
    [Tooltip("是否同时按比例放大碰撞器（Box/Circle/Capsule2D）")]
    public bool scaleColliders = true;

    [Header("属性")]
    [Tooltip("若为 true，则在索敌到玩家时将 Boss 的生命/攻击设置为玩家属性的倍数")]
    public bool syncStatsWithPlayer = true;
    [Tooltip("基于玩家生命的倍数（索敌时应用）")]
    public float playerHealthMultiplier = 2f;
    [Tooltip("基于玩家攻击力的倍数（索敌时应用）")]
    public float playerAttackMultiplier = 1.5f;

    public float MaxHealth = 200f;
    public float Health = 200f;
    public float AttackStrength = 20f;
    public float CriticalRate = 0.1f;

    [Header("移动")]
    public float runSpeed = 1.5f;
    public float patrolDistance = 3f;        // 巡逻半宽（相对于初始位置）
    public float chaseRange = 8f;            // 追击检测范围（用于索敌）
    public float attackRange = 0.5f;         // 旧的总体距离阈值（保留用于兼容）

    [Header("攻击 - 触发判定（更精确）")]
    [Tooltip("水平方向上允许触发攻击的最大距离（比 attackRange 更严格）")]
    public float attackHorizontalRange = 0.5f;
    [Tooltip("垂直容差（Y 差值）")]
    public float attackVerticalTolerance = 0.6f;
    [Tooltip("是否要求面向玩家才能触发攻击")]
    public bool requireFacingToAttack = true;
    [Tooltip("检测视线阻挡的 Layer（为空则不检测）")]
    public LayerMask obstacleLayer;

    [Header("攻击")]
    public float attackInterval = 1.8f;      // 攻击间隔（秒）
    private float attackTimer = 0f;
    private int comboStep = 0;
    public float attackMoveSpeed = 0.5f;     // 攻击时的少量位移补偿

    [Header("攻击节制")]
    [Tooltip("连续进入攻击状态的持续时长（秒），在该时段内 Boss 可正常发动攻击）")]
    public float attackBurstDuration = 6f;
    [Tooltip("完成一次攻击时段后，Boss 仅追击不再攻击的时长（秒）")]
    public float attackCooldownDuration = 4f;

    // 运行时控制
    private float burstTimer = 0f;
    private float cooldownTimer = 0f;
    private bool inAttackBurst = false;      // 正在处于可攻击的连续时间段
    private bool inAttackCooldown = false;   // 在冷却期，仅追击不攻击

    [Header("命中检测")]
    [Tooltip("攻击判定延迟（秒），可通过动画事件改为更精确的触发）")]
    public float attackHitDelay = 0.15f;
    [Tooltip("用于 Overlap 的 Layer，优先使用以减少误伤；若为 0 则检测所有层并通过 Tag/组件筛选")]
    public LayerMask attackHitLayerMask;

    [Header("检测")]
    public string playerTag = "Player";
    [Tooltip("指定玩家所在 Layer（优先使用 Layer 进行物理查询），若未设置可开启 Tag 回退")]
    public LayerMask playerLayer;
    [Tooltip("当使用 Tag 回退时，索敌的最小间隔（秒），避免每帧 Find")]
    public float seekInterval = 0.25f;
    public bool enableTagFallback = true;

    // 状态
    private bool isAttack = false;
    private bool isDead = false;
    private bool isFacingRight = true;

    private Transform playerTrans;
    private Rigidbody2D rb;
    private Animator anim;

    private Vector2 startPosition;
    private float patrolTargetX;
    private float patrolDir = 1f;
    private float patrolSpeed;

    // 索敌相关（优化：非每帧查找，使用 Physics2D.OverlapCircleNonAlloc）
    private float seekTimer = 0f;
    private static readonly Collider2D[] seekResults = new Collider2D[4]; // 预分配，避免 GC

    // 命中检测缓冲（预分配）
    private static readonly Collider2D[] attackHitResults = new Collider2D[6];

    // 只在一次攻击周期内命中一次标志（避免重复伤害）
    private bool hasAppliedHitThisAttack = false;

    // 保存原始值以便等比放大
    private Vector3 originalLocalScale;
    private float originalPatrolDistance;
    private float originalChaseRange;
    private float originalAttackRange;
    private float originalAttackHorizontalRange;
    private float originalAttackVerticalTolerance;
    private float originalAttackMoveSpeed;

    // 记录已为哪个玩家应用过 stats，同一玩家只应用一次
    private Transform statsAppliedForPlayer = null;

    void Awake()
    {
        // 缓存原始数值（在任何缩放动作前）
        originalLocalScale = transform.localScale;
        originalPatrolDistance = patrolDistance;
        originalChaseRange = chaseRange;
        originalAttackRange = attackRange;
        originalAttackHorizontalRange = attackHorizontalRange;
        originalAttackVerticalTolerance = attackVerticalTolerance;
        originalAttackMoveSpeed = attackMoveSpeed;
    }

    void Start()
    {
        // 初始化组件
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        startPosition = transform.position;
        patrolTargetX = startPosition.x + patrolDistance;
        patrolSpeed = runSpeed;

        // 应用等比缩放（若 sizeMultiplier != 1）
        if (sizeMultiplier != 1f)
        {
            ApplyScaling(sizeMultiplier);
        }

        // 尝试通过 Tag 立即缓存（一次性）
        if (enableTagFallback)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
                playerTrans = p.transform;
        }

        Health = MaxHealth;
    }

    // 根据 sizeMultiplier 按比例调整：transform、判定范围、碰撞器等
    void ApplyScaling(float multiplier)
    {
        // 1. 缩放 transform（保持原始符号）
        transform.localScale = originalLocalScale * multiplier;

        // 2. 缩放范围/判定（可选择）
        if (scaleRanges)
        {
            patrolDistance = originalPatrolDistance * multiplier;
            chaseRange = originalChaseRange * multiplier;
            attackRange = originalAttackRange * multiplier;
            attackHorizontalRange = originalAttackHorizontalRange * multiplier;
            attackVerticalTolerance = originalAttackVerticalTolerance * multiplier;
            attackMoveSpeed = originalAttackMoveSpeed * multiplier;
        }

        // 3. 缩放碰撞器（常见2D碰撞器）
        if (scaleColliders)
        {
            // BoxCollider2D
            var box = GetComponent<BoxCollider2D>();
            if (box != null)
            {
                box.size = box.size * multiplier;
                box.offset = box.offset * multiplier;
            }

            // CircleCollider2D
            var circle = GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                circle.radius = circle.radius * multiplier;
                circle.offset = circle.offset * multiplier;
            }

            // CapsuleCollider2D
            var capsule = GetComponent<CapsuleCollider2D>();
            if (capsule != null)
            {
                capsule.size = capsule.size * multiplier;
                capsule.offset = capsule.offset * multiplier;
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        float dt = Time.deltaTime;

        // 更新攻击时段 / 冷却计时器
        if (inAttackBurst)
        {
            burstTimer -= dt;
            if (burstTimer <= 0f)
            {
                inAttackBurst = false;
                inAttackCooldown = true;
                cooldownTimer = attackCooldownDuration;
                // 当进入仅追逐冷却期时，取消当前攻击状态
                isAttack = false;
                hasAppliedHitThisAttack = false;
                // Debug.Log("Boss: 进入仅追逐冷却期");
            }
        }
        if (inAttackCooldown)
        {
            cooldownTimer -= dt;
            if (cooldownTimer <= 0f)
            {
                inAttackCooldown = false;
                // Debug.Log("Boss: 冷却结束，可恢复攻击");
            }
        }

        attackTimer -= dt;

        // 索敌：优先使用物理层查询（OverlapCircleNonAlloc），若无结果且启用 Tag 回退，按间隔使用 Find
        if (!IsValidTarget(playerTrans))
        {
            seekTimer -= dt;
            if (seekTimer <= 0f)
            {
                SeekForPlayer();
                seekTimer = seekInterval;
            }
        }

        // 如果找到了 playerTrans 且尚未为其应用过 stats，则更新一次
        if (playerTrans != null && statsAppliedForPlayer != playerTrans)
        {
            if (syncStatsWithPlayer)
            {
                UpdateStatsFromPlayer(playerTrans);
            }
            statsAppliedForPlayer = playerTrans;
        }

        // 只有在找到 playerTrans 时才进入三段逻辑
        if (playerTrans != null && IsValidTarget(playerTrans))
        {
            float dist = Vector2.Distance(playerTrans.position, transform.position);

            // 使用更严格的判定：水平距离 + 垂直容差 + 面向 + 可视线阻挡
            if (!inAttackCooldown && IsPlayerInAttackZone(playerTrans))
            {
                // 攻击（仅在非冷却期允许发动）
                AttackBehaviour();
            }
            else if (dist <= chaseRange)
            {
                // 追击玩家（冷却期或未到攻击判定均会执行）
                ChaseBehaviour();
            }
            else
            {
                // 巡逻
                PatrolBehaviour();
            }
        }
        else
        {
            // 丢失玩家时重置攻击节制状态（当再次索敌时会重新应用）
            inAttackBurst = false;
            inAttackCooldown = false;
            burstTimer = 0f;
            cooldownTimer = 0f;
            statsAppliedForPlayer = null;

            PatrolBehaviour();
        }

        // 更新动画通用开关（Run / Idle）
        bool movingHorizontally = Mathf.Abs(rb.velocity.x) > Mathf.Epsilon;
        anim.SetBool("Run", movingHorizontally);
    }

    // 从 player 的数据更新 Boss 的 MaxHealth 与 AttackStrength（只在索敌到玩家时调用一次）
    void UpdateStatsFromPlayer(Transform player)
    {
        if (player == null) return;

        // 优先尝试从 HealthBar 静态字段读取（项目已有实现）
        bool applied = false;
        try
        {
            if (HealthBar.HealthMax > 0f)
            {
                MaxHealth = HealthBar.HealthMax * Mathf.Max(0f, playerHealthMultiplier);
                applied = true;
            }
            if (HealthBar.attackStrength > 0f)
            {
                AttackStrength = HealthBar.attackStrength * Mathf.Max(0f, playerAttackMultiplier);
                applied = true;
            }
        }
        catch
        {
            // ignore
        }

        // 回退：从 PlayerController 读取攻击力，尽量读取 MaxHealth（可能为私有）
        var pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            // AttackStrength 是 public，可以直接读取
            if (!applied || pc.AttackStrength > 0f)
            {
                AttackStrength = pc.AttackStrength * Mathf.Max(0f, playerAttackMultiplier);
                applied = true;
            }

            // 尝试通过反射读取可能为私有的 MaxHealth 字段
            var fi = typeof(PlayerController).GetField("MaxHealth", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            if (fi != null)
            {
                object val = fi.GetValue(pc);
                if (val is float fval && fval > 0f)
                {
                    MaxHealth = fval * Mathf.Max(0f, playerHealthMultiplier);
                    applied = true;
                }
            }
        }

        // 如果仍然没有可用玩家数据则保留当前数值（或可设置为默认）
        if (!applied)
        {
            Debug.LogWarning("boss: 未能读取玩家属性以同步 Boss 数值，保留现有数值。");
        }

        // 将当前生命调整为最大生命（通常希望 Boss 在被发现时满血）
        Health = MaxHealth;
    }

    // 检查 Transform 是否为有效目标（非 null 且处于激活状态）
    bool IsValidTarget(Transform t)
    {
        return t != null && t.gameObject != null && t.gameObject.activeInHierarchy;
    }

    // 更严格的攻击判定：水平距离、垂直容差、朝向以及可选的视线检测
    bool IsPlayerInAttackZone(Transform player)
    {
        if (!IsValidTarget(player)) return false;

        Vector2 dir = player.position - transform.position;
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        // 先用更严格的水平/垂直判断
        if (absX > attackHorizontalRange) return false;
        if (absY > attackVerticalTolerance) return false;

        // 若要求朝向，确保面向玩家一侧（避免背对时触发）
        if (requireFacingToAttack)
        {
            float facingSign = transform.localScale.x >= 0 ? 1f : -1f;
            if (Mathf.Sign(dir.x) != Mathf.Sign(facingSign)) return false;
        }

        // 可选：视线阻挡检测（如墙体），有阻挡则不能攻击
        if (obstacleLayer != 0)
        {
            RaycastHit2D hit = Physics2D.Linecast(transform.position, player.position, obstacleLayer);
            if (hit.collider != null)
            {
                // 若命中的是玩家本身则允许，否则阻挡
                if (hit.transform != player) return false;
            }
        }

        return true;
    }

    // 优化的索敌方法：使用 OverlapCircleNonAlloc 查找最近的玩家（基于 layer），若未设置 layer 可回退到 Tag 查找（间隔）
    void SeekForPlayer()
    {
        // 优先使用 layer 查询
        if (playerLayer.value != 0)
        {
            int found = Physics2D.OverlapCircleNonAlloc(transform.position, chaseRange, seekResults, playerLayer.value);
            if (found > 0)
            {
                // 选最近的一个（更稳健）
                float minDist = float.MaxValue;
                Transform best = null;
                for (int i = 0; i < found; i++)
                {
                    var tr = seekResults[i].transform;
                    if (tr == null) continue;
                    float d = Mathf.Abs(tr.position.x - transform.position.x) + Mathf.Abs(tr.position.y - transform.position.y);
                    if (d < minDist)
                    {
                        minDist = d;
                        best = tr;
                    }
                }
                if (best != null)
                {
                    playerTrans = best;

                    // 在索敌到玩家时更新 Boss 数值并启动攻击时段（仅对新玩家）
                    if (syncStatsWithPlayer && statsAppliedForPlayer != playerTrans)
                    {
                        UpdateStatsFromPlayer(playerTrans);
                        statsAppliedForPlayer = playerTrans;
                    }

                    // 发现玩家时开启攻击时段（只有当不在冷却期时才开启）
                    if (!inAttackCooldown)
                    {
                        inAttackBurst = true;
                        burstTimer = attackBurstDuration;
                    }

                    return;
                }
            }
        }

        // Layer 未设置或未找到，使用 Tag 回退（如果启用）
        if (enableTagFallback)
        {
            GameObject p = GameObject.FindGameObjectWithTag(playerTag);
            if (p != null)
            {
                // 额外检查玩家是否在 chaseRange 内，避免锁定过远玩家
                float d = Vector2.Distance(p.transform.position, transform.position);
                if (d <= chaseRange * 1.2f) // 允许一点缓冲
                {
                    playerTrans = p.transform;

                    // 在索敌到玩家时更新 Boss 数值并启动攻击时段（仅对新玩家）
                    if (syncStatsWithPlayer && statsAppliedForPlayer != playerTrans)
                    {
                        UpdateStatsFromPlayer(playerTrans);
                        statsAppliedForPlayer = playerTrans;
                    }

                    if (!inAttackCooldown)
                    {
                        inAttackBurst = true;
                        burstTimer = attackBurstDuration;
                    }
                }
            }
        }
    }

    void FixedUpdate()
    {
        // 在攻击动画中可能需要小幅移动以配合动画表现
        if (isAttack)
        {
            // 在攻击期间保留水平速度（由动画事件或AttackBehaviour控制）
        }
    }

    // 巡逻：在起始位置左右往返
    void PatrolBehaviour()
    {
        anim.SetBool("Idle", false);

        float leftX = startPosition.x - patrolDistance;
        float rightX = startPosition.x + patrolDistance;

        // 目标点到达则切换方向
        if (transform.position.x >= rightX)
            patrolDir = -1f;
        else if (transform.position.x <= leftX)
            patrolDir = 1f;

        Vector2 vel = new Vector2(patrolDir * patrolSpeed, rb.velocity.y);
        rb.velocity = vel;

        UpdateFacing(patrolDir);
    }

    // 追击玩家：朝玩家方向移动
    void ChaseBehaviour()
    {
        if (playerTrans == null) return;

        float dir = Mathf.Sign(playerTrans.position.x - transform.position.x);
        Vector2 vel = new Vector2(dir * runSpeed, rb.velocity.y);
        rb.velocity = vel;

        UpdateFacing(dir);
    }

    // 攻击流程：触发动画并设置 combo
    void AttackBehaviour()
    {
        if (isAttack) return;

        // 若处于冷却期，拒绝发动攻击（会由 Update 转为 Chase）
        if (inAttackCooldown) return;

        if (attackTimer > 0f) return;

        isAttack = true;
        hasAppliedHitThisAttack = false; // reset per attack

        // 如果当前不是在攻击时段，则开启一个新的攻击时段
        if (!inAttackBurst && !inAttackCooldown)
        {
            inAttackBurst = true;
            burstTimer = attackBurstDuration;
        }

        comboStep++;
        if (comboStep > 3) comboStep = 1;
        anim.SetInteger("ComboStep", comboStep);
        anim.SetTrigger("LightAttack");

        // 如果没有使用动画事件触发命中，则用延迟协程在合适时刻执行命中判定
        StartCoroutine(AttackHitRoutine());

        // 攻击后开始冷却（防止连续触发）
        attackTimer = attackInterval;
    }

    IEnumerator AttackHitRoutine()
    {
        yield return new WaitForSeconds(attackHitDelay);
        PerformAttackHit();
    }

    // 对外也提供动画事件调用的接口（在动画关键帧调用这个函数）
    public void PerformAttackHit()
    {
        if (hasAppliedHitThisAttack) return; // 已经命中过本次攻击
        hasAppliedHitThisAttack = true;

        // 决定检测中心：优先使用 attackPoint（若存在），否则基于朝向在前方偏移
        Vector2 center;
        var ap = transform.Find("attackPoint");
        if (ap != null)
        {
            center = ap.position;
        }
        else
        {
            float facing = transform.localScale.x >= 0 ? 1f : -1f;
            center = (Vector2)transform.position + Vector2.right * (facing * (attackHorizontalRange * 0.5f + 0.1f));
        }

        int mask = attackHitLayerMask.value != 0 ? attackHitLayerMask.value : ~0;
        int found = Physics2D.OverlapCircleNonAlloc(center, attackHorizontalRange, attackHitResults, mask);

        for (int i = 0; i < found; i++)
        {
            var col = attackHitResults[i];
            if (col == null) continue;

            // 优先通过组件调用 Player 的受伤函数
            var pc = col.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.PlayerHurt(AttackStrength);
                continue;
            }

            // 回退通过 Tag 判断并尝试调用
            if (col.CompareTag(playerTag))
            {
                var pc2 = col.GetComponent<PlayerController>();
                if (pc2 != null)
                {
                    pc2.PlayerHurt(AttackStrength);
                }
            }
        }

        // 清理数组引用以避免后续误判（非必须，但更安全）
        for (int i = 0; i < found; i++) attackHitResults[i] = null;
    }

    // 更新朝向（同时修改 localScale）
    void UpdateFacing(float dir)
    {
        if (dir > 0.1f && !isFacingRight)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            isFacingRight = true;
        }
        else if (dir < -0.1f && isFacingRight)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), Mathf.Abs(transform.localScale.y), Mathf.Abs(transform.localScale.z));
            isFacingRight = false;
        }
    }

    // 由动画事件或动画结束调用，表示攻击动作结束
    public void AttackOver()
    {
        isAttack = false;
        hasAppliedHitThisAttack = false; // 重置，允许下次攻击命中
    }

    // 受击接口（其他脚本可直接调用 boss.GetHurt(damage)）
    public void GetHurt(float damage)
    {
        if (isDead) return;

        // 做暴击判定
        float rnd = Random.Range(0f, 1f);
        float applied = damage;
        if (rnd <= CriticalRate)
            applied *= 1.5f;

        Health -= applied;
        anim.SetTrigger("Hurt");

        if (Health <= 0f)
        {
            Health = 0f;
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        anim.SetTrigger("Die");
        // 停止移动
        rb.velocity = Vector2.zero;
        // 可以在若干秒后销毁或禁用碰撞器：这里将此物体在动画播放完后短延迟销毁
        StartCoroutine(DieAndDestroy());
    }

    IEnumerator DieAndDestroy()
    {
        // 等待动画播放（可根据实际动画长度调整）
        yield return new WaitForSeconds(2.0f);
        // 禁用物理交互
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        // 最后销毁GameObject
        Destroy(gameObject, 1.0f);
    }

    // 调试：绘制索敌半径与攻击判定范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        // 绘制水平攻击范围（以当前朝向为参考）
        Vector3 center = transform.position;
        Gizmos.DrawWireCube(center + Vector3.right * (transform.localScale.x >= 0 ? attackHorizontalRange / 2f : -attackHorizontalRange / 2f),
                           new Vector3(attackHorizontalRange, attackVerticalTolerance * 2f, 0.1f));
    }
}