using DG.Tweening;
using Photon.Pun;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    protected PhotonView photonView = null;
    protected ProjectileType projectileType;
    protected float damage = 0;

    protected virtual void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public ProjectileType GetProjectileType()
    {
        return projectileType; 
    }

    public virtual void SetDamage(float inDamage)
    {
        damage = inDamage;  
    }

    public virtual void SetDirection(Vector3 dir)
    {
        gameObject.transform.rotation = Quaternion.LookRotation(dir);
    }

    public virtual void Shot(Vector3 destination, float shotTime)
    {
        transform.DOMove(destination, shotTime).OnComplete(() =>
        {
            if (gameObject.activeSelf)
            {
                SetActive(false);
            }
        });
    }

    public void SetActive(bool isActive)
    {
        photonView.RPC("RPCSetActive", RpcTarget.AllBuffered, isActive);
    }

    [PunRPC]
    public void RPCSetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
