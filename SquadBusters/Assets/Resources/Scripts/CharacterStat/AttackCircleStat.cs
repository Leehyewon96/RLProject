using UnityEngine;

public class AttackCircleStat : MonoBehaviour
{
    protected int coin = 2;
    protected int gem = 1;

    public float attackRadius = 2f;

    public int GetCoin()
    {
        return coin;
    }

    public int GetGem()
    {
        return gem;
    }

    public void SetCoin(int cnt)
    {
        coin = cnt;
    }

    public void SetGem(int cnt)
    {
        gem = cnt;
    }
}
