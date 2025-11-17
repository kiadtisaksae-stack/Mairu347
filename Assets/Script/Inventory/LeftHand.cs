// 📁 LeftHandSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class LeftHand : InventorySlot // **ชื่อคลาสที่ถูกต้อง**
{
    // Override OnDrop: อนุญาตให้วางอาวุธมือเดียว (Type ID = 5) หรือ โล่ (สมมติว่า Type ID = 7 หรือตามที่คุณกำหนด)
    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot.item != null)
        {
            // ตรวจสอบ tybe (อนุญาต อาวุธมือเดียว หรือ โล่)
            // ***หมายเหตุ: ถ้าคุณต้องการใช้โล่ คุณต้องเพิ่มค่าคงที่สำหรับ SHIELD ใน ItemTypes.cs***
            if (sourceSlot.item.tybe == ItemTypes.ONE_HAND_WEAPON /* || sourceSlot.item.tybe == ItemTypes.SHIELD */)
            {
                base.OnDrop(eventData);
            }
            else
            {
                Debug.Log("ไม่สามารถสวมใส่ไอเท็มนี้ในมือซ้ายได้");
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
            // (ถ้ามี) iventory.playerController.UnequipLeftHand();
        }
    }
}