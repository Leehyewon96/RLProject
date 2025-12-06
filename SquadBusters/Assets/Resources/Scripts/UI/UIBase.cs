using UnityEngine;

public class UIBase : MonoBehaviour
{
    public virtual void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }
}
