using Photon.Pun;
using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    protected float repeatInterval = 10f;
    protected string path = null;
    protected GameObject spawnObject = null;
    protected PhotonView photonView = null;

    protected virtual void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    protected virtual void Start()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            StartSpawn();
        }
    }

    protected virtual void SetPath(string inPath)
    {
        path = inPath;
    }

    public virtual void StartSpawn()
    {
        StartCoroutine(CoStartSpawn());
    }

    private IEnumerator CoStartSpawn()
    {
        yield return new WaitUntil(() => GameManager.Instance.effectManager != null);
        InvokeRepeating("Spawn", 0.0f, repeatInterval);
    }

    protected virtual GameObject Spawn()
    {
        if (spawnObject == null || !spawnObject.activeSelf)
        {
            photonView.RPC("RPCEffect", RpcTarget.AllBuffered);

            GameObject obj = PhotonNetwork.Instantiate(path, transform.position, Quaternion.identity);

            GameManager.Instance.EnemiesInFullRange.Add(obj);

            return spawnObject = obj;
        }

        return null;
    }

    [PunRPC]
    protected virtual void RPCEffect()
    {
        StartCoroutine(CoEffect());
    }

    protected IEnumerator CoEffect()
    {
        yield return new WaitUntil(() => GameManager.Instance.effectManager != null);
        GameManager.Instance.effectManager.Play(EffectType.StarAura, transform.position, transform.forward);
    }
}
