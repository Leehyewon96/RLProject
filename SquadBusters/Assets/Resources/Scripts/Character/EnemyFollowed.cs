using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowed : CharacterNonPlayer
{
    GameObject target;

    public void Start()
    {
        GameManager.Instance.enemyFollowed = this;
    }

    protected override void Update()
    {
        animator.SetFloat(AnimLocalize.moveSpeed, navMeshAgent.velocity.magnitude);

        if (GameManager.Instance.attackCircle != null && !isAttacking)
        {
            navMeshAgent.enabled = true;
            SetDestination(GameManager.Instance.attackCircle.transform.position);
        }
        
        target = GetTarget();
        if (target == null && target == this) return;
        //MoveToEnemy(target.gameObject);
        Attack(target.gameObject);
    }

    protected override void Attack(GameObject target)
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

        Vector3 dirVec = target.transform.position - transform.position;
        float angle = Quaternion.FromToRotation(transform.forward, dirVec).eulerAngles.y;
        angle += Quaternion.FromToRotation(Vector3.forward, transform.forward).eulerAngles.y;
        dirVec = Vector3.up * angle;

        transform.DORotate(dirVec, 1f).OnComplete(() =>
        {
            if (gameObject.activeSelf)
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
