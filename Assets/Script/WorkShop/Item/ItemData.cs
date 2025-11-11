using UnityEngine;

[System.Serializable]
public class ItemData
{
    public Equipment EquipmentTybe;
    public string Name;
    public int ItemID;
    public Sprite sprite;
    // เพิ่มคุณสมบัติอื่นๆ เช่น Icon, Type, Value

    // Constructor เพื่อสร้างข้อมูลจาก Item Component
    public ItemData(Item itemComponent)
    {
        this.Name = itemComponent.Name;
        this.sprite = itemComponent.sprite;
        this.EquipmentTybe = itemComponent.GetEquipment();
        // กำหนดค่าอื่นๆ ตามต้องการ
    }
}
