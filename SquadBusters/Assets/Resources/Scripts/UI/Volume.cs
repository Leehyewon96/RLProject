using UnityEngine;
using UnityEngine.UI;

public class Volume : MonoBehaviour
{
    private Button button = null;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }

    private void OnButtonClick()
    {
        GameManager.Instance.soundManager.Mute();
    }
}
