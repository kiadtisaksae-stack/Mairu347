using UnityEngine;
using UnityEngine.UI; // สำหรับ UI Image
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;

public class CharacterSelection : MonoBehaviour
{
    public TextMeshProUGUI numberCharacterText; 
    public Image characterImage; 
    public List<Sprite> characterSprites; 
    public int index = 0;

    void Start()
    {
        UpdateCharacterNumber();
        UpdateCharacterImage(); 
    }

    public void NextCharacter()
    {
        index = (index + 1) % RelayManager.Instance.characterPrefabs.Length;
        UpdateCharacterNumber();
        UpdateCharacterImage();
    }

    public void PrevCharacter()
    {
        index = (index - 1 + RelayManager.Instance.characterPrefabs.Length) %
                RelayManager.Instance.characterPrefabs.Length;
        UpdateCharacterNumber();
        UpdateCharacterImage();
    }

    public void ConfirmAndPlayHost()
    {
        SetSelectedPrefab();
        RelayManager.Instance.StartRelay();
    }

    public void ConfirmAndPlayClient()
    {
        SetSelectedPrefab();
        RelayManager.Instance.JoinRelay();
    }

    private void SetSelectedPrefab()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab =
            RelayManager.Instance.characterPrefabs[index];
    }

    public void UpdateCharacterNumber() // ต้องเป็น public เผื่อให้ MainMenu เรียก
    {
        numberCharacterText.text = "Character : " + index.ToString();
    }

    private void UpdateCharacterImage()
    {
        if (characterSprites != null && characterSprites.Count > 0)
        {
            int spriteIndex = index % characterSprites.Count; // ป้องกัน out of range
            characterImage.sprite = characterSprites[spriteIndex];
        }
    }
}
