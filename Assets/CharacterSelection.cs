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
        RelayManager.Instance.selectedCharacterIndex = index; // บันทึกตัวเลือก
        SetSelectedPrefab();
        SetupConnectionApproval(); // ✅ ตั้งค่า Connection Approval
        RelayManager.Instance.StartRelay();
    }

    public void ConfirmAndPlayClient()
    {
        RelayManager.Instance.selectedCharacterIndex = index; // บันทึกตัวเลือก
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
    private void SetupConnectionApproval()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback += OnConnectionApproval;
    }

    private void OnConnectionApproval(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // ✅ อนุมัติการเชื่อมต่อทั้งหมด
        response.Approved = true;
        response.CreatePlayerObject = true;
        
        // ✅ ส่งข้อมูลตัวละครที่เลือกให้ client
        response.PlayerPrefabHash = GetPrefabHash(RelayManager.Instance.selectedCharacterIndex);
    }

    private uint GetPrefabHash(int characterIndex)
    {
        if (characterIndex >= 0 && characterIndex < RelayManager.Instance.characterPrefabs.Length)
        {
            var prefab = RelayManager.Instance.characterPrefabs[characterIndex];
            var networkObject = prefab.GetComponent<NetworkObject>();
            
            // ✅ ใช้ PrefabIdHash แทน GlobalObjectIdHash
            return networkObject.PrefabIdHash;
        }
        return 0;
    }
}
