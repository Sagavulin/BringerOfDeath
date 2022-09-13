using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;
    [SerializeField] float jumpSpeed = 20f;

    [SerializeField] AudioClip[] playerJump;

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

    void Update()
    {
        if (myRigidbody.velocity.y < 0) // if player if falling add multiplier
        {
            myRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        /*else if (myRigidbody.velocity.y > 0 && !value.isPressed) // if player is going up and "jump" button is not pressed
        {
            myRigidbody.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }*/

        Debug.Log(myRigidbody.velocity.y);
    }

    // only get called when "jump" button is pressed
    void OnJump(InputValue value)
    {
        if (!isAlive) { return; }

        // if player isn't touching ground player cannot jump again
        if (!myFeetCollider.IsTouchingLayers(LayerMask.GetMask("Ground"))) { return; }

        // jump if jump-button is pressed
        if (value.isPressed)
        {
            myRigidbody.velocity += new Vector2(0f, jumpSpeed);
            myAudioSource.clip = playerJump[Random.Range(0, playerJump.Length)];
            AudioManager.Instance.PlaySound(myAudioSource.clip, gameObject);
            
        }
    }

}
