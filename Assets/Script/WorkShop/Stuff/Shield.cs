using Unity.Collections;
using UnityEngine;

public class Shield : Item
{
    public override Equipment GetEquipment()
    {
        return Equipment.Shield;
    }
    [SerializeField]
    public int Diffent = 25;
    int Number = 0;
    public override void OnCollect(Player player)
    {
        // 1. ตรวจสอบ Server Authority
        if (!IsServer) return;
        base.OnCollect(player); 
        player.Deffent += Diffent;
        player.AddItem(this);
        Inventory.Instance.ActivateLeftHandWeapon(Number);
    }
}
