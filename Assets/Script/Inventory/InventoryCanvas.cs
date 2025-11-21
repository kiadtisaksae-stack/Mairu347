using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCanvas : MonoBehaviour
{
    [Header("Inventory")]
    public ItemSO Empty_Item;
    public Transform slotPrefab;
    public Transform SlotPanel;
    public Transform InventoryPanel;
    protected GridLayoutGroup gridLayoutGroup;

    [Space(5)]
    public int slotAmount = 16;
    public InventorySlot[] inventorySlot;
    public GameObject headPrefab;
    public HeadSlot headSlot; // **เปลี่ยนเป็น HeadSlot**

    public GameObject rightPrefab;
    public RigthHand rightHandSlots; // **เปลี่ยนเป็น RightHandSlot**

    public GameObject leftPrefab;
    public LeftHand leftHandSlots; // **เปลี่ยนเป็น LeftHandSlot**

    public GameObject bodyPrefab;
    public BodySlot bodySlot; // **เปลี่ยนเป็น BodySlot**

    public GameObject legPrefab;
    public LegSlot legSlot; // **เปลี่ยนเป็น LegSlot**


    [Header("Selection")]
    public Button useButton;
    public Button deleteButton;
    public List<InventorySlot> selectedSlots = new List<InventorySlot>();
    public Color selectedSlotColor = new Color(0.5f, 0.8f, 1f, 0.5f);

    [Header("UI References")]
    public Canvas inventoryCanvas;
    private InputSystem_Actions inputActions;
    public Player playerController;
    private int openAndCloseCount = 0;



    void Start()
    {
        inventoryCanvas = GetComponent<Canvas>();
        gridLayoutGroup = SlotPanel.GetComponent<GridLayoutGroup>();
        InventoryPanel.gameObject.SetActive(false);
        CreateInventorylot();
        SetupEquipmentSlots();
        SetupButtonListeners();
        
    }
    void Update()
    {
        
    }
    void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
        inputActions.Player.Inventory.performed += ctx =>
        {
            InventoryPanel.gameObject.SetActive(!InventoryPanel.gameObject.activeSelf);
            
        };
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    #region Slot Creation Methods
    public virtual void CreateInventorylot()
    {
        inventorySlot = new InventorySlot[slotAmount];
        for (int i = 0; i < slotAmount; i++)
        {
            Transform slot = Instantiate(slotPrefab, SlotPanel);
            InventorySlot inveSlot = slot.GetComponent<InventorySlot>();
            inventorySlot[i] = inveSlot;
            inveSlot.iventory = this;
            inveSlot.SetThisSlot(Empty_Item, 0);
        }
    }
    private void SetupEquipmentSlots()
    {
        // กำหนดค่าเริ่มต้นและอ้างอิงกลับไปยัง InventoryCanvas
        if (headSlot != null)
        {
            headSlot.iventory = this;
            headSlot.SetThisSlot(Empty_Item, 0);
        }
        // ทำซ้ำสำหรับ bodySlot, legSlot, rightHandSlots, leftHandSlots
        if (bodySlot != null)
        {
            bodySlot.iventory = this;
            bodySlot.SetThisSlot(Empty_Item, 0);
        }
        if(rightHandSlots != null)
        {
            rightHandSlots.iventory = this;
            rightHandSlots.SetThisSlot(Empty_Item,0);
        }
        if(leftHandSlots != null)
        {
            leftHandSlots.iventory = this;
            leftHandSlots.SetThisSlot(Empty_Item, 0);
        
        }
        if(legSlot != null)
        {
            legSlot.iventory = this;
            legSlot.SetThisSlot(Empty_Item, 0);
        }
    }
    
    #endregion

    #region Selection Management
    private void SetupButtonListeners()
    {
        // เชื่อมต่อ useButton
        if (useButton != null)
        {
            useButton.onClick.RemoveAllListeners(); // ลบ listener เดิม
            useButton.onClick.AddListener(OnUseButtonClicked);
        }
        else
        {
            Debug.LogWarning("useButton is not assigned in Inspector!");
        }

        // เชื่อมต่อ deleteButton
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners(); // ลบ listener เดิม
            deleteButton.onClick.AddListener(OnDeleteButtonClicked);
        }
        else
        {
            Debug.LogWarning("deleteButton is not assigned in Inspector!");
        }
    }
    private void OnDeleteButtonClicked()
    {
        if (selectedSlots.Count > 0)
        {
            DeleteSelectedItems();
        }
        else
        {
            Debug.Log("No item selected to delete!");
        }
    }

    // อัพเดตสถานะปุ่มตามการเลือก
    public void UpdateButtonInteractability()
    {
        bool hasSelection = selectedSlots.Count > 0;

        if (useButton != null)
        {
            useButton.interactable = hasSelection;
        }

        if (deleteButton != null)
        {
            deleteButton.interactable = hasSelection;
        }
    }

    private void OnUseButtonClicked()
    {
        if (selectedSlots.Count > 0)
        {
            UseItem();
        }
        else
        {
            Debug.Log("No item selected to use!");
        }
    }
    public void SelectThisSlot(InventorySlot slot)
    {
        // ตรวจสอบว่า slot นี้ถูกเลือกอยู่แล้วหรือไม่
        if (selectedSlots.Contains(slot) && selectedSlots.Count == 1)
            return;

        // Clear all selections first
        ClearAllSelections();

        // Add new selection
        selectedSlots.Add(slot);
        slot.isSelected = true; // ตั้งค่าโดยตรงแทนการเรียก method
        slot.UpdateSelectionVisual();
        UpdateButtonInteractability();
    }

    public void UpdateSlotSelection(InventorySlot slot, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedSlots.Contains(slot))
                selectedSlots.Add(slot);
        }
        else
        {
            selectedSlots.Remove(slot);
        }
        UpdateButtonInteractability();

    }

    public void ClearAllSelections()
    {
        // ใช้ for loop แทน foreach เพื่อป้องกัน modification during iteration
        for (int i = selectedSlots.Count - 1; i >= 0; i--)
        {
            InventorySlot slot = selectedSlots[i];
            slot.isSelected = false; // ตั้งค่าโดยตรง
            slot.UpdateSelectionVisual();
        }
        selectedSlots.Clear();
        UpdateButtonInteractability();
    }

    public bool HasMultipleSelections()
    {
        return selectedSlots.Count > 1;
    }

    public void DeleteSelectedItems()
    {
        if (selectedSlots.Count == 0) return;

        // Create copy to avoid modification during iteration
        List<InventorySlot> slotsToDelete = new List<InventorySlot>(selectedSlots);

        foreach (InventorySlot slot in slotsToDelete)
        {
            if (slot.item != Empty_Item)
            {
                RemoveItem(slot);
            }
        }

        ClearAllSelections();
    }
    #endregion

    #region Inventory Methods
    public void MakeThisToTopLayer(bool toTop, int extraLayer = 0)
    {
        if (inventoryCanvas != null)
        {
            if (toTop)
                inventoryCanvas.sortingOrder = 100 + extraLayer; // เพิ่ม layer
            else
                inventoryCanvas.sortingOrder = 10 + extraLayer;  // ลดกลับ
        }
    }

    public virtual void AddItem(ItemSO item, int amount)
    {
        InventorySlot slot = IsEmptySlotLeft(item);
        if (slot == null)
        {
            DropItem(item, amount);
            return;
        }
        slot.MergeThisSlot(item, amount);
    }

    public void UseItem()
    {
        if (selectedSlots.Count > 0)
        {
            selectedSlots[0].UseItem();
            
        }
    }

    public void DropItem()
    {
        if (selectedSlots.Count > 0)
        {
            NetworkPlayerManager playerManager = FindOwnerPlayerManager();
            if (playerManager != null && ItemSpawner.Instance != null)
            {
                ItemSpawner.Instance.SpawnItem(
                    selectedSlots[0].item,
                    selectedSlots[0].stack,
                    playerManager.transform.position
                );
                RemoveItem(selectedSlots[0]);
            }
        }
    }
    

    private NetworkPlayerManager FindOwnerPlayerManager()
    {
        var player = FindFirstObjectByType<NetworkPlayerManager>();
        return (player != null && player.player) ? player : null;
    }

    public void DropItem(ItemSO item, int amount)
    {
        NetworkPlayerManager playerManager = FindOwnerPlayerManager();
        if (playerManager != null && ItemSpawner.Instance != null)
        {
            Vector3 dropPosition = playerManager.transform.position + Vector3.forward * 2;
            ItemSpawner.Instance.SpawnItem(item, amount, dropPosition);
        }
    }

    public void RemoveItem(InventorySlot slot)
    {
        slot.SetThisSlot(Empty_Item, 0);
        selectedSlots.Remove(slot);
    }

    public void SetLayoutControlChiad(bool isControlled)
    {
        if (gridLayoutGroup != null)
            gridLayoutGroup.enabled = isControlled;
    }
    #endregion

    #region Utility Methods
    private InventorySlot FindFirstNonEmptySlot()
    {
        foreach (InventorySlot slot in inventorySlot)
        {
            if (slot.item != Empty_Item)
                return slot;
        }
        return null;
    }

    public InventorySlot IsEmptySlotLeft(ItemSO itemChecker = null, InventorySlot itemslot = null)
    {
        if (inventorySlot == null) return null;

        InventorySlot firstEmptySlot = null;
        foreach (InventorySlot slot in inventorySlot)
        {
            if (slot == itemslot) continue;

            if (slot.item == itemChecker && slot.stack < slot.item.maxStack)
            {
                return slot;
            }
            else if (slot.item == Empty_Item && firstEmptySlot == null)
            {
                firstEmptySlot = slot;
            }
        }
        return firstEmptySlot;
    }
    #endregion


}