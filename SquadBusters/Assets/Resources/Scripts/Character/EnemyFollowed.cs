using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollowed : CharacterNonPlayer
{
    [SerializeField] NavMeshAgent navAgent;
    CharacterPlayer target;

    public void Start()
    {
        GameManager.Instance.enemyFollowed = this;
    }

    protected override void Update()
    {
        if (target == null) return;
        navAgent.SetDestination(target.transform.position);
        Attack(target.gameObject);
    }
}
