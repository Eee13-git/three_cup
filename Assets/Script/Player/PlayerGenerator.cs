using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    public RougeInterface rougeInterface;

    void Awake()
    {
        PlayerInfo.Instance.lastPoint = this.transform.position;
        GameObject player = Instantiate(playerPrefab, PlayerInfo.Instance.lastPoint, Quaternion.identity);
        PlayerController playerController = player.GetComponent<PlayerController>();
        Debug.Log("½±Àø´ÎÊý"+rougeInterface.GetRewardSceneIndex());
        if (rougeInterface != null&&rougeInterface.GetRewardSceneIndex()==0)
        {
           playerController.ResetEcho();
           Debug.Log("¹éÁã");
            
        }
    }
}
