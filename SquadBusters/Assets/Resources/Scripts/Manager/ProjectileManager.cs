using DG.Tweening;
using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileType
{
    FireBullet,
    Bomb,
    Bullet,
    Cannon,
    Stone,

    End
}

public class ProjectileManager : MonoBehaviour
{
    private List<Projectile> projectiles = new List<Projectile>();

    public Projectile GetProjectile(Vector3 pos, ProjectileType type)
    {
        Projectile projectile = null;
        if(projectiles.Count > 0)
        {
            projectile = projectiles.Find(p => !p.gameObject.activeSelf && p.GetProjectileType() == type);
        }
        
        if (projectile == null)
        {
            string path = $"Prefabs/Projectile/{type.ToString()}";
            GameObject newBullet = PhotonNetwork.Instantiate(path, pos, Quaternion.identity);
            projectile = newBullet.GetComponent<Projectile>();
            projectile.transform.SetParent(transform);
            projectiles.Add(projectile);
        }

        DOTween.Kill(projectile.gameObject.transform);
        projectile.transform.position = pos;
        projectile.SetActive(true);

        return projectile;
    }
}
