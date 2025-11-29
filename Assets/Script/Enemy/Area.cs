using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]

public class Area : MonoBehaviour
{
    [SerializeField]
    private FSM enemyFSM;


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
        if(other.CompareTag("Player"))
        {
            enemyFSM.Parameter.target = other.transform;

            if (enemyFSM.Parameter.enemyType == EnemyType.Goblin)
            {
                enemyFSM.Parameter.is_Ranged_Attack = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && enemyFSM.Parameter.enemyType == EnemyType.Goblin)
        {
            enemyFSM.Parameter.is_Ranged_Attack = false;
        }
    }
}
