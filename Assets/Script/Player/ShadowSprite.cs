using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowSprite : MonoBehaviour
{
    private Transform player;

    private SpriteRenderer thisSprite;
    private SpriteRenderer playerSprite;

    private Color color;

    [Header("时间控制参数")]
    public float activeTime;//显示时间
    public float activeStart;//开始显示的时间点

    [Header("不透明度控制参数")]
    private float alpha;
    public float alphaSet;
    public float alphaMutiplier;

    private void OnEnable()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        thisSprite = GetComponent<SpriteRenderer>();
        playerSprite = player.GetComponent<SpriteRenderer>();

        alpha = alphaSet;

        thisSprite.sprite = playerSprite.sprite;

        transform.position = player.position;
        transform.localScale = player.localScale;
        transform.rotation = player.rotation;//保持和player一致的位置，大小方向，旋转

        activeStart = Time.time;
    }
    void Update()
    {
        alpha *= alphaMutiplier;

        color = new Color(0.5f, 0.5f, 1f, alpha);//红，绿，蓝，透明度

        thisSprite.color = color;

        if (Time.time - activeStart >= activeTime)
        {

           //返回对象池
           ShadowPool.instance.ReturnPool(this.gameObject);
        }
    }
}
