using UnityEngine;
using Unity.Netcode;

public class Potion : Item
{
    public int AmountHealth = 20;

    // ✅ 1. Default Constructor
    public Potion() 
    {
        // ตั้งค่าพื้นฐาน
        Name = "Health Potion";
    }

    public override void OnCollect(Player player)
    {
        base.OnCollect(player);

        if (player != null)
        {
            player.Heal(AmountHealth);

        }
        else
        {
            Debug.Log("Player is null when collecting potion.");
        }
        
    }

}