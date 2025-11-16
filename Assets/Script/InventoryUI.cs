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
    public GameObject Panel;

    [Header("Equipment Slots & Swap")]
    [Tooltip("ช่อง Equipment 5 ช่องตามลำดับ: Head, Weapon, Shield, Armor, Boots")]
    public GameObject[] listEquitpment; 
    private List<InventorySlots> inventorySlots = new List<InventorySlots>();
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
        Panel.gameObject.SetActive(false);
        
    }

    private void SetupSlots()
    {
        // 1. 🛡️ ตั้งค่า Equipment Slots 5 ช่อง
        SetupEquipmentSlots();

        // 2. 🎒 ตั้งค่า Standard Inventory Slots (Stackable)
        SetupStandardSlots();
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

            InventorySlots slot = new InventorySlots
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

            InventorySlots slot = new InventorySlots
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
            InventorySlots slot = inventorySlots[i];
            
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
            InventorySlots slot = inventorySlots[i];
            
            if (string.IsNullOrEmpty(slot.CurrentItemName) && slot.SlotType == Equipment.None)
            {
                ApplyItemToSlot(slot, newItemData);
                return;
            }
        }
        
        Debug.Log("Inventory is full. Cannot display new item.");
    }

    // ----------------------------------------------------
    // 🛡️ Logic สำหรับ Equipment (Single Slot)
    // ----------------------------------------------------
    
    private void HandleEquipmentCollect(ItemData newItemData, Equipment itemType)
    {
        InventorySlots targetSlot = FindEquipmentSlot(itemType);
        
        if (targetSlot == null)
        {
            Debug.LogError($"InventoryUI: Could not find slot for {itemType}.");
            return;
        }

        if (string.IsNullOrEmpty(targetSlot.CurrentItemName))
        {
            ApplyItemToSlot(targetSlot, newItemData);
            return;
        }
    }


    // ----------------------------------------------------
    // 💡 Helper Functions
    // ----------------------------------------------------

    private InventorySlots FindEquipmentSlot(Equipment type)
    {
        // ค้นหา Slot ที่ตรงกับ Equipment Type
        foreach (InventorySlots slot in inventorySlots)
        {
            if (slot.SlotType == type)
            {
                return slot;
            }
        }
        return null;
    }
    
    private void ApplyItemToSlot(InventorySlots slot, ItemData itemData)
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
}
