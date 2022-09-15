using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] GameObject arrow;
    [SerializeField] Transform bow; //sets arrow start position

    Animator myAnimator;

    void Start()
    {
        myAnimator = GetComponent<Animator>();
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            myAnimator.SetTrigger("Shooting");
            Instantiate(arrow, bow.position, transform.rotation);
        }
    }
}
