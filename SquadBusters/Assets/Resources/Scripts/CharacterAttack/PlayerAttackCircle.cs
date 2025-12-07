using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static CharacterPlayer;
using static UnityEngine.UI.GridLayoutGroup;

public class PlayerAttackCircle : AttackCircle, IAttackCircleUIInterface, IAttackCircleItemInterface, IPlayerAttackCircleProjectileInterface
{
    protected GameObject moveObj;
    protected Movement3D movement3D;
    protected float commonSpeed = 7.5f;
    protected CharacterController characterController;
    Vector3 pos;

    [SerializeField] protected ParticleSystem blueCircleEffect;
    [SerializeField] protected ParticleSystem redCircleEffect;

    [SerializeField] protected TextMeshProUGUI userName;
    [SerializeField] protected TextMeshProUGUI gemCnt;
    protected CircleAgent agent;

    private bool isStunned = false;

    protected override void Awake()
    {
        base.Awake();
        moveObj = new GameObject($"Move{gameObject.name}");
        movement3D = moveObj.AddComponent<Movement3D>();
        characterController = moveObj.AddComponent<CharacterController>();
        type = circleType.Player;
        attackCircleStat.SetCoin(0);
        attackCircleStat.SetGem(0);

        redCircleEffect.gameObject.SetActive(false);
        blueCircleEffect.gameObject.SetActive(false);

        agent = GetComponent<CircleAgent>();
    }

    protected override void Start()
    {
        moveObj.transform.position = transform.position;

        if(photonView.IsMine)
        {
            GameManager.Instance.uiManager.fastMoveUI.onMoveFast -= () => movement3D.UpdateMoveSpeed(commonSpeed * 2f);
            GameManager.Instance.uiManager.fastMoveUI.onMoveFast += () => movement3D.UpdateMoveSpeed(commonSpeed * 2f);
            GameManager.Instance.uiManager.fastMoveUI.onMoveCommon -= () => movement3D.UpdateMoveSpeed(commonSpeed);
            GameManager.Instance.uiManager.fastMoveUI.onMoveCommon += () => movement3D.UpdateMoveSpeed(commonSpeed);
            GameManager.Instance.uiManager.skillUI.doSkill += DoItemSkill;

            photonView.RPC("SetUserName", RpcTarget.AllBuffered, GameManager.Instance.userName);
            photonView.RPC("UpdateGemCnt", RpcTarget.AllBuffered, attackCircleStat.GetGem());
        }
    }
    
    protected virtual void Update()
    {
        if (GameManager.Instance.endGame)
        {
            return;
        }

        if (!photonView.IsMine)
        {
            return;
        }

        SetCircleColor(CheckInput());

        pos = moveObj.transform.position;
        pos.y = 2.1f;

        if (CheckInput())
        {
            Move();
            transform.position = pos;
        }

        foreach (var owner in owners)
        {
            if(owner.gameObject.activeSelf)
            {
                //owner.SetDestination(moveObj.transform.position);
                owner.SetDestinationPos(pos);
            }            
        }
    }

    [PunRPC]
    public virtual void SetUserName(string username)
    {
        userName.SetText(username);
    }

    [PunRPC]
    public virtual void UpdateGemCnt(int cnt)
    {
        gemCnt.SetText(cnt.ToString());
    }

    public override void UpdateOwners(CharacterBase newOwner, bool isMerged)
    {
        base.UpdateOwners(newOwner, isMerged);

        if (owners.LastOrDefault() == newOwner)
        {
            if (newOwner.gameObject.TryGetComponent<CharacterPlayer>(out CharacterPlayer player))
            {
                OnTakeItem takeCoin = GainCoin;
                player.AddTakeItemActions(takeCoin);
                OnTakeItem takeGem = GainGem;
                player.AddTakeItemActions(takeGem);
                OnTakeItem takeBomb = GainBomb;
                player.AddTakeItemActions(takeBomb);
                OnTakeItem takeCannon = GainCannon;
                player.AddTakeItemActions(takeCannon);

                player.updateCoin = SetCoin;
                player.totalCoin = GetCoin;
            }

            if(!isMerged)
            {
                return;
            }

            //머지할 수 있는지 검사
            if(newOwner.GetCharacterLevel() == CharacterLevel.End - 1)
            {
                return;
            }
            List<CharacterBase> chars = owners.FindAll(o => o.gameObject.activeSelf &&
            o.GetCharacterType() == newOwner.GetCharacterType() &&
            o.GetCharacterLevel() == newOwner.GetCharacterLevel()).ToList();
            if (chars.Count < 3)
            {
                return;
            }

            StartCoroutine(CoMergeCharacter(chars, newOwner.transform.position));
        }
    }

