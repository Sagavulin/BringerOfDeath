using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSounds : MonoBehaviour
{
    [SerializeField] AudioClip[] sounds;

    AudioSource myAudioSource;

    void Start()
    {
        myAudioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Sounds();
        }
    }

    void Sounds()
    {
        AudioClip clip = sounds[UnityEngine.Random.Range(0, sounds.Length)];
        myAudioSource.PlayOneShot(clip);
    }
}
