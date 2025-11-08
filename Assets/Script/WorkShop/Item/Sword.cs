using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

// Sword สือบทอดจาก Item
public class Sword : Item
{
    // ใช้ [SerializeField] เพื่อตั้งค่าใน Inspector
    [SerializeField]
    public int baseDamage = 25;
    public override void SetUP()
    {
        base.SetUP();
        Name = "Sword";
    }
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