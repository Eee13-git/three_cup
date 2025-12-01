using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHideAndShow : MonoBehaviour
{
    public AnimationCurve showCurve;
    public AnimationCurve hideCurve;
    public float animationSpeed;
    public GameObject Panel;

    public bool isPanelActive = false;


    IEnumerator ShowPanel(GameObject gameObject)
    {
        float timer = 0f;
        while (timer <= 1)
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
   
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isPanelActive)
            {
                
                StartCoroutine(ShowPanel(Panel));
                isPanelActive = true;
                Panel.SetActive(true);
            }
            else
            {
                
                StartCoroutine(HidePanel(Panel));
                isPanelActive = false;
                Panel.SetActive(false);
            }
        }
        
    }
    private void Start()
    {
        Panel.SetActive(true);
        StartCoroutine(ShowPanel(Panel));
        isPanelActive = true;
    }
}
