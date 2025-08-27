using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class CharacterLobbyController : MonoBehaviour
{
    public CharacterLobbyUI ui;
    public Transform playerNode;

    public GameObject[] characterPrefabs; // Optional: used if you want diff models
    private GameObject currentCharacterGO;

    private List<CharacterData> characters = new List<CharacterData>();
    private int currentIndex = 0;

    void Start()
    {
        ui.Init(this);
        RequestCharacterList();
    }

    void RequestCharacterList()
    {
        NetworkAPI.Instance.Send("player.character.list", new Dictionary<string, object>());
    }

    public void OnCharacterListReceived(List<CharacterData> charList)
    {
        characters = charList;
        currentIndex = 0;
        UpdateDisplayedCharacter();
    }

    public void NextCharacter()
    {
        if (characters.Count == 0) return;
        currentIndex = (currentIndex + 1) % characters.Count;
        UpdateDisplayedCharacter();
    }

    public void PreviousCharacter()
    {
        if (characters.Count == 0) return;
        currentIndex = (currentIndex - 1 + characters.Count) % characters.Count;
        UpdateDisplayedCharacter();
    }

    void UpdateDisplayedCharacter()
    {
        if (currentCharacterGO != null)
            Destroy(currentCharacterGO);

        if (characters.Count == 0) return;

        // Placeholder — assuming all characters use same prototype prefab
        GameObject prefab = characterPrefabs[0];
        currentCharacterGO = Instantiate(prefab, playerNode.position, Quaternion.identity);
        currentCharacterGO.transform.SetParent(playerNode); // Optional, if you want it anchored
        currentCharacterGO.transform.localPosition = Vector3.zero;

        ui.UpdateCharacterInfo(characters[currentIndex]);
    }

    public void OnEnterWorldPressed()
    {
        if (characters.Count == 0) return;

        Dictionary<string, object> payload = new()
        {
            { "char_id", characters[currentIndex].id }
        };

        NetworkAPI.Instance.Send("player.enter", payload);
    }
}
