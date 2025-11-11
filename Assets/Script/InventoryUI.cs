using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

// 💡 Enum Equipment ต้องอยู่ในไฟล์ที่เข้าถึงได้ (Global Scope)
public enum Equipment
{
    Weapon,
    Shield,
    Armor,
    Head,
    Boots,
    None
}

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI Slot Setup")]
    [Tooltip("ช่อง Inventory ทั่วไป (Stackable Slots)")]
    public GameObject[] listContian; 

    [Header("Equipment Slots & Swap")]
    [Tooltip("ช่อง Equipment 5 ช่องตามลำดับ: Head, Weapon, Shield, Armor, Boots")]
    public GameObject[] listEquitpment; 
    [SerializeField] private GameObject swapPromptPanel;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private List<InventorySlot> inventorySlots = new List<InventorySlot>();
    private ItemData itemWaitingForSwap; 
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        SetupSlots();
    }

    private void SetupSlots()
    {
        // 1. 🛡️ ตั้งค่า Equipment Slots 5 ช่อง
        SetupEquipmentSlots();

        // 2. 🎒 ตั้งค่า Standard Inventory Slots (Stackable)
        SetupStandardSlots();
        
        // ตั้งค่าปุ่ม Swap Prompt
        if (swapPromptPanel != null)
        {
            swapPromptPanel.SetActive(false);
            yesButton.onClick.RemoveAllListeners();
            yesButton.onClick.AddListener(ConfirmSwap);
            noButton.onClick.RemoveAllListeners();
            noButton.onClick.AddListener(CancelSwap);
        }
        
        Debug.Log($"InventoryUI initialized with {inventorySlots.Count} total slots.");
    }
    
    // ----------------------------------------------------
    // 🛡️ Setup Logic สำหรับ 5 ช่อง Equipment
    // ----------------------------------------------------
    private void SetupEquipmentSlots()
    {
        if (listEquitpment == null || listEquitpment.Length != 5)
        {
            Debug.LogError("listEquitpment must contain exactly 5 elements!");
            return;
        }

        Equipment[] equipmentOrder = new Equipment[] 
        {
            Equipment.Head, 
            Equipment.Weapon, 
            Equipment.Shield, 
            Equipment.Armor, 
            Equipment.Boots
        };
        
        for (int i = 0; i < 5; i++)
        {
            GameObject slotObj = listEquitpment[i];
            SlotCompanent slotComp = slotObj.GetComponent<SlotCompanent>(); 
            
            if (slotComp == null) continue;

            InventorySlot slot = new InventorySlot
            {
                SlotObject = slotObj,
                ItemIcon = slotComp.ItemIcon,
                ItemCountText = slotComp.ItemCountText,
                SlotType = equipmentOrder[i] // ⬅️ กำหนด Type จากลำดับที่แน่นอน
            };
            slot.SetSlotEmpty();
            inventorySlots.Add(slot);
        }
    }
    
    // ----------------------------------------------------
    // 🎒 Setup Logic สำหรับ ช่อง Inventory ทั่วไป (Stackable)
    // ----------------------------------------------------
    private void SetupStandardSlots()
    {
        foreach (GameObject slotObj in listContian)
        {
            SlotCompanent slotComp = slotObj.GetComponent<SlotCompanent>(); 
            
            if (slotComp == null) continue;

            InventorySlot slot = new InventorySlot
            {
                SlotObject = slotObj,
                ItemIcon = slotComp.ItemIcon,
                ItemCountText = slotComp.ItemCountText,
                SlotType = Equipment.None // ⬅️ กำหนด Type เป็น None สำหรับ Stackable
            };
            slot.SetSlotEmpty();
            inventorySlots.Add(slot);
        }
    }


    // ----------------------------------------------------
    // 🎯 ฟังก์ชันหลัก: อัปเดต UI เมื่อเก็บ Item
    // ----------------------------------------------------

    public void UpdateUIOnItemCollect(ItemData newItemData, Equipment itemType)
    {
        // 1. ตรวจสอบว่าเป็น Equipment
        if (itemType != Equipment.None)
        {
            HandleEquipmentCollect(newItemData, itemType);
            return;
        }

        // --- Logic สำหรับ Stackable Item (Equipment.None) ---
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            
            // 💡 ถ้าชื่อ Item ซ้ำ และเป็น Slot ทั่วไป (Stackable)
            if (slot.CurrentItemName == newItemData.Name && slot.SlotType == Equipment.None)
            {
                slot.CurrentCount++;
                slot.ItemCountText.text = (slot.CurrentCount > 1) ? slot.CurrentCount.ToString() : ""; 
                return;
            }
        }
        
        // 2. ถ้าไม่ซ้ำ: หา Slot ว่าง (Non-Equipment Slot)
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            InventorySlot slot = inventorySlots[i];
            
            if (string.IsNullOrEmpty(slot.CurrentItemName) && slot.SlotType == Equipment.None)
            {
                ApplyItemToSlot(slot, newItemData);
                return;
            }
        }
        
        Debug.LogWarning("Inventory is full. Cannot display new item.");
    }

    // ----------------------------------------------------
    // 🛡️ Logic สำหรับ Equipment (Single Slot)
    // ----------------------------------------------------
    
    private void HandleEquipmentCollect(ItemData newItemData, Equipment itemType)
    {
        InventorySlot targetSlot = FindEquipmentSlot(itemType);
        
        if (targetSlot == null)
        {
            Debug.LogError($"InventoryUI: Could not find slot for {itemType}.");
            return;
        }

        // 1. ถ้า Slot ว่าง: สวมใส่ทันที
        if (string.IsNullOrEmpty(targetSlot.CurrentItemName))
        {
            ApplyItemToSlot(targetSlot, newItemData);
            // 🚨 Note: คุณต้องเรียก Player.EquipVisualsServerRpc ที่นี่เพื่อสวมใส่
            return;
        }
        
        // 2. ถ้า Slot ไม่ว่าง: แสดง Prompt Swap
        itemWaitingForSwap = newItemData;
        
        if (swapPromptPanel != null)
        {
            swapPromptPanel.SetActive(true);
            Debug.Log($"[UI PROMPT] Slot {itemType} is full. Swap {targetSlot.CurrentItemName} with {newItemData.Name}? (Yes/No)");
        }
    }

    // ----------------------------------------------------
    // Logic การ Swap
    // ----------------------------------------------------
    
    private void ConfirmSwap()
    {
        if (itemWaitingForSwap == null) return;

        InventorySlot targetSlot = FindEquipmentSlot(itemWaitingForSwap.EquipmentTybe); 
        
        if (targetSlot == null) return;
        ApplyItemToSlot(targetSlot, itemWaitingForSwap);
        swapPromptPanel.SetActive(false);
        itemWaitingForSwap = null;
    }
    
    private void CancelSwap()
    {
        // 🚨 (Server RPC Call): สั่ง Player ทิ้ง Item ที่เก็บมาใหม่
        // Player.Instance.RequestDropNewItemServerRpc(itemWaitingForSwap.Name);
        
        swapPromptPanel.SetActive(false);
        itemWaitingForSwap = null;
    }

    // ----------------------------------------------------
    // 💡 Helper Functions
    // ----------------------------------------------------

    private InventorySlot FindEquipmentSlot(Equipment type)
    {
        // ค้นหา Slot ที่ตรงกับ Equipment Type
        foreach (InventorySlot slot in inventorySlots)
        {
            if (slot.SlotType == type)
            {
                return slot;
            }
        }
        return null;
    }
    
    private void ApplyItemToSlot(InventorySlot slot, ItemData itemData)
    {
        slot.CurrentItemName = itemData.Name;
        slot.CurrentCount = 1; 
        if (slot.SlotType != Equipment.None)
        {
            // ถ้าเป็น Equipment Slot (ไม่นับ Stack) ให้แสดงชื่อ Item แทน
            if (slot.ItemCountText != null)
            {
                slot.ItemCountText.text = itemData.Name; // ⬅️ แสดงชื่อ Item ที่สวมใส่
            }
        }
        else
        {
            // ถ้าเป็น Stackable Slot ให้ Text เป็นค่าว่าง (นับ 1)
            if (slot.ItemCountText != null)
            {
                slot.ItemCountText.text = "";
            }
        }

        if (slot.ItemIcon != null && itemData.sprite != null)
        {
            slot.ItemIcon.sprite = itemData.sprite;
            slot.ItemIcon.color = Color.white;
        }
    }
    
    private Equipment GetEquipmentTypeFromSlotName(string slotName)
    {
        // 💡 ใช้ Contains เพื่อให้ยืดหยุ่น โดยการตรวจสอบชื่อ GameObject ใน Inspector
        if (slotName.Contains("Weapon")) return Equipment.Weapon;
        if (slotName.Contains("Shield")) return Equipment.Shield;
        if (slotName.Contains("Armor")) return Equipment.Armor;
        if (slotName.Contains("Head")) return Equipment.Head;
        if (slotName.Contains("Boots")) return Equipment.Boots;
        return Equipment.None; 
    }
}










// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using System.Collections.Generic;

// public enum Equipment
// {
//     Weapon,
//     Shield,
//     Armor,
//     Head,
//     Boots,
//     None
// }
// public class InventoryUI : MonoBehaviour
// {
    
//     // 💡 Singleton Instance: เพื่อให้ Player เข้าถึงได้ง่าย
//     public static InventoryUI Instance { get; private set; }

//     [Header("UI Slot Setup")]
//     public GameObject[] listContian;
//     public Sprite Weapon;
//     public Sprite Shield;
//     public Sprite Armor;
//     public Sprite Head;
//     public Sprite Boots;
//     private List<InventorySlot> inventorySlots = new List<InventorySlot>();

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }
//         Instance = this;
//     }

//     void Start()
//     {
//         SetupSlots();
//     }
//     private void SetupSlots()
//     {
//         foreach (GameObject slotObj in listContian)
//         {
//             // ดึง Script Component ที่ถือการอ้างอิงที่ถูกต้อง
//             // ต้องมั่นใจว่า SlotCompanent เป็นชื่อคลาสที่คุณใช้จริง
//             SlotCompanent slotComp = slotObj.GetComponent<SlotCompanent>(); 

//             if (slotComp == null)
//             {
//                 Debug.LogError($"Slot object {slotObj.name} is missing the SlotCompanent script.");
//                 continue;
//             }

