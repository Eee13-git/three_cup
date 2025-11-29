using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随设置")]
    private GameObject Player;
    public string playerTag = "Player";
    public float positionSmooth = 5f; // 位置平滑度（值越大越快）
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("景深控制")]
    public Transform farBackground,middleBackFround,nearBackground;//远景、中景、近景
    private Vector2 lastPos;//上一帧摄像机位置

    [Header("移动范围")]
    public Vector2 minPosition;
    public Vector2 maxPosition;

    void Start()
    {
        Application.targetFrameRate = 50;
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

        lastPos = transform.position;//记录相机初始位置
    }

    // 使用 LateUpdate 保证目标已经完成移动后再更新摄像机位置
    void LateUpdate()
    {
        CameraMove();
        BackgroundMove();
    }

    private void CameraMove()
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

        targetPos.x = Mathf.Clamp(targetPos.x, minPosition.x, maxPosition.x);
        targetPos.y = Mathf.Clamp(targetPos.y, minPosition.y, maxPosition.y);


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

    private void BackgroundMove()
    {
        Vector2 amountToMove = new Vector2(transform.position.x - lastPos.x, transform.position.y - lastPos.y);
        //根据摄像机移动的距离，按比例移动不同景深的背景
        farBackground.position += new Vector3(amountToMove.x * 0.8f, amountToMove.y * 0.8f, 0);
        middleBackFround.position += new Vector3(amountToMove.x * 0.5f, amountToMove.y * 0.5f, 0);
        nearBackground.position += new Vector3(amountToMove.x * 0.2f, amountToMove.y * 0.2f, 0);

        lastPos = transform.position;
    }

    public void SetCamPosLimit(Vector2 minPos,Vector2 maxPos)
    {
        minPosition = minPos;
        maxPosition = maxPos;
    }
}
