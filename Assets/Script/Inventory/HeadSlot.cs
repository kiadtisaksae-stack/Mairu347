// 📁 HeadSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class HeadSlot : InventorySlot
{
    // Override OnDrop: อนุญาตให้วางเฉพาะไอเท็มประเภท HEAD (Type ID = 2)
    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot.item != null)
        {
            // ตรวจสอบ tybe (ใช้ ItemTypes.HEAD ซึ่งเท่ากับ 2)
            if (sourceSlot.item.tybe == ItemTypes.HEAD)
            {
                base.OnDrop(eventData); // ดำเนินการ Swap/Merge ปกติ
            }
            else
            {
                Debug.Log("ไม่สามารถสวมใส่ไอเท็มนี้ในช่อง Head ได้ (ต้องการ Type: " + ItemTypes.HEAD + ")");
            }
        }
    }

    // Override UseItem: ถอดอุปกรณ์กลับเข้า Inventory
    public override void UseItem()
    {
        if (item != iventory.Empty_Item)
        {
            // นำไอเท็มกลับเข้า Inventory หลัก
            iventory.AddItem(item, stack);
            
            // Clear ช่องอุปกรณ์นี้
            iventory.RemoveItem(this); 
            
            DeselectThisSlot();
            // (ถ้ามี) iventory.playerController.UnequipHead();
        }
    }
}