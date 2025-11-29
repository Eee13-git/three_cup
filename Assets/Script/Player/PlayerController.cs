using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private bool isInputEnabled = true;

    [Header("角色属性面板")]
    [SerializeField]
    private float Health;
    [SerializeField]
    private float MaxHealth;
    [SerializeField]
    private float AttackStrength;
    [SerializeField]
    private float CriticalRate;
    [SerializeField]
    public float DieTime;
    [Header("补偿速度")]
    public float lightSpeed;

    [Header("打击感")]
    public float shakeTime;

    public int lightPause;

    public float lightStrength;
    [Header("CD的UI组件")]
    public Image cdImage;

    [Header("Dash参数")]
    public float dashTime;//dash时长
    private float dashTimeLeft;//dash剩余时间
    private float lastDash=-10f;//上次dash的时间点
    public float dashCoolDown;
    public float dashSpeed;
    public bool isDashing;
    [Space]
    private float moveDir;//移动方向

    public float runSpeed = 2.0f;
    public float jumpSpeed;

    private int comboStep;

    public float interval = 2f;

    private float timer;

    private bool isAttack;

    private bool isDefend;

    public bool isDead;

    private string attackType;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnim;
    private CircleCollider2D playerFeet;
    private bool isGround;

    // Start is called before the first frame update

    void Start()
    {
        updateUI();

        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerFeet = GetComponent<CircleCollider2D>();
        cdImage = GameObject.Find("Dash").GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isInputEnabled) {
            Run();
            Flip();
            Jump();
            CheckGrounded();
            SwitchAnimation();

            Attack();
            Defend();
            Dash();
        }
        cdImage.fillAmount -= 1.0f/dashCoolDown*Time.deltaTime;
    }

    public void updateUI()
    {
        HealthBar.HealthMax = MaxHealth;
        HealthBar.HealthCureent = Health;
        HealthBar.attackStrength = AttackStrength;
        HealthBar.criticalRate = CriticalRate;
    }

    private void FixedUpdate()
    {
        DashAct();
        

    }
    void CheckGrounded()
    {
        isGround  = playerFeet.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    void Flip()
    {
        bool playerHasXAxisSpeed = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasXAxisSpeed&&!isAttack)
        {
            if(playerRigidbody.velocity.x > 0.1f)
            {
                transform.localScale = new Vector2(1, 1);
            }
            else if(playerRigidbody.velocity.x < -0.1f)
            {
                transform.localScale = new Vector2(-1, 1);
            }
        }
    }

    void Run()
    {
        moveDir = Input.GetAxisRaw("Horizontal");

        if (!isAttack) 
        {
            Vector2 playerVel = new Vector2(moveDir * runSpeed, playerRigidbody.velocity.y);
            playerRigidbody.velocity = playerVel;

            bool playerHasXAxisSpeed = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
            playerAnim.SetBool("Run", playerHasXAxisSpeed);
        }else if (attackType=="Light")
        {
            Vector2 playerVel = new Vector2(moveDir * lightSpeed, playerRigidbody.velocity.y);
            playerRigidbody.velocity = playerVel;
        }
        

    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            //CheckGrounded();
            if (isGround) {

                playerAnim.SetBool("Jump", true);
                Vector2 jumpVel = new Vector2(0f, jumpSpeed);
                playerRigidbody.velocity = Vector2.up * jumpVel;

            }
            
        }
    }

    void SwitchAnimation()
    {
        playerAnim.SetBool("Idle", false);
        if (playerAnim.GetBool("Jump"))
        {
            if (playerRigidbody.velocity.y < 0)
            {
                playerAnim.SetBool("Jump", false);
                playerAnim.SetBool("Fall", true);
            }
        }
        else if (isGround)
        {
            playerAnim.SetBool("Fall", false);
            playerAnim.SetBool("Idle", true);
        }else if (!isGround)
        {
            playerAnim.SetBool("Jump", false);
            playerAnim.SetBool("Fall", true);
        }

    }

    void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isAttack)
        {
            isAttack = true;
            attackType = "Light";
            comboStep++;
            if(comboStep>3)
                comboStep = 1;
            timer = interval;
            playerAnim.SetTrigger("LightAttack");
            playerAnim.SetInteger("ComboStep", comboStep);
        }

        if (timer!=0)
        {
            timer-= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                comboStep = 0;
            }
        }

    }

    void Defend()
    {
        if(Input.GetKeyDown(KeyCode.E) && !isAttack&&!isDefend)
        {
            isDefend = true;
            playerAnim.SetTrigger("Defend");
        }
    }

    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            if(Time.time >= (lastDash + dashCoolDown))
            {
               ReadyToDash();
               
            }
        }
    }
    void ReadyToDash()
    {
        isDashing = true;

        dashTimeLeft = dashTime;

        lastDash = Time.time;

        cdImage.fillAmount = 1.0f;
    }

    void DashAct()
    {
        if (isDashing)
        {
            if (dashTimeLeft > 0)
            {
                if (playerRigidbody.velocity.y > 0 && !isGround)
                {
                    playerRigidbody.velocity = new Vector2(dashSpeed * moveDir, jumpSpeed);
                }

                playerRigidbody.velocity =new Vector2(dashSpeed * moveDir, playerRigidbody.velocity.y);

                dashTimeLeft-= Time.deltaTime;

                ShadowPool.instance.GetFromPool();
            }
            if (dashTimeLeft <= 0)
            {
            isDashing =false;
                if (!isGround)
                {
                    playerRigidbody.velocity = new Vector2(dashSpeed * moveDir, jumpSpeed);
                }
            }

        }
        
    }


    public void AttackOver()
    {
        isAttack = false;
    }

    public void DefendOver()
    {
        isDefend = false;
    }

    //攻击检测
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            //if (attackType == "Light")
            //{
                AttackSense.Instance.HitPause(lightPause);
                AttackSense.Instance.CameraShake(shakeTime, lightStrength);
            //}
            //敌人受伤的函数
            //Debug.Log("命中");
            FSM fsm = other.GetComponent<FSM>();


            //暴击判定
            float randomValue = Random.Range(0f, 1f);

            if (randomValue<=CriticalRate) {
               fsm.GetHurt(AttackStrength*1.5f);
            }
            fsm.GetHurt(AttackStrength);

        }
        if (isDefend)
        {
            playerAnim.SetTrigger("Defend_2");
            isDefend = false;
        }
    }

    public void PlayerHurt(float damage)
    {
        
        
        if (Health>0)
        {
            playerAnim.SetTrigger("Hurt");
            Health -= damage;
        }
        if (Health <= 0&&!isDead)
        {
            Health = 0;
            isDead = true;
            playerAnim.SetTrigger("Die");
            //玩家重生
            isInputEnabled = false;
            Invoke("PlayerReburn",DieTime);
        }
        HealthBar.HealthCureent = Health;
    }

    private void PlayerReburn()
    {
        isDead = false;
        playerAnim.Play("Player_Idle");
        this.transform.position = PlayerInfo.Instance.lastPoint;
        Health = MaxHealth;
        HealthBar.HealthCureent = Health;
        isInputEnabled = true;
    }
}
