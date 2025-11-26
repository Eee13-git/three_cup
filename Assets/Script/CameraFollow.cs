using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public GameObject Player;
    public float positionSmooth = 5f; // 位置平滑度（值越大越快）
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void Start()
    {
        Application.targetFrameRate = 60;
        // 初始化 z 轴偏移，保持摄像机与目标的原始深度差
        offset.z = transform.position.z - Player.transform.position.z;
    }

    // 使用 LateUpdate 保证目标已经完成移动后再更新摄像机位置
    void LateUpdate()
    {
        if (Player == null) return;

        // 目标位置（只跟随 x,y，保持相机 z 不变）
        Vector3 targetPos = Player.transform.position + new Vector3(offset.x, offset.y, 0f);
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, positionSmooth * Time.deltaTime);
    }
}
