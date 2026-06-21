using UnityEngine;

public class AudioManager : MonoBehaviour
    // toca sons
{
    public static AudioManager Instance;

    [SerializeField] private AudioDatabase database;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Play(string soundName)
    {
        foreach (SoundData sound in database.sounds)
        {
            if (sound.soundName == soundName)
            {
                PlayClip(sound);
                return;
            }
        }

        Debug.LogWarning($"Sound '{soundName}' not found.");
    }

    //toca o clip, se tiver mais de um sendo em um soundData, sorteia um
    private void PlayClip(SoundData sound)
    {
        if (sound.clips.Length == 0)
            return;

        AudioClip chosenClip =
            sound.clips[Random.Range(0, sound.clips.Length)];

        AudioSource source = gameObject.AddComponent<AudioSource>();

        source.clip = chosenClip;
        source.volume = sound.volume;

        source.Play();

        Destroy(source, chosenClip.length);
    }
}