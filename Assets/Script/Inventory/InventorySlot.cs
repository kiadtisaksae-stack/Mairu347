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


    // สำหรับ double click
    private float lastClickTime;
    private const float doubleClickThreshold = 0.3f;

    // สำหรับ multi-selection
    public bool isSelected = false;
    private InputSystem_Actions inputActions;
    private RectTransform canvasRect;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
        siblingIndex = transform.GetSiblingIndex();
        if (canvas != null)
        {
            canvasRect = canvas.GetComponent<RectTransform>();
        }

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
        inputActions.Disable();
    }

    void Update() { }

    #region Drag and Drop Methods
    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetAsLastSibling();
        iventory.SetLayoutControlChiad(false);
        iventory.MakeThisToTopLayer(true, 2);

        // ให้ draggable ติดเมาส์ตรง ๆ
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out localPoint
        );
        draggable.anchoredPosition = localPoint;
    }

    public virtual void OnDrag(PointerEventData eventData)
    {
        // ✅ วิธีง่ายๆ: ตั้งตำแหน่งตรงๆ
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
        if (eventData.pointerDrag != null)
        {
            InventorySlot slot = eventData.pointerDrag.GetComponent<InventorySlot>();
            if (slot != null)
            {
                if (slot.item == item)
                {
                    MergeThisSlot(slot);
                }
                else
                {
                    SwapSlot(slot);
                }
            }
        }
        var localObj = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
        if (localObj != null)
        {
            Player player = localObj.GetComponent<Player>();
            if (player != null)
            {
                player.EquipBody();
                player.EquipWeapon();
                player.EquipLeg();
                player.EquipHead();
            }
        }



    }
    #endregion

    #region Click and Selection Methods
    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (item == iventory.Empty_Item)
            return;

        // ตรวจสอบ double click
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (Time.time - lastClickTime < doubleClickThreshold)
            {
                OnPointerDoubleClick(eventData);
            }
            else
            {
                // Single click - select/deselect
                if (inputActions != null && inputActions.UI.RightClick.IsPressed())
                {
                    ToggleSelection();
                }
                else
                {
                    if (isSelected && iventory.selectedSlots.Count == 1)
                    {
                        DeselectThisSlot();
                    }
                    else
                    {
                        SelectThisSlot();
                    }
                }
                inputActions.Disable();
            }
            lastClickTime = Time.time;
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            iventory.ClearAllSelections();
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, false); // แจ้งเตือนเท่านั้น
        }
    }

    public void OnPointerDoubleClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && item != iventory.Empty_Item)
        {
            UseItem();
        }
    }

    public void ToggleSelection()
    {
        isSelected = !isSelected;
        UpdateSelectionVisual();
        iventory.UpdateSlotSelection(this, isSelected); // แจ้งเตือนเท่านั้น
    }

    public void SelectThisSlot()
    {
        if (!isSelected)
        {
            isSelected = true;
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, true); // แจ้งเตือนเท่านั้น ไม่เรียก recursive
        }
    }

    public void DeselectThisSlot()
    {
        if (isSelected)
        {
            isSelected = false;
            UpdateSelectionVisual();
            iventory.UpdateSlotSelection(this, false); // แจ้งเตือนเท่านั้น ไม่เรียก recursive
        }
    }

    public void UpdateSelectionVisual()
    {
        if (outline != null)
        {
            outline.enabled = isSelected;

            // สามารถปรับแต่งเพิ่มเติมเมื่อเลือก
            if (isSelected)
            {
                outline.effectColor = selectedColor;
                outline.effectDistance = new Vector2(3, 3);
            }
        }

        // เปลี่ยนสี background เพิ่มเติม (optional)
        if (background != null)
        {
            background.color = isSelected ? backgroundColor : Color.white;
        }
    }

    public bool IsSelected()
    {
        return isSelected;
    }
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
            // ตรวจสอบว่ามี slot ที่เลือกมากกว่าหนึ่งชิ้นหรือไม่
            if (iventory.HasMultipleSelections())
            {
                iventory.DeleteSelectedItems();
            }
            else
            {
                // ลบแค่ชิ้นเดียว
                iventory.RemoveItem(this);
            }
            iventory.UpdateButtonInteractability();
        }
    }
    #endregion

    #region Item Management
    public virtual void UseItem()
    {
        if (item == iventory.Empty_Item) return;

        stack = Mathf.Clamp(stack - 1, 0, item.maxStack);
        if (stack > 0)
        {
            checkShowText();
        }
        else
        {
            iventory.RemoveItem(this);
        }
        DeselectThisSlot();
    }

    public void SwapSlot(InventorySlot newSlot)
    {
        ItemSO keepItem = item;
        int keepstack = stack;

        SetSwap(newSlot.item, newSlot.stack);
        newSlot.SetSwap(keepItem, keepstack);
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

        int ItemAmount = stack + mergeSlot.stack;
        int intInthisSlot = Mathf.Clamp(ItemAmount, 0, item.maxStack);
        stack = intInthisSlot;

        checkShowText();
        int amountLeft = ItemAmount - intInthisSlot;
        if (amountLeft > 0)
        {
            mergeSlot.SetThisSlot(mergeSlot.item, amountLeft);
        }
        else
        {
            iventory.RemoveItem(mergeSlot);
        }
    }

    public void MergeThisSlot(ItemSO mergeItem, int mergeAmount)
    {
        item = mergeItem;
        icons.sprite = mergeItem.icon;
        int ItemAmount = stack + mergeAmount;

        int intInthisSlot = Mathf.Clamp(ItemAmount, 0, item.maxStack); //า itemAmout ว่าเกืน newItem มั้ย ท่าเกินตัดออก
        stack = intInthisSlot;

        checkShowText();
        int amountLeft = ItemAmount - intInthisSlot;//เช็คว่า ไอเทมเกินช่ิงมั้ย
        if (amountLeft > 0)//เกินเท่าไหร่
        {
            InventorySlot slot = iventory.IsEmptySlotLeft(mergeItem, this);
            if (slot == null)
            {
                iventory.DropItem(mergeItem, amountLeft);
                return;
            }
            else
            {
                slot.MergeThisSlot(mergeItem, amountLeft);//รีเคอซีพ
            }
        }
        else
        {

        }
    }

    public virtual void SetThisSlot(ItemSO newItem, int amount)
    {
        item = newItem;
        icons.sprite = newItem.icon;

        int ItemAmount = amount;
        int intInthisSlot = Mathf.Clamp(ItemAmount, 0, newItem.maxStack);
        stack = intInthisSlot;

        checkShowText();
        int amountLeft = ItemAmount - intInthisSlot;
        if (amountLeft > 0)
        {
            InventorySlot slot = iventory.IsEmptySlotLeft(newItem, this);
            if (slot == null)
            {
                return;
            }
            else
            {
                slot.SetThisSlot(newItem, amountLeft);
            }
        }
        UpdateSelectionVisual();
    }
    #endregion

    #region UI Methods
    public void checkShowText()
    {
        UpdateColorSlot();
        stackText.text = stack.ToString();
        if (item.maxStack < 2)
        {
            stackText.gameObject.SetActive(false);
        }
        else
        {
            stackText.gameObject.SetActive(stack > 1);
        }
    }

    public void UpdateColorSlot()
    {
        if (iventory == null || iventory.Empty_Item == null)
        {
            Debug.LogWarning("Inventory or Empty_Item reference is missing!");
            return;
        }

        if (icons == null)
        {
            Debug.LogWarning("Icons reference is missing!");
            return;
        }

        icons.color = (item == iventory.Empty_Item) ? emptyColor : itemColor;
        icons.gameObject.SetActive(item != iventory.Empty_Item);
    }
    #endregion
}