using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Colt : CharacterPlayer
{
    [SerializeField] private GameObject gunPoint = null;
    private float shotDistance = 10f;

    protected override void Update()
    {
        base.Update();
        SetDestination(destinationPos);
        if (Vector3.Distance(transform.position, navMeshAgent.destination) <= navMeshAgent.stoppingDistance)
        {
            ResetPath();
        }
    }

    protected override void MoveToEnemy(GameObject target)
    {
        if(target == gameObject)
        {
            animator.SetBool(AnimLocalize.contactEnemy, false);
            isAttacking = false;
            return;
        }

        if(isAttacking)
        {
            return;
        }
        Attack(target);
    }

    protected override void Attack(GameObject target)
    {
        isAttacking = true;
        TweenCallback callback = () =>
        {
            if (gameObject.activeSelf)
            {
                if(coroutineAttack != null)
                {
                    StopCoroutine(coroutineAttack);
                }

                coroutineAttack = StartCoroutine(CoAttack(target));
            }
        };
        ForwardToEnemy(target, callback);
    }

    protected override IEnumerator CoAttack(GameObject target)
    {
        int cnt = 1;
        animator.SetBool(AnimLocalize.contactEnemy, true);
        while (cnt > 0)
        {
            ForwardToEnemy(target);
            Projectile projectile = GameManager.Instance.projectileManager.GetProjectile(gunPoint.transform.position, ProjectileType.Bullet);
            projectile.SetDirection(transform.forward.normalized);
            projectile.SetDamage(characterStat.GetAttackDamage());
            projectile.Shot(transform.position + transform.forward.normalized * shotDistance, 1f);
            GameManager.Instance.soundManager.Play(SoundEffectType.Shot);

            cnt--;
            yield return new WaitForSeconds(0.6f);
        }

        animator.SetBool(AnimLocalize.contactEnemy, false);
        isAttacking = false;

        if(target.TryGetComponent<CharacterBase>(out CharacterBase characterBase))
        {
            if (characterBase.isDead)
            {
                OnTargetDead(target);
            }
        }
    }
}
