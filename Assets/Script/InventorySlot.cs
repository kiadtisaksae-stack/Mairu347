using UnityEngine;
using UnityEngine.UI;
using TMPro; 
[System.Serializable]
public class InventorySlot
{
    public Equipment SlotType = Equipment.None;
    public GameObject SlotObject; 
    public Image ItemIcon;
    public TextMeshProUGUI ItemCountText;
    public string CurrentItemName; 
    public int CurrentCount = 0;
    public void SetSlotEmpty()
    {
        if (ItemIcon != null) ItemIcon.sprite = null;
        if (ItemCountText != null) ItemCountText.text = "";
        CurrentItemName = string.Empty;
        CurrentCount = 0;
    }
    
}