using UnityEngine;
using UnityEngine.UI;

public class EndingUI : RankUI
{
    [SerializeField] private Button btnNext = null;

    protected override void Awake()
    {
        base.Awake();

        SetActive(false);
    }

    public override void SetActive(bool isActive)
    {
        base.SetActive(isActive);
        Debug.Log($"endingUI setActive {isActive}");
    }
}
