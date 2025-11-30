using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RougeChoose : MonoBehaviour
{
    [Header("配置")]
    // 5个UI预制体（在Inspector中拖拽赋值）
    public List<GameObject> uiPrefabs = new List<GameObject>();
    // 生成的UI之间的间距（像素）
    public float spacing = 20f;

    private RectTransform panelRect; // Panel的RectTransform

    void Start()
    {
        panelRect = GetComponent<RectTransform>();
        GenerateRandomUI();
    }

    /// <summary>
    /// 核心方法：随机选择3个预制体并水平均匀分布在Panel上
    /// </summary>
    void GenerateRandomUI()
    {
        // 安全校验
        if (uiPrefabs.Count != 5)
        {
            Debug.LogError("请赋值5个UI预制体！");
            return;
        }

        if (panelRect == null)
        {
            Debug.LogError("找不到 Panel 的 RectTransform。");
            return;
        }

        // 1. 从5个预制体中随机选择3个（去重）
        List<GameObject> selectedPrefabs = uiPrefabs
            .OrderBy(_ => Random.value) // 随机排序
            .Take(3) // 取前3个
            .ToList();

        // 2. 先实例化选中的预制体，收集它们的 RectTransform（使用实例的实际大小）
        List<RectTransform> instantiatedRects = new List<RectTransform>();
        for (int i = 0; i < selectedPrefabs.Count; i++)
        {
            GameObject uiObj = Instantiate(selectedPrefabs[i], transform);
            uiObj.name = $"RandomUI_{i}";

            RectTransform uiRect = uiObj.GetComponent<RectTransform>();
            if (uiRect != null)
            {
                instantiatedRects.Add(uiRect);
            }
            else
            {
                Debug.LogWarning($"选中的预制体 {selectedPrefabs[i].name} 不包含 RectTransform。");
            }
        }

        // 3. 计算位置并赋值
        List<Vector2> positions = CalculateHorizontalPositions(instantiatedRects);
        for (int i = 0; i < instantiatedRects.Count && i < positions.Count; i++)
        {
            instantiatedRects[i].anchoredPosition = positions[i];
        }
    }

    /// <summary>
    /// 计算水平方向均匀分布的位置（仅水平排列）
    /// 说明：
    /// - 优先使用用户设置的 spacing（当它能在 Panel 中放下时）。
    /// - 若用户 spacing 太大导致无法合理布局，则根据可用空间计算动态间隙（包括左右外边距），以避免物体被过度拉开。
    /// </summary>
    /// <param name="uiRects">已实例化的 UI RectTransform 列表</param>
    List<Vector2> CalculateHorizontalPositions(List<RectTransform> uiRects)
    {
        List<Vector2> positions = new List<Vector2>();
        if (uiRects == null || uiRects.Count == 0 || panelRect == null)
            return positions;

        // 获取Panel的宽度（实际显示区域）
        float panelWidth = panelRect.rect.width;

        // 计算总占用宽度：所有UI宽度之和
        float totalUIWidth = 0f;
        foreach (var uiRect in uiRects)
        {
            totalUIWidth += uiRect.rect.width;
        }

        int count = uiRects.Count;
        float requiredSpacingTotal = spacing * (count - 1);

        // 如果用户设置的 spacing 可以放下（总宽度 + 间距 <= Panel 宽度），则使用用户 spacing 并居中。
        if (totalUIWidth + requiredSpacingTotal <= panelWidth)
        {
            float totalOccupiedWidth = totalUIWidth + requiredSpacingTotal;
            float startX = -totalOccupiedWidth / 2f;
            float currentX = startX;
            foreach (var uiRect in uiRects)
            {
                float uiWidth = uiRect.rect.width;
                float xPos = currentX + uiWidth / 2f;
                positions.Add(new Vector2(xPos, 0));
                currentX += uiWidth + spacing;
            }
        }
        else
        {
            // 用户 spacing 太大，按可用空间分配间隙（包括左右两侧外边距），使间隙均等
            float available = panelWidth - totalUIWidth;
            if (available <= 0f)
            {
                // Panel 宽度不足以容纳所有元素，简单地按元素总宽居中堆叠（间隙为0）
                float startX = -totalUIWidth / 2f;
                float currentX = startX;
                foreach (var uiRect in uiRects)
                {
                    float uiWidth = uiRect.rect.width;
                    float xPos = currentX + uiWidth / 2f;
                    positions.Add(new Vector2(xPos, 0));
                    currentX += uiWidth; // 间隙为0
                }
            }
            else
            {
                float gap = available / (count + 1); // 包含左右两侧外边距
                float currentX = -panelWidth / 2f + gap;
                foreach (var uiRect in uiRects)
                {
                    float uiWidth = uiRect.rect.width;
                    float xPos = currentX + uiWidth / 2f;
                    positions.Add(new Vector2(xPos, 0));
                    currentX += uiWidth + gap;
                }
            }
        }

        return positions;
    }
}
