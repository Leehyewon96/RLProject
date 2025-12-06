using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum EffectType
{ 
    SnowHit = 0,
    Explosion,
    StonesHit,
    StarAura,
    StoneSlash,
    MagicCircle2,
    PortalRed,
    ChargeSlashPurple,
    LaserAOE,

    End,
}

public class EffectManager : MonoBehaviour
{
    List<Effect> effects = new List<Effect>();

    public void Awake()
    {
        effects = GetComponentsInChildren<Effect>(true).ToList(); 
    }


    public void Play(EffectType type, Vector3 pos, Vector3 rot)
    {
        Effect effect = effects.Find(e => e.effectType.Equals(type) && !e.GetIsPlaying());

        if (effect == null)
        {
            Effect origin = effects.Find(e => e.effectType.Equals(type));
            Effect newEffect = Instantiate(origin, transform);
            effect = newEffect.GetComponent<Effect>();
            effects.Add(effect);
        }

        effect.gameObject.transform.rotation = Quaternion.LookRotation(rot);
        effect.gameObject.transform.position = pos;
        effect.gameObject.transform.SetParent(transform);

        effect.gameObject.SetActive(true);
        effect.Play();
    }
}
