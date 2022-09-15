using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    // Jump parameters
    [SerializeField] float jumpSpeed = 20f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    // Coyote Time parameters
    float coyoteTime = 1.0f;
    float coyoteTimeCounter;

    // Audio arrays
    [SerializeField] AudioClip[] playerJump;
    
    // References to components
    Rigidbody2D myRigidbody;
    BoxCollider2D myFeetCollider;
    AudioSource myAudioSource;

    float gravityScaleAtStart;

    bool isAlive = true;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        myAudioSource = GetComponent<AudioSource>();
        gravityScaleAtStart = myRigidbody.gravityScale;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (!isAlive) { return; }

        if (myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

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
        if (context.performed && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")) && !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }
    } 
}


