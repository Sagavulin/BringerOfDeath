using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public GameObject audioSourcePrefab;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound (AudioClip clip, GameObject objectToPlay)
    {
        AudioSource.PlayClipAtPoint(clip, objectToPlay.transform.position);
    }
}
