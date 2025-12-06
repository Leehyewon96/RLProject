using UnityEngine;
using UnityEngine.UI;

public class CharacterUI : UIBase
{
    protected Image hpBar = null;

    protected virtual void Awake()
    {
        hpBar = GetComponent<Image>();
    }
}
