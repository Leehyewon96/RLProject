using DG.Tweening;
using System.Collections;
using System.Linq;
using UnityEngine;

public class CharacterNonPlayer : CharacterBase
{
    protected override void Update()
    {
        base.Update();

        if (!photonView.IsMine)
        {
            return;
        }

        if (characterState == CharacterState.InVincible)
        {
            return;
        }

        if (characterState == CharacterState.Stun)
        {
            StopAllCoroutines();
            ResetPath();
            isAttacking = false;
            DetectedEnemies.Clear();
            animator.SetBool(AnimLocalize.contactEnemy, false);
            animator.SetFloat(AnimLocalize.moveSpeed, 0);
            return;
        }

        var target = GetTarget();
        MoveToEnemy(target);
    }

    protected override void Attack(GameObject target)
    {
        if (target == null) return;
        base.Attack(target);
    }

    protected override IEnumerator CoAttack(GameObject target)
    {
        TweenCallback callBack = null;
        callBack = () =>
        {
            animator.SetBool(AnimLocalize.contactEnemy, true);
            AnimationClip clip = animatorController.animationClips.ToList().Find(anim => anim.name.Equals(AnimLocalize.attack));
            attackTerm = new WaitForSecondsRealtime(clip.length);
        };
        ForwardToEnemy(target, callBack);
        while (true)
        {
            ForwardToEnemy(target);
            yield return attackTerm;
            if (target.TryGetComponent<CharacterBase>(out CharacterBase targetObj))
            {
                targetObj.TakeDamage(characterStat.GetAttackDamage());
                if (targetObj.isDead)
                {
                    animator.SetBool(AnimLocalize.contactEnemy, false);
                    isAttacking = false;
                    yield break;
                }
            }
        }
    }


    public override void OnUnDetectEnemy(CharacterBase target)
    {
        DetectedEnemies.Clear();
    }
}
