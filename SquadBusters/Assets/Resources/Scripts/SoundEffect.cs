using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    private AudioSource audioSource = null;
    [SerializeField] private SoundEffectType soundEffectType;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public SoundEffectType GetSoundEffectType()
    {
        return soundEffectType;
    }

    public void Play()
    {
        audioSource.Play();
    }

    public void Stop()
    {
        audioSource.Stop();
    }

    public void UpdateVolume(float newVolume)
    {
        audioSource.volume = newVolume;
    }

    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    public bool IsLoop()
    {
        return audioSource.loop;
    }
}
