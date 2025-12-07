using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [SerializeField] Agent agent;

    public Vector3 GetNearestEnemyPosition()
    {
        var nearestPos = Vector3.zero;
        if (GameManager.Instance.EnemiesInFullRange.Count == 0)
        {
            return gameObject.transform.position;
        }

        GameManager.Instance.EnemiesInFullRange = GameManager.Instance.EnemiesInFullRange.OrderBy(e => 
            Vector3.Distance(gameObject.transform.position, e.transform.position)).ToList();
        nearestPos.x = GameManager.Instance.EnemiesInFullRange.FirstOrDefault().transform.position.x;
        nearestPos.z = GameManager.Instance.EnemiesInFullRange.FirstOrDefault().transform.position.z;
        return nearestPos;
    }

    /// <summary>
    /// 가장 가까운 적 찾기
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearestEnemy()
    {
        //return GameManager.Instance.EnemiesInFullRange.FirstOrDefault();
        if(GameManager.Instance.attackCircle.TryGetComponent<AttackCircle>(out AttackCircle attackCircle))
        {
            attackCircle.GetDetectedEnemies = attackCircle.GetDetectedEnemies.OrderBy(e =>
            Vector3.Distance(gameObject.transform.position, e.transform.position)).ToList();
            return attackCircle.GetDetectedEnemies.FirstOrDefault();
        }
        return null;
    }

    /// <summary>
    /// 가장 체력 낮은 적 찾기
    /// </summary>
    /// <returns></returns>
    public GameObject GetWeakestEnemy()
    {
        if (GameManager.Instance.attackCircle.TryGetComponent<AttackCircle>(out AttackCircle attackCircle))
        {
            var enemies = attackCircle.GetDetectedEnemies.Where(e => e.TryGetComponent<CharacterStat>(out _))
                .Select(e => e.GetComponent<CharacterStat>())
                .OrderBy(e => e.GetCurrentHp())
                .ToList();
            return enemies.FirstOrDefault() == null ? null : enemies.FirstOrDefault().gameObject;
        }
        return null;
    }

    /// <summary>
    /// 가장 가까운 아군 찾기
    /// </summary>
    /// <returns></returns>
    public GameObject GetNearestFriendly()
    {
        if (GameManager.Instance.attackCircle.TryGetComponent<AttackCircle>(out AttackCircle attackCircle))
        {
            var owners = attackCircle.GetOwners
                .OrderBy(e => Vector3.Distance(gameObject.transform.position, e.gameObject.transform.position))
                .ToList();
            return owners.Count == 0 ? null : owners.FirstOrDefault().gameObject;
        }
        return null;
    }

    /// <summary>
    /// 가장 체력 낮은 아군 찾기
    /// </summary>
    /// <returns></returns>
    public GameObject GetWeakestFriendly()
    {
        if (GameManager.Instance.attackCircle.TryGetComponent<AttackCircle>(out AttackCircle attackCircle))
        {
            var owners = attackCircle.GetOwners.Where(e => e.TryGetComponent<CharacterStat>(out _))
                .Select(e => e.GetComponent<CharacterStat>())
                .OrderBy(e => e.GetCurrentHp())
                .ToList();
            return owners.Count == 0 ? null : owners.FirstOrDefault().gameObject;
        }
        return null;
    }

    /// <summary>
    /// 가장 가까운 아이템 찾기
    /// </summary>
    /// <returns></returns>
    public Item GetNearestItem(bool coin)
    {
        List<Item> items = new List<Item>();
        if (GameManager.Instance.attackCircle.TryGetComponent<AttackCircle>(out AttackCircle attackCircle))
        {
            if(coin)
            {
                items = attackCircle.GetDetectedItems
                .Where(e => e.GetItemType() == ItemType.Coin)
                .OrderBy(e => Vector3.Distance(gameObject.transform.position, e.gameObject.transform.position))
                .ToList();
            }
            else
            {
                items = attackCircle.GetDetectedItems
                .Where(e => e.GetItemType() == ItemType.Gem || e.GetItemType() == ItemType.Bomb || e.GetItemType() == ItemType.Cannon)
                .OrderBy(e => Vector3.Distance(gameObject.transform.position, e.gameObject.transform.position))
                .ToList();
            }
            
            return items.Count == 0 ? null : items.FirstOrDefault();
        }
        return null;
    }

    public void ResetEnemyFollowed()
    {
        // 적(Enemy) 위치 리셋 (나랑 좀 떨어진 곳 랜덤)
        float randX = Random.Range(-8f, 8f);
        float randZ = Random.Range(-8f, 8f);

        // 적 오브젝트 찾아서 이동 (TrainingManager 등을 통해 가져오세요)
        if (GameManager.Instance.enemyFollowed != null)
        {
            GameManager.Instance.enemyFollowed.transform.localPosition = agent.transform.position + new Vector3(randX, 0, randZ);
            // 적 체력 100으로 리셋
            GameManager.Instance.enemyFollowed.Init();
        }
    }

    public void OnEnemyKill()
    {
        agent.AddReward(RewardConstant.KillEnemyScore);
        agent.EndEpisode();
    }

    public void OnUnitDead()
    {
        agent.AddReward(RewardConstant.DeadUnit);
        agent.EndEpisode();
    }
}
