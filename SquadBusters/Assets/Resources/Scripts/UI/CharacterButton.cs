using UnityEngine;

public class CharacterButton : MonoBehaviour
{
    public CharacterType characterType { get; private set; } = CharacterType.ElPrimo;

    public void SetCharacterType(CharacterType newType)
    {
        characterType = newType;
    }
}
