using DG.Tweening;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CharacterType
{
    ElPrimo,
    Greg,
    Colt,
    PlayerEnd,

    Eggy,
    Chilli,
    Kiwi,
    BabyDragon,
    Golem,
    Boller,

    End,
}

public enum CharacterLevel
{
    NPC,
    Classic,
    Super,
    Ultra,

    End,
}

public enum CharacterState
{
    Idle,
    Attack,
    Dead,
    Stun,
    InVincible,
}

public class CharacterBase : MonoBehaviour, ICharacterProjectileInterface
{
    //protected AttackCircle attackCircle = null;
    protected HpBar hpBar = null;

    protected Animator animator = null;
    protected RuntimeAnimatorController animatorController = null;
    protected Movement3D movement3D = null;
    protected CharacterStat characterStat = null;
    protected NavMeshAgent navMeshAgent = null;
    protected CharacterController characterController = null;

    protected WaitForSecondsRealtime attackTerm = new WaitForSecondsRealtime(0.933f);

    protected CharacterState characterState = CharacterState.Idle;
    protected CharacterLevel characterLevel = CharacterLevel.Classic;
    protected bool isAttacking = false; // CharacterStat안에 있어야 되나?
    public bool isDead { get; protected set; } = false;

    protected List<GameObject> DetectedEnemies = new List<GameObject>();
    [SerializeField] protected CharacterType characterType = CharacterType.ElPrimo;
    [SerializeField] protected GameObject hpBarOrigin;
    [SerializeField] public GameObject attackCircleOrigin;
    protected TrainingManager trainingManager;

    protected PhotonView photonView = null;
    protected Vector3 destinationPos;

    public delegate void DeadAction(CharacterBase characterBase);
    public DeadAction deadAction = null;

