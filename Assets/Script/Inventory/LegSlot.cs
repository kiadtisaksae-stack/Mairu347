// 📁 LegSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class LegSlot : InventorySlot
{
    // Override OnDrop: อนุญาตให้วางเฉพาะไอเท็มประเภท LEGS (Type ID = 4)
    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot.item != null)
        {
            // ตรวจสอบ tybe (ใช้ ItemTypes.LEGS ซึ่งเท่ากับ 4)
            if (sourceSlot.item.tybe == ItemTypes.LEGS)
            {
                base.OnDrop(eventData);
            }
            else
            {
                Debug.Log("ไม่สามารถสวมใส่ไอเท็มนี้ในช่อง Legs ได้ (ต้องการ Type: " + ItemTypes.LEGS + ")");
            }
        }
    }

    // Override UseItem: ถอดอุปกรณ์กลับเข้า Inventory
    public override void UseItem()
    {
        if (item != iventory.Empty_Item)
        {
            iventory.AddItem(item, stack);
            iventory.RemoveItem(this); 
            DeselectThisSlot();
            // (ถ้ามี) iventory.playerController.UnequipLeg();
        }
    }
}