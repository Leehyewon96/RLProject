using Photon.Pun;
using System.Collections;
using UnityEngine;

public class AOE : MonoBehaviour
{
    private float lifeTime = 3f;
    private float damage = 10f;

    private PhotonView photonView = null;
    private ParticleSystem particle = null;
    private ParticleSystem.MainModule particleMain;
    [SerializeField] private AOEType aoeType;
    private CapsuleCollider capsuleCollider = null;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        particle = GetComponentInChildren<ParticleSystem>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        particleMain = particle.main;
    }

    private void OnEnable()
    {
        GameManager.Instance.soundManager.Play(SoundEffectType.Explose);
        SetEnable(true);
        StartCoroutine(CoSetActive(false));
        StartCoroutine(CoSetEnable(false));
    }

    private IEnumerator CoSetActive(bool isActive)
    {
        yield return new WaitForSeconds(lifeTime);
        SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<ICharacterProjectileInterface>(out ICharacterProjectileInterface player))
        {
            player.GetAOE(damage, transform.position, 5f);
        }
    }

    private IEnumerator CoSetEnable(bool isEnabled)
    {
        yield return new WaitForSeconds(0.1f);
        SetEnable(false);
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

    public AOEType GetAoeType()
    {
        return aoeType;
    }

    private void SetEnable(bool isEnabled)
    {
        photonView.RPC("RPCSetEnable", RpcTarget.AllBuffered, isEnabled);
    }

    [PunRPC]
    public void RPCSetEnable(bool isEnabled)
    {
        capsuleCollider.enabled = isEnabled;
    }
}