//             InventorySlot slot = new InventorySlot
//             {
//                 SlotObject = slotObj,
//                 ItemIcon = slotComp.ItemIcon,
//                 ItemCountText = slotComp.ItemCountText
//             };
//             slot.SetSlotEmpty();
            
//             // 🚨 แก้ไข: เพิ่ม Slot ที่สร้างแล้วเข้าไปใน List หลัก
//             inventorySlots.Add(slot);
//         }
//         Debug.Log($"InventoryUI initialized with {inventorySlots.Count} slots.");
//     }
    
//     // ----------------------------------------------------
//     // 🎯 ฟังก์ชันหลัก: อัปเดต UI เมื่อเก็บ Item (Local Only)
//     // ----------------------------------------------------

//     public void UpdateUIOnItemCollect(ItemData newItemData)
//     {
        
//         // 1. ตรวจสอบว่ามี Item นี้อยู่ใน Slot แล้วหรือยัง (Stackable Logic)
//         for (int i = 0; i < inventorySlots.Count; i++)
//         {
//             InventorySlot slot = inventorySlots[i];
            
//             if (slot.CurrentItemName == newItemData.Name)
//             {
//                 // ถ้าซ้ำ: +1 และอัปเดต Text
//                 slot.CurrentCount++;
//                 slot.ItemCountText.text = (slot.CurrentCount > 1) ? slot.CurrentCount.ToString() : ""; 
//                 return;
//             }
//         }

//         // 2. ถ้าไม่ซ้ำ: หา Slot ที่ว่างเพื่อเพิ่ม Item ใหม่
//         for (int i = 0; i < inventorySlots.Count; i++)
//         {
//             InventorySlot slot = inventorySlots[i];
            
//             if (string.IsNullOrEmpty(slot.CurrentItemName))
//             {
//                 // ✅ Slot ว่างเปล่า, เพิ่ม Item ใหม่
//                 slot.CurrentItemName = newItemData.Name;
//                 slot.CurrentCount = 1;
                
//                 // 🚨 แก้ไข: ตรวจสอบ Sprite ก่อนตั้งค่า
//                 if (slot.ItemIcon != null && newItemData.sprite != null)
//                 {
//                     slot.ItemIcon.sprite = newItemData.sprite;
//                     slot.ItemIcon.color = Color.white; // ทำให้ไอคอนมองเห็นได้
//                 }
                
//                 slot.ItemCountText.text = ""; 
//                 return;
//             }
//         }
        
//         // 3. ถ้าไม่มีช่องว่าง
//         Debug.LogWarning("Inventory is full. Cannot display new item.");
//         // Note: ถ้าคุณต้องการให้ AddItem return ออกไป ต้องเพิ่ม Logic ใน Player.AddItem
//         // เพื่อตรวจสอบว่า InventoryUI.UpdateUIOnItemCollect ส่งค่าเตือน Inventory full หรือไม่
//     }
// }