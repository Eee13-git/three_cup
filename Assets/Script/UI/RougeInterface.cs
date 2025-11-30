using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class RougeInterface : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed;
    public GameObject rougePanel;

    public bool isPanelActive = false;

    public BoxCollider2D boxCollider2D;

    private static int rewardSceneIndex = 0;

    IEnumerator ShowPanel(GameObject gameObject)
    {
        float timer = 0f;
        while (timer<=1)
        {
            gameObject.transform.localScale = Vector3.one * showCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }
    }
    IEnumerator HidePanel(GameObject gameObject)
    {
        float timer = 0f;
        while (timer <= 1)
        {
            gameObject.transform.localScale = Vector3.one * hideCurve.Evaluate(timer);
            timer += Time.deltaTime * animationSpeed;
            yield return null;
        }
        gameObject.transform.localScale = Vector3.zero;
    }
    public void IsShowed(bool trigger)
    {
        Debug.Log("显示");
        if (trigger&&!isPanelActive)
        {
            rougePanel.SetActive(true);
            StartCoroutine(ShowPanel(rougePanel));
            isPanelActive = true;

        }
        if(!trigger&&isPanelActive)
        {
            rougePanel.SetActive(false);
            StartCoroutine(HidePanel(rougePanel));
            isPanelActive = false;
        }
    }
    //void Start()
    //{
    //    // 防止运行时某个 Button 被选中，从而按空格触发它
    //    if (EventSystem.current != null)
    //    {
    //        EventSystem.current.SetSelectedGameObject(null);
    //    }

    //    IsShowed(true);
    //}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")
            && other.GetType().ToString() == "UnityEngine.CapsuleCollider2D")
        {
            if (rewardSceneIndex < SceneManager.GetActiveScene().buildIndex) {
                IsShowed(true);
                boxCollider2D.enabled = false;
                rewardSceneIndex++;
            }
        }
    }

}
