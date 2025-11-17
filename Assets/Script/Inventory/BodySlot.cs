// 📁 BodySlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class BodySlot : InventorySlot
{
    // Override OnDrop เพื่อให้วางเฉพาะไอเท็มประเภท Body
    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot.item != null)
        {
            // **🛑 เปลี่ยนการตรวจสอบจาก enum เป็น int ที่กำหนดไว้ใน ItemTypes**
            if (sourceSlot.item.tybe == ItemTypes.BODY)
            {
                base.OnDrop(eventData); // ดำเนินการ Swap/Merge ปกติ
            }
            else
            {
                Debug.Log("ไม่สามารถสวมใส่ไอเท็มนี้ในช่อง Body ได้ (Type ID: " + sourceSlot.item.tybe + ")");
            }
        }
    }

    // Override UseItem เพื่อจัดการการถอดอุปกรณ์ (Logic ส่วนนี้ยังคงใช้ได้)
    public override void UseItem()
    {
        if (item != iventory.Empty_Item)
        {
            // 1. นำไอเท็มกลับเข้า Inventory หลัก
            iventory.AddItem(item, stack);
            
            // 2. Clear ช่องอุปกรณ์นี้
            iventory.RemoveItem(this); 
            
            // 3. (ถ้ามี) แจ้ง PlayerController ให้ถอดโมเดล/สถิติ
            // iventory.playerController.UnequipBody(); 

            DeselectThisSlot();
        }
    }
}