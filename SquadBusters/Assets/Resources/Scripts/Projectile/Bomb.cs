using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Bomb : Projectile
{
    protected override void Awake()
    {
        base.Awake();
        projectileType = ProjectileType.Bomb;
    }

    public void Explode(float waiting)
    {
        Vector3 startPos = transform.position + Vector3.up;
        Vector3 endPos = transform.position;
        Vector3 midPos = transform.position;
        midPos.y += 2f;
        Vector3[] path = { startPos, midPos, endPos };
        transform.DOPath(path, 0.5f, PathType.CatmullRom, PathMode.Full3D).OnComplete(() =>
        {
            StartCoroutine(CoExplode(waiting));
        });
    }

    private IEnumerator CoExplode(float waiting)
    {
        yield return new WaitForSeconds(waiting);
        SetActive(false);

        AOE aoe = GameManager.Instance.aoeManager.GetAOE(transform.position, AOEType.Red, 0.7f);
        GameManager.Instance.effectManager.Play(EffectType.Explosion, transform.position, transform.forward);
    }
}
