using System;
using System.Collections;
using UnityEngine;

public class HeliAudio : MonoBehaviour
{
    [SerializeField] private AudioClip _voiceAudio;
    [SerializeField] private AudioClip _heliSoundEffect;
    
    private AudioSource _audioSource;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        StartCoroutine(HandleHeliAudio());
    }

    private IEnumerator HandleHeliAudio()
    {
        _audioSource.PlayOneShot(_heliSoundEffect);
        yield return new WaitForSeconds(1f);
        _audioSource.PlayOneShot(_voiceAudio);
    }
}
