using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private GameObject Player;
    public string playerTag = "Player";
    public float positionSmooth = 5f; // 位置平滑度（值越大越快）
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    void Start()
    {
        Application.targetFrameRate = 60;
        // 尝试在启动时查找 Player（如果未在 Inspector 指定）
        if (Player == null)
        {
            FindPlayer();
        }

        // 如果找到了 Player，则初始化 z 轴偏移
        if (Player != null)
        {
            offset.z = transform.position.z - Player.transform.position.z;
        }
    }

    // 使用 LateUpdate 保证目标已经完成移动后再更新摄像机位置
    void LateUpdate()
    {
        // 如果引用为空，尝试动态查找（处理 Player 被销毁并重新生成的场景）
        if (Player == null)
        {
            FindPlayer();
            if (Player == null) return;
            // 新生成的 Player 找到后，重新初始化 z 偏移
            offset.z = transform.position.z - Player.transform.position.z;
        }

        // 目标位置（只跟随 x,y，保持相机 z 不变）
        Vector3 targetPos = Player.transform.position + new Vector3(offset.x, offset.y, 0f);
        targetPos.z = transform.position.z;

        transform.position = Vector3.Lerp(transform.position, targetPos, positionSmooth * Time.deltaTime);
    }

    private void FindPlayer()
    {
        var found = GameObject.FindWithTag(playerTag);
        if (found != null)
        {
            Player = found;
        }
    }
}
