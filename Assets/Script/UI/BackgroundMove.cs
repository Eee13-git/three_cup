using System;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundMove : MonoBehaviour
{
    [Serializable]
    public class ParallaxLayer
    {
        public Image image; // UI Image（必须有 RectTransform）

        [HideInInspector] public RectTransform rect;
        [HideInInspector] public Vector2 baseAnchoredPos; // 初始 anchoredPosition（不随指针偏移变化）
        [HideInInspector] public Vector2 pointerOffset;   // 指针视差临时偏移（不会累加）
        [HideInInspector] public Vector2 pointerVelocity; // 用于 SmoothDamp 的速度缓存
    }

    [Header("层级设置（从远到近）")]
    public ParallaxLayer farBackground;
    public ParallaxLayer middleBackground;
    public ParallaxLayer nearBackground;

    [Header("指针视差设置")]
    public bool enablePointerParallax = true;   // 启用鼠标/触控视差
    public float pointerParallaxAmount = 10f;   // 视差强度（像素）
    public float pointerSmooth = 6f;            // 指针偏移平滑速度（值越大越快）

    private Vector2 canvasSize;
    private RectTransform canvasRect;
    private Canvas parentCanvas;

    void Start()
    {
        InitLayer(farBackground);
        InitLayer(middleBackground);
        InitLayer(nearBackground);

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.GetComponent<RectTransform>() != null)
        {
            canvasRect = parentCanvas.GetComponent<RectTransform>();
            canvasSize = canvasRect.rect.size;
        }
        else
        {
            canvasRect = null;
            canvasSize = new Vector2(Screen.width, Screen.height);
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (enablePointerParallax)
            PointerParallax(dt);
        else
            ClearPointerOffsetsSmooth(dt);
    }

    private void InitLayer(ParallaxLayer layer)
    {
        if (layer == null || layer.image == null) return;
        layer.rect = layer.image.rectTransform;
        layer.baseAnchoredPos = layer.rect.anchoredPosition;
        layer.pointerOffset = Vector2.zero;
        layer.pointerVelocity = Vector2.zero;
    }

    // 基于鼠标/触控位置的轻微视差，用于开始界面交互感（仅鼠标/触控偏移，不做滚动）
    private void PointerParallax(float dt)
    {
        Vector2 pointer;
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        pointer = Input.mousePosition;
#else
        if (Input.touchCount > 0) pointer = Input.GetTouch(0).position;
        else pointer = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
#endif
        // 将屏幕坐标映射到 -1..1（使用画布本地坐标以减小抖动）
        Vector2 normalized;
        if (canvasRect != null && parentCanvas != null)
        {
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointer, parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : parentCanvas.worldCamera, out localPoint);
            // localPoint 的范围通常为 -rect.size/2 .. +rect.size/2（以 pivot 为中心），归一化到 -1..1
            float halfW = canvasSize.x * 0.5f;
            float halfH = canvasSize.y * 0.5f;
            if (halfW <= 0f) halfW = Screen.width * 0.5f;
            if (halfH <= 0f) halfH = Screen.height * 0.5f;
            normalized = new Vector2(Mathf.Clamp(localPoint.x / halfW, -1f, 1f), Mathf.Clamp(localPoint.y / halfH, -1f, 1f));
        }
        else
        {
            normalized = new Vector2(
                (pointer.x / (canvasSize.x > 0 ? canvasSize.x : Screen.width) - 0.5f) * 2f,
                (pointer.y / (canvasSize.y > 0 ? canvasSize.y : Screen.height) - 0.5f) * 2f
            );
        }

        // 远中近层的目标偏移（可按需调整系数）
        Vector2 targetFar = normalized * (pointerParallaxAmount * 0.3f * -1f);
        Vector2 targetMid = normalized * (pointerParallaxAmount * 0.6f * -1f);
        Vector2 targetNear = normalized * (pointerParallaxAmount * 1.0f * -1f);

        UpdatePointerOffsetSmooth(farBackground, targetFar, dt);
        UpdatePointerOffsetSmooth(middleBackground, targetMid, dt);
        UpdatePointerOffsetSmooth(nearBackground, targetNear, dt);
    }

    private void UpdatePointerOffsetSmooth(ParallaxLayer layer, Vector2 targetOffset, float dt)
    {
        if (layer == null || layer.image == null) return;
        // 将 pointerSmooth 转换为 SmoothDamp 的 smoothTime：pointerSmooth 越大，响应越快（smoothTime 越小）
        float smoothTime = 1f / Mathf.Max(0.01f, pointerSmooth);
        layer.pointerOffset = Vector2.SmoothDamp(layer.pointerOffset, targetOffset, ref layer.pointerVelocity, smoothTime, Mathf.Infinity, dt);
        layer.rect.anchoredPosition = layer.baseAnchoredPos + layer.pointerOffset;
    }

    private void ClearPointerOffsetsSmooth(float dt)
    {
        UpdatePointerOffsetSmooth(farBackground, Vector2.zero, dt);
        UpdatePointerOffsetSmooth(middleBackground, Vector2.zero, dt);
        UpdatePointerOffsetSmooth(nearBackground, Vector2.zero, dt);
    }

    // 外部接口：开启/关闭指针视差
    public void SetPointerParallaxEnabled(bool on)
    {
        enablePointerParallax = on;
    }

    // 外部接口：设置视差强度
    public void SetPointerParallaxAmount(float amount)
    {
        pointerParallaxAmount = amount;
    }
}
