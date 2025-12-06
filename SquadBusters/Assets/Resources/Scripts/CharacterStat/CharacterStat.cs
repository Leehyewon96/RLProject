using UnityEngine;


public class CharacterStat : MonoBehaviour
{
    private float maxHp = 100f;
    private float attackDamage = 30f;
    private float currentHp = -1f;

    private int coin = 0;
    private int gem = 0;

    public delegate void OnCurrentHpChanged(float newHp);
    public delegate void OnCurrentHpZero();
    public delegate void OnAttackRadiusChanged(float newAttackRadius);
    public delegate void OnCharacterBeginAttack();

    public event OnCurrentHpChanged onCurrentHpChanged;
    public event OnCurrentHpZero onCurrentHpZero;
    public event OnAttackRadiusChanged onAttackRadiusChanged;
    public event OnCharacterBeginAttack onCharacterBeginAttack;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public float GetCurrentHp() { return currentHp; }
    public float GetMaxHp() { return maxHp; }
    public float GetAttackDamage() { return attackDamage; }
    public int GetCoin() { return coin; }
    public int GetGem() { return gem; }
    

    public void Init(float inMaxHp, float inAttackDamage, int inCoin, int inGem)
    {
        maxHp = inMaxHp;
        currentHp = maxHp;
        attackDamage = inAttackDamage;
        coin = inCoin;
        gem = inGem;
    }

    public void ApplyDamage(float inDamage)
    {
        currentHp = Mathf.Clamp(currentHp - inDamage, 0, maxHp);

        if(onCurrentHpChanged != null)
        {
            onCurrentHpChanged.Invoke(currentHp);
        }
    }

    public bool CheckDead()
    {
        if (currentHp <= 0)
        {
            if (onCurrentHpZero != null)
            {
                onCurrentHpZero.Invoke();
            }
            return true;
        }

        return false;
    }
}