    protected Coroutine coroutineAttack;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        animatorController = animator.runtimeAnimatorController;
        characterStat = GetComponent<CharacterStat>();
        movement3D = GetComponent<Movement3D>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        characterController = GetComponent<CharacterController>();
        hpBar = GetComponentInChildren<HpBar>();
        photonView = GetComponent<PhotonView>();
        trainingManager = GameManager.Instance.attackCircle.GetComponent<TrainingManager>();
        Init();
    }

    protected virtual void OnDisable()
    {
        if(coroutineAttack != null)
        {
            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }
    }

    protected virtual void Update()
    {
        if(GameManager.Instance.endGame)
        {
            return;
        }
        //attackCircle.MoveAttackCircle(transform.position);
        hpBar.UpdatePos(transform.position + new Vector3(0, characterController.height + 0.4f, 0));

        //업데이트용 이벤트 하나 생성 후 상속받는 클래스에서 Start에서 다 등록.
        //여기서 업데이트용 이벤트 계속 Invoke하기(상속받는 클래스에는 Update 작성 X)
    }

    public virtual void Init()
    {
        InitCharacterStat();
        hpBar.SetMaxHp(characterStat.GetMaxHp());
        hpBar.UpdateCurrentHp(characterStat.GetCurrentHp());
        if(!photonView.IsMine)
        {
            hpBar.InitColor(Color.red);
        }

        characterStat.onCurrentHpChanged -= hpBar.UpdateCurrentHp;
        characterStat.onCurrentHpChanged += hpBar.UpdateCurrentHp;
        characterStat.onCurrentHpZero -= delegate { trainingManager?.OnUnitDead(); };
        characterStat.onCurrentHpZero += delegate { trainingManager?.OnUnitDead(); };
        characterStat.onCurrentHpZero -= SetDead;
        characterStat.onCurrentHpZero += SetDead;

        if (!photonView.IsMine)
        {
            return;
        }
    }

    protected virtual void InitCharacterStat()
    {
        Dictionary<string, object> stat = CSVReader.Read("CharacterStat").Find(s => s["name"].ToString().Equals(characterType.ToString()));
        if(stat == null)
        {
            Debug.Log($"{characterType.ToString()} 없음");
        }
        else
            characterStat.Init(float.Parse(stat.GetValueOrDefault("maxHp").ToString()), float.Parse(stat.GetValueOrDefault("attackDamage").ToString()), int.Parse(stat.GetValueOrDefault("coin").ToString()), int.Parse(stat.GetValueOrDefault("gem").ToString()));
        //Debug.Log($"[{gameObject.name}] : maxHp : {characterStat.GetMaxHp()}");
    }

    public virtual void SetCharacterState(CharacterState newState)
    {
        photonView.RPC("RPCSetCharacterState", RpcTarget.AllBuffered, newState);
    }

    public virtual void PlayStunAnimation()
    {
        animator.SetTrigger(AnimLocalize.knockBack);
    }

    [PunRPC]
    public virtual void RPCSetCharacterState(CharacterState newState)
    {
        characterState = newState;
    }

    public virtual CharacterType GetCharacterType()
    {
        return characterType;
    }

    public virtual void TakeDamage(float inDamage)
    {
        photonView.RPC("RPCTakeDamage", RpcTarget.AllBuffered, inDamage);
    }

    [PunRPC]
    public virtual void RPCTakeDamage(float inDamage)
    {
        if (isDead)
        {
            return;
        }
        characterStat.ApplyDamage(inDamage);

        if(characterStat.CheckDead())
        {
            if(photonView.IsMine)
            {
                if (deadAction != null)
                {
                    deadAction.Invoke(this);
                }
            }
        }
        GameManager.Instance.effectManager.Play(EffectType.StonesHit, gameObject.transform.position, transform.forward);
    }

    public virtual void Merged()
    {
        isDead = true;
        hpBar.UPdateIsUsed(false);
        hpBar.SetActive(false);
        GameManager.Instance.effectManager.Play(EffectType.Explosion, transform.position, transform.forward);

        gameObject.SetActive(false);
    }
    
    public virtual void SetDead()
    {
        photonView.RPC("RPCSetDead", RpcTarget.AllBuffered);
    }

    [PunRPC]
    public virtual void RPCSetDead()
    {
        isDead = true;
        hpBar.UPdateIsUsed(false);
        hpBar.SetActive(false);
        GameManager.Instance.effectManager.Play(EffectType.Explosion, transform.position, transform.forward);

        gameObject.SetActive(false);
    }

    public virtual void OnDetectEnemy(CharacterBase target)
    {
        if (!DetectedEnemies.Contains(target.gameObject))
        {
            DetectedEnemies.Add(target.gameObject);
        }
    }

    public virtual void SetDestinationPos(Vector3 destination)
    {
        destinationPos = destination;
    }

    public virtual void SetDestination(Vector3 destination)
    {
        if(navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isActiveAndEnabled)
        {
            return;
        }

        navMeshAgent?.SetDestination(destination);
        navMeshAgent.isStopped = false;
        navMeshAgent.updatePosition = true;
        navMeshAgent.updateRotation = true;
    }

    public virtual void ResetPath()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !gameObject.activeSelf)
        {
            return;
        }
        navMeshAgent.ResetPath();
        navMeshAgent.isStopped = true;
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
    }

    public virtual void SetSpeed(float inSpeed)
    {
        if (!navMeshAgent.enabled)
        {
            return;
        }
        navMeshAgent.speed = inSpeed;
    }

    protected virtual GameObject GetTarget()
    {
        if (DetectedEnemies.Count > 0)
        {
            GameObject target = DetectedEnemies.Find(e => e != null && !e.GetComponent<CharacterBase>().isDead);
            if (target != null)
            {
                return target;
            }
        }

        return gameObject;
    }

    public virtual void AIActionByJob(GameObject target)
    {
        if (!gameObject.activeSelf) return;
        if (target == null)
        {
            SetDestination(destinationPos);
            return;
        }

        switch (characterType)
        {
            case CharacterType.ElPrimo:
            case CharacterType.Colt:
                if (target.TryGetComponent<CharacterBase>(out _) && !isAttacking)
                    MoveToEnemy(target);
                    //Attack(target);
                break;
            case CharacterType.Greg:
                if(target.TryGetComponent<MoneyTree>(out _) && !isAttacking)
                    MoveToEnemy(target);
                    //Attack(target);
                break;
            default:
                break;
        }
    }


    public virtual void AIAction(int moveAction)
    {
        float x = 0;
        float z = 0;

        switch(moveAction)
        {
            case 0: // forward 이동
                x = 0;
                z = 1;
                break;
            case 1: // back 이동
                x = 0;
                z = -1;
                break;
            case 2: // right 이동
                x = 1;
                z = 0;
                break;
            case 3: // left 이동
                x = -1;
                z = 0;
                break;
            case 4: // attack
                if (trainingManager == null) return;
                Attack(trainingManager.GetNearestEnemy());
                return;

            default:
                break;
        }
        navMeshAgent.enabled = false;
        Move(x, z);
    }

    protected virtual void Move(float x, float z)
    {
        movement3D.Move(x, z);
    }

    public virtual void MoveToTarget(GameObject target)
    {
        animator.SetFloat(AnimLocalize.moveSpeed, navMeshAgent.velocity.magnitude);

        if (target == gameObject)
        {
            StopAllCoroutines();
            animator.SetBool(AnimLocalize.contactEnemy, false);
            isAttacking = false;
            navMeshAgent.enabled = true;
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
        }
    }

    protected virtual void MoveToEnemy(GameObject target)
    {
        animator.SetFloat(AnimLocalize.moveSpeed, navMeshAgent.velocity.magnitude);

        if (target == gameObject)
        {
            StopAllCoroutines();
            animator.SetBool(AnimLocalize.contactEnemy, false);
            isAttacking = false;
            navMeshAgent.enabled = true;
            return;
        }

        if(isAttacking)
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

    protected virtual void Attack(GameObject target)
    {
        if (isAttacking) return;

        navMeshAgent.enabled = false;

        if (coroutineAttack != null) return;
        coroutineAttack = StartCoroutine(CoAttack(target));
    }

    protected virtual IEnumerator CoAttack(GameObject target) 
    {
        yield return null;
    }

    protected virtual void OnTargetDead(GameObject target)
    {
        if (trainingManager == null) return;
        trainingManager.OnEnemyKill();

        var enemies = GameManager.Instance.attackCircle.GetComponent<AttackCircle>().GetDetectedEnemies;
        if(enemies.Contains(target))
            GameManager.Instance.attackCircle.GetComponent<AttackCircle>().GetDetectedEnemies.Remove(target);

        if(coroutineAttack != null)
        {
            StopCoroutine(coroutineAttack);
            coroutineAttack = null;
        }
    }

    protected virtual void ForwardToEnemy(GameObject target, TweenCallback action = null)
    {
        Vector3 dirVec = target.transform.position - transform.position;
        float angle = Quaternion.FromToRotation(transform.forward, dirVec).eulerAngles.y;
        angle += Quaternion.FromToRotation(Vector3.forward, transform.forward).eulerAngles.y;
        dirVec = Vector3.up * angle;
        transform.DORotate(dirVec, 0.5f).OnComplete(action);
    }

    public virtual void OnUnDetectEnemy(CharacterBase target)
    {
        if(DetectedEnemies.Contains(target.gameObject))
        {
            DetectedEnemies.Remove(target.gameObject);
        }
    }

    public virtual void KnockBack(float inDamage, float inKnockBackTime, float inKnockBackDis)
    {
        photonView.RPC("RPCKnockBack", RpcTarget.AllBuffered, inDamage, inKnockBackTime, inKnockBackDis);
    }

    [PunRPC]
    public virtual void RPCKnockBack(float inDamage, float inKnockBackTime, float inKnockBackDis)
    {
        if (photonView.IsMine)
        {
            navMeshAgent.enabled = false;
            characterController.enabled = false;
            animator.SetTrigger(AnimLocalize.knockBack);

            Vector3 destination = transform.position - transform.forward.normalized * inKnockBackDis;
            Vector3[] path = { transform.position, destination };
            TakeDamage(inDamage);
            transform.DOPath(path, inKnockBackTime, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
            {
                navMeshAgent.enabled = true;
                characterController.enabled = true;
                characterState = CharacterState.Idle;
            });
        }
    }

    public virtual int GetCoin()
    {
        return characterStat.GetCoin();
    }

    public virtual int GetGem()
    {
        return characterStat.GetGem();
    }

    public virtual void GetAOE(float inDamage, Vector3 fromPos, float distance)
    {
        //SetCharacterState(CharacterState.Stun);
        ////characterState = CharacterState.Stun;
        //fromPos.y = transform.position.y;
        ////Debug.Log($"[{gameObject.name}] transform.position.y : {transform.position.y}");
        //Vector3 dir = transform.position - fromPos;
        //Vector3 startPos = transform.position;
        //Vector3 endPoint = startPos + dir.normalized * distance;
        //Vector3 midPoint = startPos + (endPoint - transform.position) * 0.5f;
        //midPoint.y += 2f;
        //endPoint.y = fromPos.y;
        ////Debug.Log($"[{gameObject.name}] endPoint.y : {endPoint.y}");
        //Vector3[] paths = { startPos, midPoint, endPoint };
        //transform.DOPath(paths, 0.5f, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
        //{
        //    TakeDamage(inDamage);
        //    //characterState = CharacterState.Idle;
        //    SetCharacterState(CharacterState.Idle);
        //    //Debug.Log($"[{gameObject.name}] y : {transform.position.y}");
        //});
        photonView.RPC("RPCGetAOE", RpcTarget.AllBuffered, inDamage, fromPos, distance);
    }

    [PunRPC]
    public virtual void RPCGetAOE(float inDamage, Vector3 fromPos, float distance)
    {
        //SetCharacterState(CharacterState.Stun);
        characterState = CharacterState.Stun;
        fromPos.y = transform.position.y;
        //Debug.Log($"[{gameObject.name}] transform.position.y : {transform.position.y}");
        Vector3 dir = transform.position - fromPos;
        Vector3 startPos = transform.position;
        Vector3 endPoint = startPos + dir.normalized * distance;
        Vector3 midPoint = startPos + (endPoint - transform.position) * 0.5f;
        midPoint.y += 2f;
        endPoint.y = fromPos.y;
        //Debug.Log($"[{gameObject.name}] endPoint.y : {endPoint.y}");
        Vector3[] paths = { startPos, midPoint, endPoint };
        transform.DOPath(paths, 0.5f, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
        {
            TakeDamage(inDamage);
            characterState = CharacterState.Idle;
            //SetCharacterState(CharacterState.Idle);
            //Debug.Log($"[{gameObject.name}] y : {transform.position.y}");
        });
    }

    public virtual CharacterLevel GetCharacterLevel()
    {
        return characterLevel;
    }

}
