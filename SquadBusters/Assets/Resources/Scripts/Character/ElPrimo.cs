using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class ElPrimo : CharacterPlayer
{
    private int killCount = 0;
    [SerializeField] private int FlyingElbowAttackValue = 2;
    [SerializeField] protected float knockBackTime = 3f;
    [SerializeField] protected float knockBackDistance = 4f;

    private bool isFlyingElbowAttackMode = false;
    public Rigidbody body;
    float jumpForce = 100f;

    protected float flyingElbowDamage = 0;

    protected override void Awake()
    {
        base.Awake();
        body = GetComponent<Rigidbody>();
        flyingElbowDamage = 100f;
        characterLevel = CharacterLevel.Classic;
        attackEffectType = EffectType.ChargeSlashPurple;
    }

    private void FlyingElbowAttack(GameObject target)
    {
        StopAllCoroutines();

        isAttacking = true;
        navMeshAgent.enabled = false;
        characterController.enabled = false;
        SetCharacterState(CharacterState.InVincible);
        animator.SetTrigger(AnimLocalize.flyingElbow);

        StartCoroutine(CoFlyingElbowAttack(target));
    }

    private IEnumerator CoFlyingElbowAttack(GameObject target) 
    {
        transform.LookAt(target.transform.position);
        target.transform.LookAt(gameObject.transform.position);
        if(target.TryGetComponent<CharacterBase>(out CharacterBase targetBase))
        {
            targetBase.SetCharacterState(CharacterState.Stun);
        }

        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName(AnimLocalize.jump));

        float jumpTime = animatorController.animationClips.ToList().Find(a => a.name.Equals(AnimLocalize.jump)).length;
        float elbowTime = animatorController.animationClips.ToList().Find(a => a.name.Equals(AnimLocalize.elbow)).length;

        Vector3 startPos = transform.position;
        Vector3 targetPos = target.transform.position - transform.forward.normalized * 0.5f; // 타겟지점에서 0.5만큼 거리 두기
        Vector3 midPos = startPos + ((targetPos - startPos) / 2f) + Vector3.up * 2f;
        Vector3[] jumpPath = { startPos, midPos, targetPos };
        
        transform.DOPath(jumpPath, jumpTime, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
        {
            GameManager.Instance.soundManager.Play(SoundEffectType.Explose);
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered, (int)EffectType.PortalRed, transform.position, Vector3.up * (-1f));
        });

        yield return new WaitForSeconds(elbowTime);

        isAttacking = false;
        navMeshAgent.enabled = true;
        characterController.enabled = true;

        targetBase.KnockBack(flyingElbowDamage, knockBackTime, knockBackDistance);
        OnUnDetectEnemy(targetBase);
        SetCharacterState(CharacterState.Idle);
    }

    protected override void MoveToEnemy(GameObject target)
    {
        //animator.SetFloat(AnimLocalize.moveSpeed, navMeshAgent.velocity.magnitude);

        if (target == gameObject || target == null)
        {
            StopAllCoroutines();
            animator.SetBool(AnimLocalize.contactEnemy, false);
            isAttacking = false;
            navMeshAgent.enabled = true;
            //SetDestination(destinationPos);
            return;
        }

        if (isAttacking)
        {
            return;
        }

        if (killCount >= FlyingElbowAttackValue)
        {
            killCount = 0;
            FlyingElbowAttack(target);
            return;
        }

        SetDestination(target.transform.position);
        navMeshAgent.stoppingDistance = characterController.radius * 1.5f + target.GetComponent<CharacterController>().radius * 1.3f;

        if (Vector3.Distance(transform.position, target.transform.position) <= navMeshAgent.stoppingDistance)
        {
            ResetPath();
            //animator.SetFloat(AnimLocalize.moveSpeed, 0);
            Attack(target);
        }
    }

    protected override bool AttackTarget(GameObject target)
    {
        GameManager.Instance.soundManager.Play(SoundEffectType.Punch);
        return base.AttackTarget(target);
    }

    protected override void OnTargetDead(GameObject target)
    {
        base.OnTargetDead(target);
        killCount++;
    }
}
