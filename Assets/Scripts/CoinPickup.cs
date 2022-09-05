using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    [SerializeField] AudioClip[] coinPickups;

    AudioSource myAudioSource;

    void Awake()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            //AudioSource.PlayClipAtPoint(coinPickupSFX, Camera.main.transform.position); // most simplest way to play sound!!
            myAudioSource.clip = coinPickups[Random.Range(0, coinPickups.Length)];
            AudioManager.Instance.PlaySound(myAudioSource.clip, gameObject);

            Destroy(gameObject);
        }
    }
}
