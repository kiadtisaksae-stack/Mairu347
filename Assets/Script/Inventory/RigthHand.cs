// 📁 RightHandSlot.cs
using UnityEngine;
using UnityEngine.EventSystems;

public class RigthHand : InventorySlot // **ชื่อคลาสที่ถูกต้อง**
{
    // Override OnDrop: อนุญาตให้วางอาวุธมือเดียว (Type ID = 5) หรือสองมือ (Type ID = 6)
    public override void OnDrop(PointerEventData eventData)
    {
        InventorySlot sourceSlot = eventData.pointerDrag.GetComponent<InventorySlot>();
        if (sourceSlot != null && sourceSlot.item != null)
        {
            // ตรวจสอบ tybe (อนุญาต อาวุธมือเดียว หรือ สองมือ)
            if (sourceSlot.item.tybe == ItemTypes.ONE_HAND_WEAPON || sourceSlot.item.tybe == ItemTypes.TWO_HAND_WEAPON)
            {
                base.OnDrop(eventData);
            }
            else
            {
                Debug.Log("ไม่สามารถสวมใส่อาวุธนี้ในมือขวาได้");
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
            // (ถ้ามี) iventory.playerController.UnequipRightHand();
        }
    }
}