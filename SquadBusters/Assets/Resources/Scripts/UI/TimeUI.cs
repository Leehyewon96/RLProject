using TMPro;
using UnityEngine;

public class TimeUI : UIBase
{
    [SerializeField] private TextMeshProUGUI timeText = null;

    public void UpdateTime(string newTime)
    {
        timeText.SetText(newTime);
    }
}
