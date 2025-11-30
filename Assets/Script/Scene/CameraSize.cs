using UnityEngine;

[ExecuteAlways]
public class CameraSize : MonoBehaviour
{
    public Camera targetCamera;
    public float targetAspect = 16f / 9f; // 目标宽高比

    void Start()
    {
        if (targetCamera == null) targetCamera = GetComponent<Camera>();
        AdjustCameraRect();
    }

    void AdjustCameraRect()
    {
        // 当前屏幕宽高比
        float windowAspect = (float)Screen.width / Screen.height;
        // 目标比与当前比的差值
        float scaleHeight = windowAspect / targetAspect;

        Rect rect = targetCamera.rect;

        if (scaleHeight < 1)
        {
            // 屏幕高度不足，裁剪上下部分
            rect.width = 1;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1 - scaleHeight) / 2; // 居中
        }
        else
        {
            // 屏幕宽度不足，裁剪左右部分
            float scaleWidth = 1 / scaleHeight;
            rect.width = scaleWidth;
            rect.height = 1;
            rect.x = (1 - scaleWidth) / 2;
            rect.y = 0;
        }

        targetCamera.rect = rect;
    }

    // 屏幕分辨率变化时重新适配
    void OnScreenResize()
    {
        AdjustCameraRect();
    }
}
