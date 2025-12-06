using UnityEngine;

public interface ICharacterProjectileInterface
{
    public void GetAOE(float inDamage, Vector3 fromPos, float distance);
    public void TakeDamage(float inDamage);
}
