using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    void Start()
    {
        PlayerInfo.Instance.lastPoint = this.transform.position;
        GameObject player = Instantiate(playerPrefab, PlayerInfo.Instance.lastPoint, Quaternion.identity);
    }
}
