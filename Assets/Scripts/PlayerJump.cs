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

    // Audio arrays
    [SerializeField] AudioClip[] playerJump;

    // References to components
    Rigidbody2D myRigidbody;
    BoxCollider2D myFeetCollider;
    AudioSource myAudioSource;

    bool isAlive = true;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        myFeetCollider = GetComponent<BoxCollider2D>();
        myAudioSource = GetComponent<AudioSource>();
    }
    
    public void Jump(InputAction.CallbackContext context)
    {
        if (!isAlive) { return; }

        // jump if jump-button is pressed and feet touches ground

        if (context.performed && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }
        
        // Low Jump -- if player is going up and "jump" button is released
        if (myRigidbody.velocity.y > 0 && context.canceled)
        {
            myRigidbody.velocity = Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        
        // if player is falling add multiplier
        else if (myRigidbody.velocity.y < 0)
        {
            myRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        
        /* Jumping off from ladders
        if (context.performed && myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")) && !myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
        }*/
    }
}


