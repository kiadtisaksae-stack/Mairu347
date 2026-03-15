using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Unity.Netcode;

public class InventorySlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler, IPointerClickHandler
{
    [Header("Crafting Detail")]
    public GameObject[] reCipePrefubs;

    [Header("Inventory Detail")]
    public InventoryCanvas iventory;

    [Header("Slot Detail")]
    public ItemSO item;
    public int stack;

    [Header("UI")]
    public Color emptyColor;
    public Color itemColor;
    [SerializeField] private Outline outline;
    public Color selectedColor;
    public Color backgroundColor;
    public Image icons;
    public Image background;
    public TextMeshProUGUI stackText;

    [Header("Drag and Drop")]
    public int siblingIndex;
    public int craftInts;
    public RectTransform draggable;
    protected Canvas canvas;
    protected CanvasGroup canvasGroup;

    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    public bool isSelected = false;
    private InputSystem_Actions inputActions;
    private RectTransform canvasRect;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        siblingIndex = transform.GetSiblingIndex();
        if (canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        if (background == null)
            background = GetComponent<Image>();
    }

    void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Enable();
    }

    void OnDisable()
    {
        if (inputActions != null)
            inputActions.Disable();
    }

    #region Drag and Drop

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
        iventory.SetLayoutControlChiad(false);
        iventory.MakeThisToTopLayer(true, 2);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPoint);
        draggable.anchoredPosition = localPoint;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector3 worldPoint))
        {
            draggable.position = worldPoint;
        }
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.blocksRaycasts = true;
        draggable.anchoredPosition = Vector2.zero;
        transform.SetSiblingIndex(siblingIndex);
    }

    public virtual void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;

        InventorySlot slot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (slot == null) return;

        if (slot.item == item)
            MergeThisSlot(slot);
        else
            SwapSlot(slot);

        // อัพเดต Equipment บน Server
        Player player = GetLocalPlayer();
        if (player != null)
        {
            if (this == iventory.headSlot) player.EquipHead();
            else if (this == iventory.bodySlot) player.EquipBody();
            else if (this == iventory.legSlot) player.EquipLeg();
            else if (this == iventory.rightHandSlots || this == iventory.leftHandSlots)
                player.EquipWeapon();

            player.UpdateEquipmentStats();
        }
    }

    #endregion

    #region Click and Selection

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (item == iventory.Empty_Item) return;

        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - lastClickTime < doubleClickThreshold)
            {
                OnPointerDoubleClick(eventData);
            }
            else
            {
                if (inputActions != null && inputActions.UI.RightClick.IsPressed())
                    ToggleSelection();
                else
                {
                    if (isSelected && iventory.selectedSlots.Count == 1)
                        DeselectThisSlot();
                    else
                        SelectThisSlot();
                }
                inputActions?.Disable();
            }
            lastClickTime = Time.time;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            iventory.ClearAllSelections();
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, false);
        }
    }

    public void OnPointerDoubleClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && item != iventory.Empty_Item)
            UseItem();
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        UpdateSelectionVisual();
        iventory.UpdateSlotSelection(this, isSelected);
    }

    public void SelectThisSlot()
    {
        if (!isSelected)
        {
            isSelected = true;
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, true);
        }
    }

    public void DeselectThisSlot()
    {
        if (isSelected)
        {
            isSelected = false;
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, false);
        }
    }

    public void UpdateSelectionVisual()
    {
        if (outline != null)
        {
            outline.enabled = isSelected;
            if (isSelected)
            {
                outline.effectColor = selectedColor;
                outline.effectDistance = new Vector2(3, 3);
            }
        }

        if (background != null)
            background.color = isSelected ? backgroundColor : Color.white;
    }

    public bool IsSelected() => isSelected;

    #endregion

    #region Button Handlers

    public void OnClickButtonUseItem()
    {
        if (item != iventory.Empty_Item)
        {
            UseItem();
            iventory.UpdateButtonInteractability();
        }
    }

    public void OnClickButtonDeleteItem()
    {
        if (item != iventory.Empty_Item)
        {
            if (iventory.HasMultipleSelections())
                iventory.DeleteSelectedItems();
            else
                iventory.RemoveItem(this);

            iventory.UpdateButtonInteractability();
        }
    }

    #endregion

    #region Item Usage

    public virtual void UseItem()
    {
        if (item == iventory.Empty_Item) return;

        bool itemUsed = ApplyItemEffects();

        if (itemUsed)
        {
            stack = Mathf.Clamp(stack - 1, 0, item.maxStack);
            if (stack > 0)
                checkShowText();
            else
                iventory.RemoveItem(this);
        }

        DeselectThisSlot();
    }

    private bool ApplyItemEffects()
    {
        Player player = GetLocalPlayer();
        if (player == null)
        {
            Debug.LogWarning("[InventorySlot] ไม่พบ Local Player!");
            return false;
        }

        switch (item.tybe)
        {
            case ItemTypes.CONSUMABLE:
                return UseConsumableItem(player);

            case ItemTypes.HEAD:
            case ItemTypes.BODY:
            case ItemTypes.LEGS:
            case ItemTypes.ONE_HAND_WEAPON:
            case ItemTypes.TWO_HAND_WEAPON:
                return EquipItem(player);

            default:
                Debug.Log($"[InventorySlot] ไม่รู้จักประเภทไอเทม: {item.tybe}");
                return false;
        }
    }

    private bool UseConsumableItem(Player player)
    {
        bool effectApplied = false;

        // ✅ ส่งผ่าน ServerRpc — Client ห้ามเรียก Heal() ตรง
        if (item.healAmount > 0)
        {
            player.HealServerRpc(item.healAmount);
            Debug.Log($"[InventorySlot] ใช้ {item.itemName} รักษา {item.healAmount} HP");
            effectApplied = true;
        }

        if (!effectApplied)
            Debug.Log($"[InventorySlot] {item.itemName} ไม่มี effect");

        return effectApplied;
    }

    private bool EquipItem(Player player)
    {
        Debug.Log($"[InventorySlot] {item.itemName} ต้องสวมใส่ผ่านการลากไปที่ช่อง Equipment");
        return false;
    }

    // Helper — ดึง Local Player ได้จากทุกที่
    protected Player GetLocalPlayer()
    {
        if (NetworkManager.Singleton == null) return null;
        var localObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        return localObj != null ? localObj.GetComponent<Player>() : null;
    }

    #endregion

    #region Slot Management

    public void SwapSlot(InventorySlot newSlot)
    {
        ItemSO keepItem = item;
        int keepStack = stack;
        SetSwap(newSlot.item, newSlot.stack);
        newSlot.SetSwap(keepItem, keepStack);
    }

    public void SetSwap(ItemSO swapItem, int amount)
    {
        item = swapItem;
        stack = amount;
        icons.sprite = swapItem.icon;
        checkShowText();
        UpdateSelectionVisual();
    }

    public void MergeThisSlot(InventorySlot mergeSlot)
    {
        if (stack == item.maxStack || mergeSlot.stack == mergeSlot.item.maxStack)
        {
            SwapSlot(mergeSlot);
            return;
        }

        int total = stack + mergeSlot.stack;
        stack = Mathf.Clamp(total, 0, item.maxStack);
        checkShowText();

        int leftOver = total - stack;
        if (leftOver > 0)
            mergeSlot.SetThisSlot(mergeSlot.item, leftOver);
        else
            iventory.RemoveItem(mergeSlot);
    }

    public void MergeThisSlot(ItemSO mergeItem, int mergeAmount)
    {
        item = mergeItem;
        icons.sprite = mergeItem.icon;

        int total = stack + mergeAmount;
        stack = Mathf.Clamp(total, 0, item.maxStack);
        checkShowText();

        int leftOver = total - stack;
        if (leftOver > 0)
        {
            InventorySlot slot = iventory.IsEmptySlotLeft(mergeItem, this);
            if (slot == null)
                iventory.DropItem(mergeItem, leftOver);
            else
                slot.MergeThisSlot(mergeItem, leftOver);
        }
    }

    public virtual void SetThisSlot(ItemSO newItem, int amount)
    {
        item = newItem;
        icons.sprite = newItem.icon;

        int total = amount;
        stack = Mathf.Clamp(total, 0, newItem.maxStack);
        checkShowText();

        int leftOver = total - stack;
        if (leftOver > 0)
        {
            InventorySlot slot = iventory.IsEmptySlotLeft(newItem, this);
            if (slot != null)
                slot.SetThisSlot(newItem, leftOver);
        }

        UpdateSelectionVisual();
    }

    #endregion

    #region UI

    public void checkShowText()
    {
        UpdateColorSlot();
        stackText.text = stack.ToString();
        stackText.gameObject.SetActive(item.maxStack >= 2 && stack > 1);
    }

    public void UpdateColorSlot()
    {
        if (iventory == null || iventory.Empty_Item == null)
        {
            Debug.LogWarning("[InventorySlot] Inventory หรือ Empty_Item หายไป!");
            return;
        }
        if (icons == null)
        {
            Debug.LogWarning("[InventorySlot] Icons reference หายไป!");
            return;
        }

        bool isEmpty = (item == iventory.Empty_Item);
        icons.color = isEmpty ? emptyColor : itemColor;
        icons.gameObject.SetActive(!isEmpty);
    }

    #endregion
}