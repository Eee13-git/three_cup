using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar : MonoBehaviour
{
    public Text healthText;
    public Text attackText;
    public Text criticalText;


    public static float HealthCureent;
    public static float HealthMax;

    public static float attackStrength;
    public static float criticalRate;

    private float crRate;

    private Image healthBar;

    // Start is called before the first frame update
    void Start()
    {
        healthBar = GetComponent<Image>();
        //HealthCureent = HealthMax;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.fillAmount = HealthCureent / HealthMax;
        healthText.text = HealthCureent.ToString() + " / " + HealthMax.ToString();

        attackText.text = "Strength   " + attackStrength.ToString();
        crRate = criticalRate*100;
        criticalText.text = "Critical Rate  " + crRate.ToString() + "%";
    }
}
