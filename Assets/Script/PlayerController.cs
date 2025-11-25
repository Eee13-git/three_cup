using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float runSpeed = 2.0f;
    public float jumpSpeed;

    private Rigidbody2D playerRigidbody;
    private Animator playerAnim;
    private int JumpFrequemcy = 1;
    private BoxCollider2D playerFeet;
    private bool isGround;

    // Start is called before the first frame update
    void Start()
    {
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerFeet = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Run();
        Flip();
        Jump();
        CheckGrounded();
        SwitchAnimation();
    }

    void CheckGrounded()
    {
        isGround  = playerFeet.IsTouchingLayers(LayerMask.GetMask("Ground"));
    }

    void Flip()
    {
        bool playerHasXAxisSpeed = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        if (playerHasXAxisSpeed)
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
        float moveDir = Input.GetAxisRaw("Horizontal");
        Vector2 playerVel = new Vector2(moveDir * runSpeed, playerRigidbody.velocity.y);
        playerRigidbody.velocity = playerVel;
        bool playerHasXAxisSpeed = Mathf.Abs(playerRigidbody.velocity.x) > Mathf.Epsilon;
        playerAnim.SetBool("Run", playerHasXAxisSpeed);

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
}
