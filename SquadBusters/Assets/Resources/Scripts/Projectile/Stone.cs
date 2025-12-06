using UnityEngine;
using DG.Tweening;

public class Stone : Projectile
{
    private GameObject host = null;
    public bool isThrowed { get; private set;} = false;

    protected override void Awake()
    {
        base.Awake();
        projectileType = ProjectileType.Stone;
    }

    private void OnEnable()
    {
        isThrowed = false;
    }

    private void Update()
    {
        if(host == null || isThrowed)
        {
            return;
        }

        transform.position = host.transform.position;
    }

    public void SetHost(GameObject newHost)
    {
        host = newHost; 
    }

    public virtual void Shot(Vector3 startPos, Vector3 destination, float shotTime)
    {
        //포물선 낙하 구현
        destination.y = 2.1f;
        Vector3 midPos = startPos + (destination - startPos) * 0.5f;
        midPos.y = startPos.y + 1f;
        Vector3[] path = { startPos, midPos, destination };
        isThrowed = true;
        transform.DOPath(path, shotTime, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
        {
            if (gameObject.activeSelf)
            {
                isThrowed = false;
                GameManager.Instance.aoeManager.GetAOE(transform.position, AOEType.Yellow, 0.5f);
                SetActive(false);
            }
        });
    }
}
