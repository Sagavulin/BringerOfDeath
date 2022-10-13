using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float runSpeed = 10f;
    [SerializeField] float climbSpeed = 5f;
    [SerializeField] Vector2 deathKick = new Vector2 (10f, 10f);
    [SerializeField] GameObject arrow;
    [SerializeField] Transform bow; //sets arrow start position


    // Jump
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    // audio
    [SerializeField] AudioClip[] playerJump;

    // Coyote Time parameters
    float coyoteTime = 0.1f;
    float coyoteTimeCounter;

    // Jump Buffer
    float jumpBufferTime = 0.2f;
    float jumpBufferCounter;

    // Dash
    bool canDash = true;
    bool isDashing;
    float dashingTime = 0.2f;
    float dashingCooldown = 1f;
    [SerializeField] float dashingPower = 10f;
    [SerializeField] TrailRenderer trailRenderer;

    //player movement related audioclip arrays
    [SerializeField] AudioClip[] playerRun;
    [SerializeField] AudioClip[] playerLand;

    // references to components
    Rigidbody2D myRigidbody;
    Animator myAnimator;
    CapsuleCollider2D myBodyCollider;
    BoxCollider2D myFeetCollider;
    
    AudioSource myAudioSource;

    bool isAlive = true;

    float gravityScaleAtStart;
    float moveHorizontal;
    float moveVertical;

    void Start()
    {
        // instansiating components
        myRigidbody = GetComponent<Rigidbody2D>();
        myAnimator = GetComponent<Animator>();
        myBodyCollider = GetComponent<CapsuleCollider2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        gravityScaleAtStart = myRigidbody.gravityScale;
        myAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isAlive) { return; }
        if (isDashing) { return; }

        Run();
        FlipSprite();
        ClimbLadder();
        
        if (GroundCheck())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        Die();

        //Set the yVelocity in the animator which controls blend between jumping and falling
        myAnimator.SetFloat("yVelocity", myRigidbody.velocity.y);
    }
    
    public void Move(InputAction.CallbackContext context)
    {
        if (!isAlive) { return; }

        moveHorizontal = context.ReadValue<Vector2>().x;
        moveVertical = context.ReadValue<Vector2>().y;
    }

    void Run()
    {
        Vector2 playerVelocity = new Vector2 (moveHorizontal * runSpeed, myRigidbody.velocity.y);
        myRigidbody.velocity = playerVelocity;

        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;
        myAnimator.SetBool("isRunning", playerHasHorizontalSpeed);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!isAlive) { return; }

        // jump if jump-button is pressed and feet touches ground
        if (context.performed && coyoteTimeCounter > 0f)
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }

        // if player is falling add multiplier
        if (myRigidbody.velocity.y < 0)
        {
            myRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }

        // Low Jump -- if player is going up and "jump" button is released
        else if (myRigidbody.velocity.y > 0 && context.canceled)
        {
            myRigidbody.velocity = Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            coyoteTimeCounter = 0f;
        }

        // Jumping off from ladders
        /*if (context.performed && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")) && !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }*/
    }

    void PlayerMove()
    {
       myAudioSource.clip = playerRun[Random.Range(0, playerRun.Length)];
       AudioManager.Instance.PlaySound(myAudioSource.clip, gameObject);
    }

    void FlipSprite()
    {
        bool playerHasHorizontalSpeed = Mathf.Abs(myRigidbody.velocity.x) > Mathf.Epsilon;

        if (playerHasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(myRigidbody.velocity.x), 1f);
        }
    }

    bool GroundCheck()
    {
        bool isGrounded = myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"));

        // ground check whether player's feet collider is touching ground
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            isGrounded = false;
        }

        // jumping is always opposite value compared to isGrounded
        myAnimator.SetBool("Jump", !isGrounded);
        
        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")) && !isGrounded)
        {
            myAnimator.SetBool("Jump", false);
        }

        return isGrounded;
    }

    void ClimbLadder()
    {
        // cannot climb if not touching the ladders
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            myRigidbody.gravityScale = gravityScaleAtStart;
            myAnimator.SetBool("isClimbing", false);
            return;
        }
        
        Vector2 climbVelocity = new Vector2(myRigidbody.velocity.x, moveVertical * climbSpeed);
        myRigidbody.velocity = climbVelocity;
        myRigidbody.gravityScale = 0f;

        bool playerHasVerticalSpeed = Mathf.Abs(myRigidbody.velocity.y) > Mathf.Epsilon; // Mathf.Epsilon is better than using just value "0" (zero)
        myAnimator.SetBool("isClimbing", playerHasVerticalSpeed);
    }

    void Die()
    {
        if (myBodyCollider.IsTouchingLayers(LayerMask.GetMask("Enemies", "Hazards")))
        {
            isAlive = false;
            myAnimator.SetTrigger("Death");
            myRigidbody.velocity = deathKick;
            FindObjectOfType<GameSession>().ProcessPlayerDeath();
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = myRigidbody.gravityScale;
        myRigidbody.gravityScale = 0f;
        myRigidbody.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        trailRenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        trailRenderer.emitting = false;
        myRigidbody.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}

