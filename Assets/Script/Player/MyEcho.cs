using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyEcho : MonoBehaviour
{
    private bool isDead;
    [Header("记录状态设置")]
    [SerializeField]
    private float recordInterval = 0.02f; // 记录间隔50帧
    [SerializeField]
    public float targetDelay = 0.5f;
    private float lastRecordTime = 0f;

    [SerializeField]
    private GameObject player;
    [SerializeField]
    
    private Animator playerAnimator;
    [SerializeField]
    private BoxCollider2D playerAttackArea;
    [SerializeField]
    private Animator echoAnimator;
    [SerializeField]
    private BoxCollider2D echoCollider;

    private PlayerController playerController;

    [Header("打击感")]
    public float shakeTime;

    public int lightPause;

    public float lightStrength;

    public float AttackStrength;


    private struct PlayerState{
        public float time;
        public Vector3 worldPos;
        public Vector3 localScale;
        public int animatorStateHash;// 动画状态哈希值
        public float animatorNormalizedTime;// 动画归一化时间
        public float animatorSpeed;// 动画播放速度
        public Vector2 attackBoxOffset;
        public Vector3 attackBoxSize;
        public bool colliderEnabled;
    }

    private Queue<PlayerState> stateQueue = new Queue<PlayerState>();

    void Awake()
    {
        // 如果 echo 是 player 的子物体，解除父子关系，保持世界位置不变
        // 这样 echo 的世界坐标不会随 player 的后续移动被改变
        if (transform.IsChildOf(player.transform))
        {
            transform.SetParent(null, true);
        }
        
        playerController = player.GetComponent<PlayerController>();
        AttackStrength = playerController.AttackStrength*0.5f;
    }

    void Start()
    {
        // 先记录一次初始状态，初始化 lastRecordTime
        RecordState();
        lastRecordTime = Time.time;
    }

    void Update()
    {
        if (!playerController.isDead)
        {
            // 按间隔记录状态
            if (Time.time - lastRecordTime >= recordInterval)
            {

                RecordState();
                lastRecordTime = Time.time;

            }

            // 根据延迟播放历史状态（使用向上取整确保延迟至少为目标值）
            UpdateState();
            isDead = false;
        }
        if (playerController.isDead && !isDead)
        {
            echoAnimator.Play("Die");
            isDead = true;
        }

    }

    private void RecordState()
    {
        var ani = playerAnimator.GetCurrentAnimatorStateInfo(0);
        PlayerState currentState = new PlayerState
        {
            time = Time.time,
            worldPos = player.transform.position,
            localScale = player.transform.localScale,
            animatorStateHash = ani.shortNameHash,
            animatorNormalizedTime = ani.normalizedTime,
            animatorSpeed = playerAnimator.speed,
            attackBoxOffset = playerAttackArea.offset,
            attackBoxSize = playerAttackArea.size,
            colliderEnabled = playerAttackArea.enabled
        };
        stateQueue.Enqueue(currentState);
    }

    private void UpdateState()
    {
        int requiredSamples = Mathf.CeilToInt(targetDelay / recordInterval);
        if (stateQueue.Count >= requiredSamples && requiredSamples > 0)
        {
            PlayerState newState = stateQueue.Dequeue();
            // 应用记录的动画状态与播放进度
            echoAnimator.speed = newState.animatorSpeed;
            echoAnimator.Play(newState.animatorStateHash, 0, newState.animatorNormalizedTime % 1f);

            // 应用碰撞体与位置（由于 echo 已脱离 player 父物体，这里使用世界坐标）
            echoCollider.enabled = newState.colliderEnabled;
            echoCollider.offset = newState.attackBoxOffset;
            echoCollider.size = newState.attackBoxSize;

            this.transform.localScale = newState.localScale;
            this.transform.position = newState.worldPos;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
           
                AttackSense.Instance.HitPause(lightPause);
                AttackSense.Instance.CameraShake(shakeTime, lightStrength);
            
            //敌人受伤的函数
            Debug.Log("命中");
            FSM fsm = other.GetComponent<FSM>();
            fsm.GetHurt(AttackStrength);
        }
        if (other.CompareTag("Boss"))
        {
            boss boss_1 = other.GetComponent<boss>();
            boss_1.GetHurt(AttackStrength);
            AttackSense.Instance.HitPause(lightPause);
            AttackSense.Instance.CameraShake(shakeTime, lightStrength);
        }
    }
}
