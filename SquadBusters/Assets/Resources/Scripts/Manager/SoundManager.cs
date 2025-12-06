using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum SoundEffectType
{
    LobbyBG,
    InGameBG,
    EndingBG,
    Shot,
    Walk,
    GainItem,
    UseItem,
    Explose,
    Kill,
    Die,
    Stun,
    Slash,
    Punch,
    Run,

    End,
}


public class SoundManager : MonoBehaviour
{
    private List<SoundEffect> audioSources = new List<SoundEffect>();

    private void Awake()
    {
        DontDestroyOnLoad(this);
        audioSources = GetComponentsInChildren<SoundEffect>(true).ToList();
    }

    private void Start()
    {
        audioSources.ForEach(e => e.Stop());
    }

    public void Play(SoundEffectType soundEffectType)
    {
        SoundEffect soundEffect = audioSources.Find(s => soundEffectType == s.GetSoundEffectType());
        if(soundEffect == null)
        {
            SoundEffect origin = audioSources.Find(s => soundEffectType == s.GetSoundEffectType());
            soundEffect = Instantiate(origin, transform);
            soundEffect.Stop();
            audioSources.Add(soundEffect);
        }

        if(soundEffect.GetSoundEffectType() != SoundEffectType.GainItem && soundEffect.IsPlaying())
        {
            return;
        }

        soundEffect.Play();
    }

    public void Stop(SoundEffectType soundEffectType)
    {
        List<SoundEffect> soundEffects = audioSources.FindAll(s => s.IsPlaying() && soundEffectType == s.GetSoundEffectType());
        if (soundEffects.Count == 0)
        {
            return;
        }

        soundEffects.ForEach(e => e.Stop());
    }

    public void AllStop()
    {
        List<SoundEffect> soundEffects = audioSources.FindAll(s => s.IsPlaying() && SoundEffectType.InGameBG != s.GetSoundEffectType());
        if (soundEffects.Count == 0)
        {
            return;
        }

        soundEffects.ForEach(e => e.Stop());
    }

    public void Mute()
    {
        audioSources.ForEach(a => a.UpdateVolume(0f));
    }
}
