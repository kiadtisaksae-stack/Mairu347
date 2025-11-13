using Unity.Netcode;
using UnityEngine;
using Unity.Collections;
using System.Runtime.InteropServices.WindowsRuntime;

// Sword สือบทอดจาก Item
public class Sword : Item
{
    public override Equipment GetEquipment()
    {
        return Equipment.Weapon;
    }
    [SerializeField]
    public int baseDamage = 25;
    public override void OnCollect(Player player)
    {
        // 1. ตรวจสอบ Server Authority
        if (!IsServer) return;
        base.OnCollect(player);
        player.Damage += baseDamage; 
        player.EquipItemVisualClientRpc(new FixedString32Bytes(Name));
        player.AddItem(this); 
    }
    
}