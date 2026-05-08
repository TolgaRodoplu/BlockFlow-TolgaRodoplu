using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Sound
{
    public string _name;
    public AudioClip audioClip;
    [Range(0, 1)] public float volume = 1f;
    public bool loop;
    public AudioMixerGroup soundMixerType;
}

