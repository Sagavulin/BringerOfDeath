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

    [SerializeField] float dashingPower = 10f;

    // Dash parameters
    bool canDash = true;
    bool isDashing;
    float dashingTime = 0.2f;
    float dashingCooldown = 1f;
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
        GroundCheck();
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

    void GroundCheck()
    {
        bool isGrounded = true;

        // ground check whether player's feet collider is touching ground
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            isGrounded = false;
        }

        // jumping is always opposite value compared to isGrounded
        myAnimator.SetBool("Jump", !isGrounded);
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

