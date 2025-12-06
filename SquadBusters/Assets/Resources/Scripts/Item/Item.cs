using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Item : MonoBehaviour
{
    protected PhotonView photonView = null;
    protected ItemType type = ItemType.Coin;

    public delegate void OnUndetectedPlayerAttackCircle();
    public OnUndetectedPlayerAttackCircle onUndetectedPlayerAttack = null;

    protected bool isPicked = false;

    protected virtual void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public virtual void SetActive(bool isActive)
    {
        photonView.RPC("RPCSetActive", RpcTarget.AllBuffered, isActive);
    }

    [PunRPC]
    public virtual void RPCSetActive(bool isActive)
    {
        isPicked = !isActive;
        GetComponent<Collider>().enabled = isActive;
        gameObject.SetActive(isActive);
    }

    public virtual void SetPosition(Vector3 newPos)
    {
        photonView.RPC("RPCSetPosition", RpcTarget.AllBuffered, newPos);
    }

    [PunRPC]
    public virtual void RPCSetPosition(Vector3 newPos)
    {
        gameObject.transform.position = newPos;
    }

    public ItemType GetItemType()
    {
        return type;
    }

    public void UpdateItemType(ItemType newType)
    {
        type = newType;
    }

    [PunRPC]
    public void SetIsPicked(bool picked)
    {
        isPicked = picked;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if(!photonView.IsMine)
        {
            return;
        }

        if (isPicked)
        {
            return;
        }

        if (other.gameObject.TryGetComponent<ICharacterPlayerItemInterface>(out ICharacterPlayerItemInterface attackCircleItemInterface))
        {
            photonView.RPC("SetIsPicked", RpcTarget.AllBuffered, true);
            transform.DOMove(other.gameObject.transform.position + Vector3.up * 1.2f, 0.25f).OnComplete(() =>
            {
                Debug.Log($"[{gameObject.name}] {type.ToString()} {other.gameObject.name}");
                attackCircleItemInterface.TakeItem(type);

                SetActive(false);
            });
        }
    }
}
