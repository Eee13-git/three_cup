using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGenerator : MonoBehaviour
{
    public GameObject enemyPrefab;
    void Start()
    {
        GameObject enemy= Instantiate(enemyPrefab, this.transform.position, Quaternion.identity);
    }

}
