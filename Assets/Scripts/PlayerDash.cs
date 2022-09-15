using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerDash : MonoBehaviour
{
    // dashing variables
    [SerializeField] float dashingPower = 10f;

    bool canDash = true;
    bool isDashing;
    float dashingTime = 0.2f;
    float dashingCooldown = 1f;

    Rigidbody2D myRigidbody;

    void Awake()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDashing) { return; }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (context.performed && canDash)
        {
            StartCoroutine(Dash());
            Debug.Log(myRigidbody.gravityScale);
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = myRigidbody.gravityScale;
        myRigidbody.gravityScale = 0f;
        myRigidbody.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        //trailrenderer.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        //trailrenderer.emiting = false;
        myRigidbody.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
    }
}
