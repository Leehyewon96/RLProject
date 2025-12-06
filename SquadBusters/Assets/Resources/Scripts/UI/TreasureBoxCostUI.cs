using TMPro;
using UnityEngine;

public class TreasureBoxCostUI : UIBase
{
    [SerializeField] private TextMeshProUGUI boxCostText = null;

    public void SetBoxCostText(int newCost)
    {
        boxCostText.text = newCost.ToString();
    }

    public void UpdatePos(Vector3 newPos)
    {
        gameObject.transform.position = newPos;
    }
}
