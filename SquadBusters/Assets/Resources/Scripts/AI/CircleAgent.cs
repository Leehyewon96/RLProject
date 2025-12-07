using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public static class RewardConstant
{
    public const float CrashScore = -0.01f;
    public const float KillEnemyScore = 0.1f;
    public const float DeadUnit = -0.1f;
    public const float GetCoinScore = 0.01f;
    public const float GetItemScore = 0.01f;
}

public class CircleAgent : Agent
{
    [SerializeField] string behaviourName;
    [SerializeField] TrainingManager trainingManager;

    StatsRecorder statsRecorder;
    PlayerAttackCircle circle;

    // 속도 계산을 위한 변수
    private Vector3 lastPosition;
    private Vector3 currentVelocity;

    float maxDistance = 10f;
    int prevAction = -1;

    private void Awake()
    {
        circle = GetComponent<PlayerAttackCircle>();
        statsRecorder = Academy.Instance.StatsRecorder;
        lastPosition = transform.position;
    }

    void FixedUpdate()
    {
        //직접 속도 계산 (이동 거리를 시간으로 나눔)
        // (현재위치 - 이전위치) / 걸린시간 = 속도
        currentVelocity = (transform.position - lastPosition) / Time.fixedDeltaTime;

        // 현재 위치를 이전 위치로 저장 (다음 프레임을 위해)
        lastPosition = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // 에이전트(나) 위치 리셋 (중앙 근처 랜덤)
        circle.MoveObj.transform.position += new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
        trainingManager.ResetEnemyFollowed();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 내 자신의 위치와 속도 정보
        // 내 위치 (3개): 맵의 중심이나 벽을 인지하기 위해 필요
        sensor.AddObservation(transform.localPosition);

        // 내 속도 (3개): 내가 지금 움직이고 있는지 알아야 멈출지 가속할지 결정함
        sensor.AddObservation(currentVelocity);

        if (GameManager.Instance.attackCircle == null) return;
        if (!GameManager.Instance.attackCircle.TryGetComponent<PlayerAttackCircle>(out PlayerAttackCircle attackCircle)) return;

        // 유닛 정보
        for (int i = 0; i < GameManager.MAX_UNITS; i++)
        {
            var units = attackCircle.GetOwners;

            if (i < units.Count)
            {
                var unit = units[i];
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

        //적 정보
        List<GameObject> enemies = new List<GameObject>(attackCircle.GetDetectedEnemies);
        var chaser = GameManager.Instance.enemyFollowed;

        // 추격자가 살아있는데 리스트에 없다면? -> 강제로 0번(최우선) 자리에 끼워 넣기
        if (chaser != null && chaser.gameObject.activeSelf && !chaser.isDead)
        {
            if (!enemies.Contains(chaser.gameObject))
            {
                enemies.Insert(0, chaser.gameObject);
            }
        }

        for (int i = 0; i < GameManager.MAX_ENEMIES; i++)
        {
            //var enemies = attackCircle.GetDetectedEnemies;

            // 적이 감지 범위 안에 있고 인덱스가 유효할 때
            if (i < enemies.Count && enemies[i] != null)
            {
                if (!enemies[i].TryGetComponent<CharacterBase>(out CharacterBase enemy))
                {
                    return;
                }

                sensor.AddObservation(1f); // [1개] 슬롯 활성화

                // [3개] 적 위치 (Vector3는 float 3개 취급)
                sensor.AddObservation(enemy.transform.position - transform.position);

                sensor.AddObservation((int)enemy.GetCharacterType()); // [1개] 종류
                sensor.AddObservation((int)enemy.GetCharacterLevel()); // [1개] 레벨

                if (enemy.TryGetComponent<CharacterStat>(out CharacterStat characterStat))
                    sensor.AddObservation(characterStat.GetCurrentHp() / characterStat.GetMaxHp()); // [1개] 체력
                else
                    sensor.AddObservation(0f);
            }
            else
            {
                //위에서 7개를 넣어서 여기도 7개를 넣어야 함
                sensor.AddObservation(0f); // [1개] 슬롯 활성화
                sensor.AddObservation(Vector3.zero); // [3개] 위치 정보 빈값
                sensor.AddObservation(0f); // [1개] 종류
                sensor.AddObservation(0f); // [1개] 레벨
                sensor.AddObservation(0f); // [1개] 체력
            }
        }
    }
    #region OnControllerColliderHit
    /*private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.01f);
        }

        if (hit.gameObject.CompareTag("Coin"))
        {
            AddReward(0.01f);
        }
    }*/
    #endregion

    public override void OnActionReceived(ActionBuffers actions)
    {
        //시간이 흐를수록 점수 깎임
        AddReward(-0.001f);

        //이동
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 moveDir = new Vector3(moveX, 0, moveZ).normalized;

        circle.MoveObj.transform.position += moveDir * 5f * Time.deltaTime;

        // 회전 (이동하는 방향 바라보기)
        if (moveDir != Vector3.zero)
        {
            circle.MoveObj.transform.forward = Vector3.Slerp(transform.forward, moveDir, 10f * Time.deltaTime);
        }

        // 인덱스 2번 값 (-1.0 ~ 1.0 사이의 값이 들어옴)
        float strategySignal = actions.ContinuousActions[2];

        // 값이 0보다 크면 1번 전략, 작으면 0번 전략
        int action = strategySignal > 0 ? 1 : 0;
        GameManager.Instance.ChangeActionToMove = (action == 0 && prevAction == 1);
        if (action != prevAction)
        {
            prevAction = action;
        }

        //공격
        if (GameManager.Instance.attackCircle == null) return;
        if (!GameManager.Instance.attackCircle.TryGetComponent<PlayerAttackCircle>(out PlayerAttackCircle attackCircle)) return;
        var units = attackCircle.GetOwners;

        //int action = actions.DiscreteActions[0];
        if (trainingManager == null) return;
        GameObject target = null;
        Item item = null;

        statsRecorder.Add("AI_Decision/Action_Choice", action);

        switch (action)
        {
            case 0: // 가장 가까운 적 공격
                target = trainingManager.GetNearestEnemy();
                Attack();
                break;
            case 1: // 가장 체력이 낮은 적 공격
                target = trainingManager.GetWeakestEnemy();
                Attack();
                break;
            //case 2: // 아이템 먹기
            //    item = trainingManager.GetNearestItem(false);
            //    if(item != null)
            //        GetItem();
            //    else
            //        GameManager.Instance.attackCircle.GetComponent<CircleAgent>().UpdateActionState(false);
            //    break;
            //case 3: // 코인 먹기
            //    item = trainingManager.GetNearestItem(true);
            //    if (item != null)
            //        GetItem();
            //    else
            //        GameManager.Instance.attackCircle.GetComponent<CircleAgent>().UpdateActionState(false);
            //    break;
            default:
                break;
        }

        void Attack()
        {
            units.ForEach(e =>
            {
                if (e.TryGetComponent<CharacterBase>(out CharacterBase character))
                    character.AIActionByJob(target);
            });
        }
        
        //void GetItem()
        //{
        //    units.ForEach(e =>
        //    {
        //        if (e.TryGetComponent<CharacterBase>(out CharacterBase character))
        //            character.MoveToTarget(item.gameObject);
        //    });
        //}
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //스쿼드 캐릭터베이스로 옮기기
        if(hit.gameObject.layer == LayerMask.NameToLayer("Item"))
        {
            AddReward(RewardConstant.GetItemScore);
        }

        if(hit.gameObject.layer == LayerMask.NameToLayer("Coin"))
        {
            AddReward(RewardConstant.GetCoinScore);
        }
    }
}
