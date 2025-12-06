using UnityEngine;
using UnityEngine.UI;

public class SelectCharacter : MonoBehaviour
{
    public CharacterButton char1 = null;
    public CharacterButton char2 = null;
    public CharacterButton char3 = null;

    public delegate void OnDisabled();
    public OnDisabled onDisabled = null;

    private void Awake()
    {
        char1.SetCharacterType(CharacterType.Greg);
        char2.SetCharacterType(CharacterType.ElPrimo);
        char3.SetCharacterType(CharacterType.Colt);

        char1.GetComponent<Button>().onClick.AddListener(delegate { SelectChar(char1.characterType, CharacterLevel.Classic); });
        char2.GetComponent<Button>().onClick.AddListener(delegate { SelectChar(char2.characterType, CharacterLevel.Classic); });
        char3.GetComponent<Button>().onClick.AddListener(delegate { SelectChar(char3.characterType, CharacterLevel.Classic); });
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void SelectChar(CharacterType newType, CharacterLevel newLevel)
    {
        GameManager.Instance.attackCircle.GetComponent<IAttackCircleUIInterface>().SelectCharacter(newType, newLevel);
        if(onDisabled !=null)
        {
            onDisabled.Invoke();
        }
        //SetActive(false);
    }
}
