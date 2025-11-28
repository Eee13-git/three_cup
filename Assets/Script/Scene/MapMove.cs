using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapMove : MonoBehaviour
{
    [Header("无限地图")]
    public GameObject mainCamera;
    public float mapWidth;//地图宽度
    public int mapNums;//地图重复次数

    private float totalWidth;//总地图宽度

   
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        mapWidth = GetComponent<SpriteRenderer>().bounds.size.x;
        totalWidth = mapWidth * mapNums;
    }

    
    void Update()
    {
        Vector3 temPosition = transform.position;
        if (mainCamera.transform.position.x>transform.position.x+totalWidth/2)
        {
            temPosition.x += totalWidth;//将地图向右平移一个完整的地图宽度
            transform.position = temPosition;//更新位置
        } 
        else if(mainCamera.transform.position.x < transform.position.x - totalWidth / 2)
        {
            temPosition.x -= totalWidth;//将地图向右平移一个完整的地图宽度
            transform.position = temPosition;//更新位置
        }
    }
}
