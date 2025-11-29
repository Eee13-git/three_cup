using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Echo : MonoBehaviour
{
    [Header("回放延迟（秒）")]
    public float delaySeconds = 1f;

    [Header("采样设置")]
    public float sampleInterval = 0f; // 0 = 每帧采样

    private Transform source; // Player（原始）
    private Animator sourceAnimator;
    private Animator echoAnimator;

    // 只针对 Player/AttackArea 上的单个 BoxCollider2D
    private BoxCollider2D sourceBox;
    private BoxCollider2D echoBox;

    // 用于保存 echo 碰撞体相对于 source 的局部变换
    private Vector3 boxLocalPos = Vector3.zero;
    private Quaternion boxLocalRot = Quaternion.identity;
    private Vector3 boxLocalScale = Vector3.one;

    private struct Snapshot
    {
        public float time;
        public Vector3 worldPos;
        public Quaternion worldRot;
        public Vector3 localScale; // 改为记录 source.localScale，以保留翻转符号
        public int animatorStateHash;
        public float animatorNormalizedTime;
        public float animatorSpeed;
        public bool colliderEnabled;
    }

    private readonly List<Snapshot> buffer = new List<Snapshot>();
    private float sampleTimer = 0f;
    private Transform colliderRoot;

    void Start()
    {
        // 优先用父物体作为源
        if (transform.parent != null && transform.parent.CompareTag("Player"))
        {
            SetSource(transform.parent);
        }
        else
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
                SetSource(found.transform);
        }

        colliderRoot = new GameObject("EchoColliderRoot").transform;
        colliderRoot.SetParent(transform, false);
    }

    void SetSource(Transform src)
    {
        source = src;
        sourceAnimator = source.GetComponent<Animator>();
        echoAnimator = GetComponent<Animator>();

        // 清理旧 echo 碰撞体
        sourceBox = null;
        if (echoBox != null)
        {
            Destroy(echoBox.gameObject);
            echoBox = null;
        }

        // 查找名为 "AttackArea" 的子物体（包含不激活的）
        Transform attackTransform = null;
        var allTransforms = source.GetComponentsInChildren<Transform>(true);
        foreach (var t in allTransforms)
        {
            if (t.name == "AttackArea")
            {
                attackTransform = t;
                break;
            }
        }

        if (attackTransform == null) return;

        // 只获取 AttackArea 上的 BoxCollider2D
        var box = attackTransform.GetComponent<BoxCollider2D>();
        if (box == null) return;

        sourceBox = box;

        // 计算 box 相对于 source 的局部变换（用于在 echo 下复位）
        boxLocalPos = source.InverseTransformPoint(box.transform.position);
        boxLocalRot = Quaternion.Inverse(source.rotation) * box.transform.rotation;
        boxLocalScale = box.transform.localScale;

        // 创建 echo 的 BoxCollider2D（作为 colliderRoot 的子对象）
        var go = new GameObject(box.name + "_echo_collider");
        go.transform.SetParent(colliderRoot, false);
        go.transform.localPosition = boxLocalPos;
        go.transform.localRotation = boxLocalRot;
        go.transform.localScale = boxLocalScale;

        var nc = go.AddComponent<BoxCollider2D>();
        nc.size = box.size;
        nc.offset = box.offset;
        nc.isTrigger = box.isTrigger;
        nc.enabled = false;
        go.layer = box.gameObject.layer;

        echoBox = nc;
    }

    void Update()
    {
        // 自动重连 source（Player 被销毁/重生）
        if (source == null)
        {
            var found = GameObject.FindWithTag("Player");
            if (found != null)
            {
                SetSource(found.transform);
            }
            return;
        }

        // 采样（按帧或间隔）
        if (sampleInterval <= 0f)
        {
            Sample();
        }
        else
        {
            sampleTimer += Time.deltaTime;
            if (sampleTimer >= sampleInterval)
            {
                sampleTimer = 0f;
                Sample();
            }
        }

        // 回放 delaySeconds 秒前的快照
        float targetTime = Time.time - delaySeconds;
        if (buffer.Count == 0) return;
        if (buffer[0].time > targetTime) return;

        int idx = buffer.FindIndex(s => s.time >= targetTime);
        Snapshot snap;
        if (idx == -1)
            snap = buffer[buffer.Count - 1];
        else
            snap = buffer[idx];

        ApplySnapshot(snap);

        // 清理过旧快照
        while (buffer.Count > 0 && buffer[0].time < targetTime - 2f)
            buffer.RemoveAt(0);
    }

    private void Sample()
    {
        if (source == null) return;

        Snapshot s = new Snapshot();
        s.time = Time.time;
        s.worldPos = source.position;
        s.worldRot = source.rotation;
        s.localScale = source.localScale; // 记录 localScale（包含负值翻转信息）

        if (sourceAnimator != null)
        {
            var st = sourceAnimator.GetCurrentAnimatorStateInfo(0);
            s.animatorStateHash = st.shortNameHash;
            s.animatorNormalizedTime = st.normalizedTime;
            s.animatorSpeed = sourceAnimator.speed;
        }
        else
        {
            s.animatorStateHash = 0;
            s.animatorNormalizedTime = 0f;
            s.animatorSpeed = 1f;
        }

        s.colliderEnabled = sourceBox != null && sourceBox.enabled;

        buffer.Add(s);

        // 限制 buffer 长度
        float keepTime = delaySeconds + 3f;
        while (buffer.Count > 0 && buffer[0].time < Time.time - keepTime)
            buffer.RemoveAt(0);
    }

    private void ApplySnapshot(Snapshot s)
    {
        // 设置 echo 的世界 transform（位置/旋转）
        transform.position = s.worldPos;
        transform.rotation = s.worldRot;
        // 应用 player 的 localScale（这样能保留翻转符号）
        transform.localScale = s.localScale;

        // Animator 回放（需 echo 配置相同 Controller）
        if (echoAnimator != null && s.animatorStateHash != 0)
        {
            echoAnimator.speed = s.animatorSpeed;
            echoAnimator.Play(s.animatorStateHash, 0, s.animatorNormalizedTime % 1f);
            echoAnimator.Update(0f);
        }

        // 应用单个碰撞体状态
        if (echoBox != null)
            echoBox.enabled = s.colliderEnabled;
    }

    private void OnDestroy()
    {
        if (colliderRoot != null)
            Destroy(colliderRoot.gameObject);
    }
}
