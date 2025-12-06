using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public enum barType
    {
        None = 0,

        Player,
        NPC,

        End = NPC + 1,
    }

    [SerializeField] private Image imgFill = null;
    public barType type = barType.None;
    private float maxHp = -1;
    [HideInInspector] public bool isUsed = false;

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void UPdateIsUsed(bool used)
    {
        isUsed = used;
    }

    public void SetMaxHp(float newMaxHp)
    {
        maxHp = newMaxHp;
    }

    public void UpdateCurrentHp(float newHp)
    {
        imgFill.fillAmount = newHp / maxHp;
    }

    public void UpdatePos(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public void InitColor(Color newColor)
    {
        imgFill.color = newColor;
    }
}
