using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;



public class CharacterPlayer : CharacterBase, ICharacterPlayerItemInterface
{
    public delegate void OnTakeItem();
    private List<OnTakeItem> takeItemActions = new List<OnTakeItem>();

    public delegate void UpdateCoin(int newcoin);
    public UpdateCoin updateCoin;

    public delegate int TotalCoin();
    public TotalCoin totalCoin;

    protected EffectType attackEffectType;

    protected virtual void OnEnable()
    {
        if(photonView.IsMine)
        {
            GameManager.Instance.uiManager.fastMoveUI.onMoveFast -= () => movement3D.UpdateMoveSpeed(15f);
            GameManager.Instance.uiManager.fastMoveUI.onMoveFast += () => movement3D.UpdateMoveSpeed(15f);
            GameManager.Instance.uiManager.fastMoveUI.onMoveCommon -= () => movement3D.UpdateMoveSpeed(7.5f);
            GameManager.Instance.uiManager.fastMoveUI.onMoveCommon += () => movement3D.UpdateMoveSpeed(7.5f);
            GameManager.Instance.effectManager.Play(EffectType.StarAura, transform.position, transform.forward);
        }
    }

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
            GameManager.Instance.soundManager.Stop(SoundEffectType.Walk);
            GameManager.Instance.soundManager.Stop(SoundEffectType.Shot);
            GameManager.Instance.soundManager.Play(SoundEffectType.Stun);
            StopAllCoroutines();
            //ResetPath();
            isAttacking = false;
            DetectedEnemies.Clear();
            animator.SetBool(AnimLocalize.contactEnemy, false);
            animator.SetFloat(AnimLocalize.moveSpeed, 0);
            return;
        }

        if (CheckInput())
        {
            StopAllCoroutines();
            //ResetPath();
            isAttacking = false;
            if (coroutineAttack != null)
            {
                StopCoroutine(coroutineAttack);
                coroutineAttack = null;
            }
                
            //DetectedEnemies.Clear();
            animator.SetBool(AnimLocalize.contactEnemy, false);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Move(x, z);
            //animator.SetFloat(AnimLocalize.moveSpeed, characterController.velocity.magnitude);
        }
        else if(GameManager.Instance.TrainingMode && GameManager.Instance.ChangeActionToMove)// 플레이어 조작없으면 AI 동작
        {
            StopAllCoroutines();
            //ResetPath();
            isAttacking = false;
            if (coroutineAttack != null)
            {
                StopCoroutine(coroutineAttack);
                coroutineAttack = null;
            }

            //DetectedEnemies.Clear();
            animator.SetBool(AnimLocalize.contactEnemy, false);
        }
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
            SetDestination(destinationPos);
            return;
        }

        if (isAttacking)
        {
            return;
        }

        SetDestination(target.transform.position);

        if (Vector3.Distance(transform.position, target.transform.position) <= navMeshAgent.stoppingDistance)
        {
            ResetPath();
            animator.SetFloat(AnimLocalize.moveSpeed, 0);
            Attack(target);
        }
    }

    public override void AIActionByJob(GameObject target)
    {
        if (CheckInput()) return;
        base.AIActionByJob(target);
    }

    protected virtual bool CheckInput()
    {
        //모바일에서 터치로 변경
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            if(movement3D.moveSpeed == 7.5f)
            {
                GameManager.Instance.soundManager.Stop(SoundEffectType.Run);
                GameManager.Instance.soundManager.Play(SoundEffectType.Walk);
            }
            else
            {
                GameManager.Instance.soundManager.Stop(SoundEffectType.Walk);
                GameManager.Instance.soundManager.Play(SoundEffectType.Run);
            }
            characterController.enabled = true;
            ResetPath();
            navMeshAgent.enabled = false;
            animator.SetFloat(AnimLocalize.moveSpeed, movement3D.moveSpeed);

            return true;
        }

        GameManager.Instance.soundManager.Stop(SoundEffectType.Walk);
        characterController.enabled = false;
        navMeshAgent.enabled = true;
        animator.SetFloat(AnimLocalize.moveSpeed, navMeshAgent.velocity.magnitude);
        return false;
    }

    protected override void Attack(GameObject target)
    {
        if (target == null) return;
        if(GameManager.Instance.TrainingMode)
        {
            if (Vector3.Distance(target.transform.position, gameObject.transform.position) > 5f)
                return;
        }

        base.Attack(target);
        
        //AttackTarget(target);
        //isAttacking = true;
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
            if (characterController.enabled || characterState == CharacterState.Stun) //캐릭터 상태로 판단하도록 변경하기
            {
                isAttacking = false;
                yield break;
            }
            yield return attackTerm;

            if(AttackTarget(target))
            {
                yield break;
            }
        }
    }

    protected virtual bool AttackTarget(GameObject target)
    {
        isAttacking = true;
        //var distance = Vector3.Distance(gameObject.transform.position, target.transform.position);
        //if (distance > 12f)
        //{
        //    isAttacking = false;
        //    MoveToEnemy(target);
        //    return false;
        //}

        if (target.TryGetComponent<CharacterBase>(out CharacterBase targetObj))
        {
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered, (int)attackEffectType, transform.position + Vector3.up * 1.5f + transform.forward.normalized * 0.5f, transform.forward);
            
            targetObj.TakeDamage(characterStat.GetAttackDamage());

            if (targetObj.isDead)
            {
                OnTargetDead(target);
                return true;
            }
        }

        return false;
    }

    [PunRPC]
    public void RPCEffect(int type, Vector3 pos, Vector3 rot)
    {
        GameManager.Instance.effectManager.Play((EffectType)type, pos, rot);
    }

    protected override void OnTargetDead(GameObject target)
    {
        base.OnTargetDead(target);
        GameManager.Instance.soundManager.Play(SoundEffectType.Kill);
        DetectedEnemies.Remove(target);
        animator.SetBool(AnimLocalize.contactEnemy, false);
        isAttacking = false;
    }

    protected IEnumerator CoInit()
    {
        yield return new WaitUntil(() => GameManager.Instance.effectManager != null);
        GameManager.Instance.effectManager.Play(EffectType.StarAura, transform.position, transform.forward);
        
    }

    public virtual void TakeItem(ItemType itemType)
    {
        Debug.Log($"[{gameObject.name}] itemType : {itemType.ToString()}");
        photonView.RPC("RPCTakeItem", RpcTarget.AllBuffered, (int)itemType);
    }

    [PunRPC]
    public virtual void RPCTakeItem(int itemType)
    {
        if (photonView.IsMine)
        {
            Debug.Log($" RPCTakeItem [{gameObject.name}] itemType : {itemType.ToString()}");
            takeItemActions[itemType].DynamicInvoke();
        }
    }

    public virtual void AddTakeItemActions(OnTakeItem onTakeItem)
    {
        if (!takeItemActions.Contains(onTakeItem))
        {
            takeItemActions.Add(onTakeItem);
        }
    }

    public virtual void UpdateTotalCoin(int newCoin)
    {
        if (updateCoin != null)
        {
            updateCoin.Invoke(newCoin);
        }
    }

    public int GetTotalCoin()
    {
        if(totalCoin != null)
        {
            return totalCoin.Invoke();
        }
        return 0;
    }


    [PunRPC]
    public override void RPCGetAOE(float inDamage, Vector3 fromPos, float distance)
    {
        if (photonView.IsMine && Camera.main.TryGetComponent<CameraFollow>(out CameraFollow cam))
        {
            if (cam.onCameraShake != null)
            {
                cam.onCameraShake.Invoke(0.5f);
            }
        }

        base.RPCGetAOE(inDamage, fromPos, distance);
    }

    public override void SetDead()
    {
        base.SetDead();
        GameManager.Instance.soundManager.Play(SoundEffectType.Die);
    }
}
