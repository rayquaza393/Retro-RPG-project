using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterLobbyUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;
    public Button leftButton;
    public Button rightButton;
    public Button enterWorldButton;

    private CharacterLobbyController controller;

    public void Init(CharacterLobbyController ctrl)
    {
        controller = ctrl;

        // Hook up UI events
        leftButton.onClick.AddListener(() => controller.PreviousCharacter());
        rightButton.onClick.AddListener(() => controller.NextCharacter());
        enterWorldButton.onClick.AddListener(() => controller.OnEnterWorldPressed());
    }

    public void UpdateCharacterInfo(CharacterData character)
    {
        nameText.text = character.name;
    }
}
