using UnityEngine;

public class Bullet : Projectile
{
    protected override void Awake()
    {
        base.Awake();
        projectileType = ProjectileType.Bullet;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<ICharacterProjectileInterface>(out ICharacterProjectileInterface player))
        {
            player.TakeDamage(damage);
            SetActive(false);
        }
    }
}
