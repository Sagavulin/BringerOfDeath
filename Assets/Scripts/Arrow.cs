using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    [SerializeField] float arrowSpeed = 5f;
    Rigidbody2D myRigidbody;
    PlayerMovement player;

    float xSpeed;

    void Start()
    {
        myRigidbody = GetComponent<Rigidbody2D>();
        player = FindObjectOfType<PlayerMovement>();
        xSpeed = player.transform.localScale.x * arrowSpeed;
    }

    
    void Update()
    {
        myRigidbody.velocity = new Vector2(xSpeed, 0f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // destroys enemy when hit
        if(other.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }

        // destroys arrow that hit enemy
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // destroy arrow when it hits anything
        Destroy(gameObject);
    }
}
