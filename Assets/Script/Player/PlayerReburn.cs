using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerReburn : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerInfo.Instance.lastPoint = this.transform.position ;
        }
    }
}
