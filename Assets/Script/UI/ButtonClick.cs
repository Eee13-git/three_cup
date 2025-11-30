using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    [Header("可选：手动指定场景中的 StartPoint（若不指定将通过名称查找）")]
    [Tooltip("场景中名为 StartPoint 的 GameObject，优先使用此项以避免 Find 调用")]
    public GameObject startPoint;

    // 缓存的引用
    private RougeInterface rougeInterface;

    void Start()
    {
        CacheRougeInterface();
    }

    // 尝试缓存 RougeInterface 引用
    void CacheRougeInterface()
    {
        if (startPoint != null)
        {
            rougeInterface = startPoint.GetComponent<RougeInterface>();
            if (rougeInterface == null)
                Debug.LogWarning("指定的 StartPoint 上未找到 RougeInterface 组件。");
            return;
        }

        // 通过名称查找场景中的 StartPoint（仅当未在 Inspector 指定时使用）
        GameObject sp = GameObject.Find("StartPoint");
        if (sp != null)
        {
            rougeInterface = sp.GetComponent<RougeInterface>();
            if (rougeInterface == null)
                Debug.LogWarning("场景中名为 StartPoint 的对象存在，但未找到 RougeInterface 组件。");
        }
        else
        {
            Debug.LogWarning("未指定 StartPoint，且场景中未找到名为 StartPoint 的对象。");
        }
    }

    // 将此方法绑定到 Button 的 OnClick() 事件（Inspector 中）
    public void OnClick_InvokeHide()
    {
        // 若缓存丢失（例如 StartPoint 在运行时创建），再次尝试查找
        if (rougeInterface == null)
            CacheRougeInterface();

        if (rougeInterface != null)
        {
            rougeInterface.IsShowed(false);
        }
        else
        {
            Debug.LogError("无法调用 IsShowed(false)：RougeInterface 引用缺失。请检查 StartPoint 是否存在并且包含 RougeInterface。");
        }
    }
}
