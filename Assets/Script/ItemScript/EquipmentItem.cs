using UnityEngine;

public class EquipmentItem : MonoBehaviour
{
    public ItemSO item; // เก็บ ItemSO ของไอเท็มนี้

    // ตัวช่วยเข้าถึงข้อมูลง่าย ๆ
    public int Id => item != null ? item.id : -1;
    public int Type => item != null ? item.tybe : -1;
    public int Damage => item != null ? item.Damage : 0;
    public int Defence => item != null ? item.Deffent : 0;
}