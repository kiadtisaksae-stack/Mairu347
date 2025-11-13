using UnityEngine;
using UnityEngine.UI;
using TMPro; // ต้องแน่ใจว่าได้ติดตั้ง TextMeshPro

[System.Serializable]
public class InventorySlot
{
    // 💡 GameObject หลักของ Slot (ที่คุณใช้ listContian[i])
    public GameObject SlotObject; 
    
    // 💡 Image Component (สำหรับแสดงไอคอน/Sprite)
    public Sprite ItemIcon;
    
    // 💡 Text Component (สำหรับแสดงจำนวน/Stack Count)
    public TextMeshProUGUI ItemCountText;

    // 💡 ข้อมูลอ้างอิง: เก็บชื่อ Item ที่ Slot นี้ถืออยู่
    public string CurrentItemName; 
    public Equipment SlotType; 
    
    // 💡 จำนวน Item ที่เก็บได้
    public int CurrentCount = 0; 

    public void SetSlotEmpty()
    {
        // เมื่อ Slot ว่างเปล่า
        if (ItemIcon != null) ItemIcon = null;
        if (ItemCountText != null) ItemCountText.text = "";
        CurrentItemName = string.Empty;
        CurrentCount = 0;
    }
}