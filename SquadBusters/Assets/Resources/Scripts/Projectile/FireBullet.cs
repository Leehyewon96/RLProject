using DG.Tweening;
using UnityEngine;

public class FireBullet : Projectile
{
    private float shotTime = 5f;
    private float stunTime = 3f;

    protected override void Awake()
    {
        base.Awake();
        projectileType = ProjectileType.FireBullet;
    }

    public void SetStunTime(float newTime)
    {
        stunTime = newTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent<IPlayerAttackCircleProjectileInterface>(out IPlayerAttackCircleProjectileInterface attackCircle))
        {
            attackCircle.Stun(stunTime, AnimLocalize.knockBack);
        }
    }
}