    private IEnumerator CoMergeCharacter(List<CharacterBase> chars, Vector3 pos)
    {
        yield return new WaitForSeconds(0.3f);
        CharacterType type = chars.FirstOrDefault().GetCharacterType();
        CharacterLevel nextLevel = chars.FirstOrDefault().GetCharacterLevel() + 1;
        foreach (var ch in chars)
        {
            //ch.Merged();
            ch.SetDead();
        }
        
        CharacterBase character = SpawnCharacter(transform.position, type, nextLevel, false);
        NoticeElem notice = GameManager.Instance.uiManager.noticeUI.ShowAcitveNotice(NoticeType.Fusion, true, character.gameObject);
        notice.Disable(2f);

        owners.LastOrDefault().transform.position = pos;
    }

    protected virtual void Move()
    {
        
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        movement3D.Move(x, z);
    }

    protected virtual bool CheckInput()
    {
        //모바일에서 터치로 변경
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D))
        {
            characterController.enabled = true;
            return true;
        }

        characterController.enabled = false;
        return false;
    }

    protected virtual void SetCircleColor(bool isMoving)
    {
        redCircleEffect.gameObject.SetActive(!isMoving);
        blueCircleEffect.gameObject.SetActive(isMoving);
    }

    public void GainCoin()
    {
        if(photonView.IsMine)
        {
            GameManager.Instance.soundManager.Play(SoundEffectType.GainItem);
            attackCircleStat.SetCoin(attackCircleStat.GetCoin() + 1);
            GameManager.Instance.uiManager.coinUI.SetCoin(attackCircleStat.GetCoin());

            agent.AddReward(RewardConstant.GetCoinScore);
        }
    }

    public void GainGem()
    {
        if (photonView.IsMine)
        {
            GameManager.Instance.soundManager.Play(SoundEffectType.GainItem);
            attackCircleStat.SetGem(attackCircleStat.GetGem() + 1);
            GameManager.Instance.UpdateRank(GameManager.Instance.userName, attackCircleStat.GetGem());
            photonView.RPC("UpdateGemCnt", RpcTarget.AllBuffered, attackCircleStat.GetGem());

            agent.AddReward(RewardConstant.GetItemScore);
        }
    }

    public void GainBomb()
    {
        GameManager.Instance.soundManager.Play(SoundEffectType.GainItem);
        GameManager.Instance.uiManager.skillUI.UpdateSkillType(ItemType.Bomb);
        GameManager.Instance.uiManager.skillUI.SetInteractable(true);
        agent.AddReward(RewardConstant.GetItemScore);
    }

    public void GainCannon()
    {
        GameManager.Instance.soundManager.Play(SoundEffectType.GainItem);
        GameManager.Instance.uiManager.skillUI.UpdateSkillType(ItemType.Cannon);
        GameManager.Instance.uiManager.skillUI.SetInteractable(true);
        agent.AddReward(RewardConstant.GetItemScore);
    }

    public int GetCoin()
    {
        return attackCircleStat.GetCoin();
    }

    public void SetCoin(int newCoin)
    {
        attackCircleStat.SetCoin(newCoin);
        GameManager.Instance.uiManager.coinUI.SetCoin(newCoin);
    }

    public virtual void GainTreasureBox()
    {
        GameManager.Instance.soundManager.Play(SoundEffectType.GainItem);
        GameManager.Instance.uiManager.ShowUI(UIType.SelectCharacter, true);
    }

    public void OnDetectedItem(NoticeType type, Item tree)
    {
        var gregs = owners.FindAll(o => o.GetCharacterType().Equals(CharacterType.Greg));
        if(gregs.Count == 0)
        {
            if(photonView.IsMine)
            {
                ShowNotice(type, tree);
            }
            
            return;
        }

        gregs.ForEach(g => g.GetComponent<Greg>().OnDetectedMoneyTree(tree));
    }

    public void OnUnDetectedItem(Item tree)
    {
        var gregs = owners.FindAll(o => o.GetCharacterType().Equals(CharacterType.Greg));
        if (gregs.Count == 0)
        {
            return;
        }

        gregs.ForEach(g => g.GetComponent<Greg>().OnUnDetectedMoneyTree(tree));

    }

    public virtual void Stun(float duration, string animName)
    {
        if (!photonView.IsMine)
        {
            return;
        }

        if (isStunned)
        {
            return;
        }

        StartCoroutine(CoStun(duration, animName));
    }

    protected IEnumerator CoStun(float duration, string animName)
    {
        isStunned = true;
        owners.ForEach(o => o.SetCharacterState(CharacterState.Stun));
        owners.ForEach(o => o.PlayStunAnimation());
        movement3D.UpdateMoveSpeed(commonSpeed * 0.1f);
        GameManager.Instance.uiManager.fastMoveUI.SetInteractable(false);
        yield return new WaitForSeconds(duration);
        owners.ForEach(o => o.SetCharacterState(CharacterState.Idle));
        movement3D.UpdateMoveSpeed(commonSpeed);
        isStunned = false;
        GameManager.Instance.uiManager.fastMoveUI.SetInteractable(true);
    }

    public void ShowNotice(NoticeType type, Item item)
    {
        NoticeElem noticeElem = GameManager.Instance.uiManager.noticeUI.ShowAcitveNotice(type, true, item.gameObject);
        item.onUndetectedPlayerAttack -= delegate { noticeElem.SetActive(false); };
        item.onUndetectedPlayerAttack += delegate { noticeElem.SetActive(false); };
    }

    #region IAttackCircleUIInterface
    public void SelectCharacter(CharacterType newType, CharacterLevel newLevel)
    {
        Vector3 pos = Vector3.zero;
        float x = Random.Range(-attackCircleStat.attackRadius + 2, attackCircleStat.attackRadius - 2);
        float z = Random.Range(0, Mathf.Pow(attackCircleStat.attackRadius, 2) - Mathf.Pow(x, 2));
        pos.x = x + transform.position.x;
        pos.z = Random.Range(-Mathf.Sqrt(z) + 2, Mathf.Sqrt(z) - 2) + transform.position.z;
        CharacterBase player = SpawnCharacter(pos, newType, newLevel, true);

        if (photonView.IsMine)
        {
            redCircleEffect.gameObject.SetActive(true);
        }
    }

    public void DoItemSkill(ItemType itemType)
    {
        Projectile projectile = null;
        Vector3 pos = transform.position;
        switch (itemType)
        {
            case ItemType.Bomb:
                pos.y = 2.1f;
                projectile = GameManager.Instance.projectileManager.GetProjectile(pos, ProjectileType.Bomb);
                Bomb bomb = projectile.gameObject.GetComponent<Bomb>();
                bomb.Explode(2f);
                break;
            case ItemType.Cannon:
                pos.y = 2.1f;
                projectile = GameManager.Instance.projectileManager.GetProjectile(transform.position, ProjectileType.Cannon);
                Cannon cannon = projectile.gameObject.GetComponent<Cannon>();
                StartCoroutine(CoDisableCannon(cannon.lifeTime, cannon));
                if (owners.Count > 0)
                {
                    owners.ForEach(o => cannon.SetHost(o.gameObject));
                }
                break;
            default:
                break;
        }
    }

    private IEnumerator CoDisableCannon(float lifeTime, Cannon cannon)
    {
        yield return new WaitForSeconds(lifeTime);
        cannon.SetActive(false);
        photonView.RPC("RPCEffect", RpcTarget.AllBuffered, (int)EffectType.StonesHit, transform.position, transform.forward);
    }

    [PunRPC]
    public void RPCEffect(int effectType, Vector3 pos, Vector3 rot)
    {
        GameManager.Instance.effectManager.Play((EffectType)effectType, pos, rot);
    }
    #endregion

    
}
