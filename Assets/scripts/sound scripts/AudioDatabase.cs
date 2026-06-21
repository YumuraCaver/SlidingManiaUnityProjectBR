using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Audio/Database")]
public class AudioDatabase : ScriptableObject
{
    public SoundData[] sounds;
}

[System.Serializable]
public class SoundData
{
    public string soundName;

    [Range(0f, 1f)]
    public float volume = 1f;

    public AudioClip[] clips;
}