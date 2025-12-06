using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Boller : CharacterNonPlayer
{
    [SerializeField] private GameObject stonePos = null;

    protected override void Awake()
    {
        base.Awake();
        characterType = CharacterType.Boller;
    }

    protected override void MoveToEnemy(GameObject target)
    {
        if (target == gameObject)
        {
            return;
        }

        if (isAttacking)
        {
            return;
        }

        isAttacking = true;

        Attack(target);
    }

    protected override void Attack(GameObject target)
    {
        TweenCallback callback = () =>
        {
            StartCoroutine(CoAttack(target));
        };
        ForwardToEnemy(target, callback);
    }

    protected override IEnumerator CoAttack(GameObject target)
    {
        animator.SetBool(AnimLocalize.attack, true);
        Projectile projectile = GameManager.Instance.projectileManager.GetProjectile(stonePos.transform.position, ProjectileType.Stone);
        Stone stone = projectile.GetComponent<Stone>();
        stone.SetHost(stonePos);
        yield return new WaitForSeconds(0.8f);

        stone.Shot(stonePos.transform.position, target.transform.position, 1f);

        yield return new WaitForSeconds(1.4f);
        animator.SetBool(AnimLocalize.attack, false);
        isAttacking = false;
    }
}
