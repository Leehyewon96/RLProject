using TMPro;
using UnityEngine;

public class CoinUI : UIBase
{
    [SerializeField] private TextMeshProUGUI text = null;

    public void SetCoin(int cnt)
    {
        text.SetText(cnt.ToString());
    }

    public void UpdatePos(Vector3 newPos)
    {
        gameObject.transform.position = newPos;
    }
}
