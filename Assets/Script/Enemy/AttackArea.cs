using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackArea : MonoBehaviour
{
    [SerializeField]
    private FSM enemyFSM;

    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            //添加攻击角色代码
            Debug.Log("打到了玩家");
            
            player = other.GetComponent<PlayerController>();
            player.PlayerHurt(enemyFSM.Parameter.attack);
            
        }
    }
}
