using UnityEngine;

public class Effect : MonoBehaviour
{
    public EffectType effectType;
    private ParticleSystem particle = null;

    private void Awake()
    {
        particle = GetComponent<ParticleSystem>(); 
    }

    public bool GetIsPlaying()
    {
        return particle.isPlaying;
    }

    public void Play()
    {
        particle.Play();
    }
}
