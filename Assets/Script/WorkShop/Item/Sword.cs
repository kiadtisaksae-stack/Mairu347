using UnityEngine;

// Sword สือบทอดจาก Item
public class Sword : Item
{
    public override Equipment GetEquipment()
    {
        return Equipment.Weapon;
    }
    [SerializeField]
    public int baseDamage = 25;
    int Number = 0;
    public override void OnCollect(Player player)
    {
        // 1. ตรวจสอบ Server Authority
        if (!IsServer) return;
        base.OnCollect(player);
        player.Damage += baseDamage;
        player.AddItem(this);
        Inventory.Instance.ActivateRightHandWeapon(Number);
    }
    
}