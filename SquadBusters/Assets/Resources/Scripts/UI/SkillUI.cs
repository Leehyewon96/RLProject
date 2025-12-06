using UnityEngine;
using UnityEngine.UI;

public class SkillUI : UIBase
{
    private Button button = null;
    private ItemType skillType;
    [SerializeField] private Image bombIcon = null;
    [SerializeField] private Image cannonIcon = null;

    public delegate void DoSkill(ItemType type);
    public DoSkill doSkill = null;

    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        SetInteractable(false);
        button.onClick.AddListener(OnButtonClick);
        bombIcon.gameObject.SetActive(false);
        cannonIcon.gameObject.SetActive(false);
    }

    public void SetInteractable(bool isEnable)
    {
        button.interactable = isEnable;
    }

    public void UpdateSkillType(ItemType type)
    {
        skillType = type;

        //스킬 아이콘 업데이트 코드 추가 
        switch(type)
        {
            case ItemType.Bomb:
                bombIcon.gameObject.SetActive(true);
                cannonIcon.gameObject.SetActive(false);
                break;
            case ItemType.Cannon:
                bombIcon.gameObject.SetActive(false);
                cannonIcon.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    public void OnButtonClick()
    {
        if (doSkill != null)
        {
            doSkill.Invoke(skillType);
        }
        GameManager.Instance.soundManager.Play(SoundEffectType.UseItem);
        bombIcon.gameObject.SetActive(false);
        cannonIcon.gameObject.SetActive(false);
        SetInteractable(false);
    }
}
