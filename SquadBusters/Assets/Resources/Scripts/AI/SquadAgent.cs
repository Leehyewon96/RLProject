using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class SquadAgent : Agent
{
    [SerializeField] string behaviourName;
    CharacterBase character;
    CharacterStat characterStat;
    TrainingManager trainingManager;

    private float maxDistance = 10f;

    private void Awake()
    {
        character = GetComponent<CharacterBase>();
        characterStat = GetComponent<CharacterStat>();
        trainingManager = GetComponent<TrainingManager>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(characterStat.GetCurrentHp() / characterStat.GetMaxHp()); // 체력
        sensor.AddObservation(character.transform.position); // 위치

        if (GameManager.Instance.attackCircle == null) return;
        if (!GameManager.Instance.attackCircle.TryGetComponent<PlayerAttackCircle>(out PlayerAttackCircle attackCircle)) return;

        //아군들의 정보 저장
        for (int i = 0; i < GameManager.MAX_UNITS; i++)
        {
            var units = attackCircle.GetOwners;

            if (i < units.Count)
            {
                var unit = units[i];
                if (units[i] == gameObject) return;
                sensor.AddObservation(1f); // 슬롯 활성화 여부
                sensor.AddObservation((int)unit.GetCharacterType()); // 유닛 종류
                sensor.AddObservation((int)unit.GetCharacterLevel()); // 유닛 레벨

                if (unit.TryGetComponent<CharacterStat>(out CharacterStat characterStat))
                {
                    sensor.AddObservation(characterStat.GetCurrentHp() / characterStat.GetMaxHp()); //현재 체력 (정규화)
                }
            }
            else
            {
                sensor.AddObservation(0f); // 슬롯 활성화 여부
                sensor.AddObservation(0f); // 유닛 종류
                sensor.AddObservation(0f); // 유닛 레벨
                sensor.AddObservation(0f); //현재 체력 (정규화)
            }
        }


        //감지된 적들의 정보 저장
        for (int i = 0; i < GameManager.MAX_ENEMIES; i++)
        {
            var enemies = attackCircle.GetDetectedEnemies;

            if (i < enemies.Count)
            {
                var enemy = enemies[i] != null ? enemies[i].GetComponent<CharacterBase>() : null;
                if (enemy == null) return;

                sensor.AddObservation(1f); // 슬롯 활성화 여부
                sensor.AddObservation(enemy.transform.position - gameObject.transform.position); // 적 캐릭터 상대위치
                sensor.AddObservation((int)enemy.GetCharacterType()); // 적 캐릭터 종류
                sensor.AddObservation((int)enemy.GetCharacterLevel()); // 적 캐릭터 레벨

                if (enemy.TryGetComponent<CharacterStat>(out CharacterStat characterStat))
                {
                    sensor.AddObservation(characterStat.GetCurrentHp() / characterStat.GetMaxHp()); //현재 체력 (정규화)
                }
            }
            else
            {
                sensor.AddObservation(0f); // 슬롯 활성화 여부
                sensor.AddObservation(0f); // 적 캐릭터 종류
                sensor.AddObservation(0f); // 적 캐릭터 레벨
                sensor.AddObservation(0f); //현재 체력 (정규화)
            }
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Wall"))
        {
            AddReward(RewardConstant.CrashScore);
        }

        if (hit.gameObject.layer == LayerMask.NameToLayer("Coin"))
        {
            AddReward(RewardConstant.GetCoinScore);
        }

        if (hit.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            AddReward(RewardConstant.GetItemScore);
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        GameObject target = null;
        
        switch (action)
        {
            case 0: // 가장 가까운 적 공격
                target = trainingManager.GetNearestEnemy();
                character.AIActionByJob(target);
                break;
            case 1: // 가장 체력 없는 적 공격
                target = trainingManager.GetWeakestEnemy(); 
                character.AIActionByJob(target);
                break;
            case 2: // 가장 가까운 아군에게 이동
                target = trainingManager.GetNearestFriendly();
                if(target != null)
                    character.MoveToTarget(target);
                break;
            case 3: // 가장 체력 없는 아군에게 이동
                target = trainingManager.GetWeakestFriendly();
                if (target != null)
                    character.MoveToTarget(target);
                break;
        }


        
    }
}
