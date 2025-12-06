using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AttackCircleManager : MonoBehaviour
{
    private List<AttackCircle> attackCircleList = new List<AttackCircle>();

    private void Awake()
    {
        attackCircleList = GetComponentsInChildren<AttackCircle>(true).ToList();
        InitAttackCircles();
    }

    public void InitAttackCircles()
    {
        foreach (AttackCircle circle in attackCircleList)
        {
            circle.SetActive(false);
            circle.UpdateIsUsed(false);
        }
    }

    public AttackCircle GetAttackCircle(AttackCircle.circleType inType)
    {
        AttackCircle circle = attackCircleList.Find(c => !c.isUsed && c.type == inType);
        if(circle == null)
        {
            switch (inType)
            { 
                case AttackCircle.circleType.Player:
                    circle = Instantiate(attackCircleList[0], transform);
                    break;
                case AttackCircle.circleType.NPC:
                    circle = Instantiate(attackCircleList[1], transform);
                    break;
            }
            
            attackCircleList.Add(circle);
        }

        circle.SetActive(true);
        if(inType == AttackCircle.circleType.NPC)
        {
            circle.UpdateIsUsed(true);
        }
        
        return circle;
    }
}
