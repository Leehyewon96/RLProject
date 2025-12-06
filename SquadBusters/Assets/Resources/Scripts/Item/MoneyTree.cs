using Photon.Pun;
using UnityEngine;

public class MoneyTree : Item
{
    private float hp = 300f; 
    private int coin = 5;
    private int gem = 2;
    public bool isDead { get; private set; } = false;

    

    protected override void Awake()
    {
        base.Awake();
        type = ItemType.MoneyTree;
    }

    public void TakeDamage(float inDamage)
    {
        hp -= inDamage;
        photonView.RPC("RPCSetEffect", RpcTarget.AllBuffered, (int)EffectType.StonesHit);

        if(hp <= 0)
        {
            hp = 0;
            isDead = true;
            SetDead();
        }
    }

    [PunRPC]
    public void RPCSetEffect(int effectType)
    {
        GameManager.Instance.effectManager.Play((EffectType)effectType, transform.position, transform.forward);
    }
    

    private void SetDead()
    {
        GameManager.Instance.itemManager.ShowItem(coin, transform.position, ItemType.Coin);
        GameManager.Instance.itemManager.ShowItem(gem, transform.position, ItemType.Gem);
        photonView.RPC("RPCSetEffect", RpcTarget.AllBuffered, (int)EffectType.Explosion);
        SetActive(false);
    }

    protected override void OnTriggerEnter(Collider other)
    {
        if (isPicked)
        {
            return;
        }

        if (other.gameObject.TryGetComponent<IAttackCircleItemInterface>(out IAttackCircleItemInterface circleItemInterface))
        {
            circleItemInterface.OnDetectedItem(NoticeType.MoneyTree, this);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent<IAttackCircleItemInterface>(out IAttackCircleItemInterface circleItemInterface))
        {
            if(onUndetectedPlayerAttack != null)
            {
                onUndetectedPlayerAttack.Invoke();
                onUndetectedPlayerAttack = null;
            }
            circleItemInterface.OnUnDetectedItem(this);
        }
    }
}
