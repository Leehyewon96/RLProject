using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Golem : CharacterNonPlayer
{
    protected override void MoveToEnemy(GameObject target)
    {
        if (characterState == CharacterState.Attack)
        {
            return;
        }

        if (target == gameObject)
        {
            return;
        }

        characterState = CharacterState.Attack;

        Attack(target);
    }

    protected override void Attack(GameObject target)
    {
        Vector3 dirVec = target.transform.position - transform.position;
        float angle = Quaternion.FromToRotation(transform.forward, dirVec).eulerAngles.y;
        angle += Quaternion.FromToRotation(Vector3.forward ,transform.forward).eulerAngles.y;
        dirVec = Vector3.up * angle;

        transform.DORotate(dirVec, 1f).OnComplete(() =>
        {
            if(gameObject.activeSelf)
            {
                StartCoroutine(CoAttack(target));
            }
        });

    }

    protected override IEnumerator CoAttack(GameObject target)
    {
        animator.SetBool(AnimLocalize.contactEnemy, true);
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AnimLocalize.attack));
        yield return new WaitForSeconds(0.6f);
        AOE aoe = GameManager.Instance.aoeManager.GetAOE(transform.position + transform.forward.normalized * 2f, AOEType.Yellow, 1f);
        animator.SetBool(AnimLocalize.contactEnemy, false);

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AnimLocalize.idle));
        characterState = CharacterState.Idle;
    }
}
