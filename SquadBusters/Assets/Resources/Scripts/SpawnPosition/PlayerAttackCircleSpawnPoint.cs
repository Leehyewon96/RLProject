using UnityEngine;

public class PlayerAttackCircleSpawnPoint : MonoBehaviour
{
    private bool isAssigned = false;

    public bool GetIsAssigned()
    {
        return isAssigned; 
    }

    public void SetIsAssigned(bool assinged)
    {
        isAssigned = assinged;
    }

}
