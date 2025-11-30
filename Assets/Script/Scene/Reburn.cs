using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Reburn : MonoBehaviour
{
    public GameObject Player;

    private void Update()
    {
        if(Player == null)
        {
            Player = GameObject.FindWithTag("Player");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    public void ReburnGame()
    {
        Player.transform.position = PlayerInfo.Instance.lastPoint;
          if (EventSystem.current != null)
         {
              EventSystem.current.SetSelectedGameObject(null);
         }
    }
}
