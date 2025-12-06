using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackCircle : MonoBehaviour
{
    public enum circleType
    { 
        None = 0,

        Player,
        NPC,

        End,
    }


    protected List<CharacterBase> owners = new List<CharacterBase>();
    protected AttackCircleStat attackCircleStat = null;

    protected List<GameObject> DetectedEnemies = new List<GameObject>();
    protected List<Item> DetectedItems = new List<Item>();

    public bool isUsed { get; private set; } = false;
    [HideInInspector] public circleType type = circleType.None;

    public delegate void DetectEnemy(CharacterBase target);
    public DetectEnemy onDetectEnemy;
    public delegate void UnDetectEnemy(CharacterBase target);
    public UnDetectEnemy onUnDetectEnemy;

    private SphereCollider sphereCollider = null;
    protected PhotonView photonView = null;

    private string postFixLayer = "AttackCircle";

    public List<CharacterBase> GetOwners => owners;
    public List<GameObject> GetDetectedEnemies { get { return DetectedEnemies; } set { DetectedEnemies = value; } }
    public List<Item> GetDetectedItems { get { return DetectedItems; } set { DetectedItems = value; } }

    protected virtual void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        attackCircleStat = GetComponent<AttackCircleStat>();
        photonView = GetComponent<PhotonView>();
        UpdateRadius(attackCircleStat.attackRadius);
    }

    protected virtual void Start()
    {
        if (owners.Count == 0)
        {
            return;
        }

        transform.position = owners.FirstOrDefault().transform.position;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void UpdateIsUsed(bool used)
    {
        isUsed = used;
    }

    public virtual void UpdateOwners(CharacterBase newOwner, bool isMerged)
    {
        if(!owners.Contains(newOwner))
        {
            owners.Add(newOwner);
            onDetectEnemy -= newOwner.OnDetectEnemy;
            onDetectEnemy += newOwner.OnDetectEnemy;
            onUnDetectEnemy -= newOwner.OnUnDetectEnemy;
            onUnDetectEnemy += newOwner.OnUnDetectEnemy;
            newOwner.deadAction -= RemoveOwner;
            newOwner.deadAction += RemoveOwner;

        }
    }

    public void RemoveOwner(CharacterBase inOwner)
    {
        if (owners.Contains(inOwner))
        {
            owners.Remove(inOwner);
        }
        
        if (owners.Count == 0)
        {
            SetDead();
        }
    }

    private void SetDead()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.itemManager.ShowItem(attackCircleStat.GetCoin(), transform.position, ItemType.Coin);
            GameManager.Instance.itemManager.ShowItem(attackCircleStat.GetGem(), transform.position, ItemType.Gem);
        }
        
        UpdateIsUsed(false);
        SetActive(false);

    }

    public void UpdateRadius(float newRadius)
    {
        transform.localScale = Vector3.one * newRadius * 2; // 콜라이더의 반지름이 아닌 전체 구 오브젝트의 지름이라서 *2
    }

    public CharacterBase SpawnCharacter(Vector3 pos, CharacterType charType, CharacterLevel level = CharacterLevel.NPC, bool isMerged = false)
    {
        string path = $"Prefabs/Character/{charType.ToString()}{level.ToString()}";
        GameObject character = PhotonNetwork.Instantiate(path, pos, Quaternion.identity);
        CharacterBase characterBase = character.GetComponent<CharacterBase>();
        characterBase.gameObject.name += "mine";
        UpdateOwners(characterBase, isMerged);
        return characterBase;
    }

    public virtual void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<CharacterBase>(out CharacterBase character))
        {
            if (owners.Contains(character))
            {
                return;
            }

            if (onDetectEnemy == null)
            {
                return;
            }
            onDetectEnemy.Invoke(character);
            if(!DetectedEnemies.Contains(character.gameObject))
            {
                DetectedEnemies.Add(character.gameObject);
            }
        }

        if (other.gameObject.TryGetComponent<MoneyTree>(out MoneyTree moneyTree))
        {
            if (!DetectedEnemies.Contains(moneyTree.gameObject))
            {
                DetectedEnemies.Add(moneyTree.gameObject);
            }
        }

        //감지된 아이템 저장
        if(other.gameObject.TryGetComponent<Item>(out Item item))
        {
            if (!DetectedItems.Contains(item))
            {
                DetectedItems.Add(item);
            }
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<CharacterBase>(out CharacterBase character))
        {
            if (onUnDetectEnemy == null)
            {
                return;
            }
            onUnDetectEnemy.Invoke(character);
            if (DetectedEnemies.Contains(character.gameObject))
            {
                DetectedEnemies.Remove(character.gameObject);
            }
        }

        if (other.gameObject.TryGetComponent<MoneyTree>(out MoneyTree moneyTree))
        {
            if (DetectedEnemies.Contains(moneyTree.gameObject))
            {
                DetectedEnemies.Remove(moneyTree.gameObject);
            }
        }

        //감지된 아이템 정보
        if (other.gameObject.TryGetComponent<Item>(out Item item))
        {
            if (DetectedItems.Contains(item))
            {
                DetectedItems.Remove(item);
            }
        }
    }
}
