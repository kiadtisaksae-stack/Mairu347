using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
public class CollectableItem : Item
{
    public int value = 10;

    public override void OnCollect(Player player)
    {
        base.OnCollect(player);
        // ตัวอย่าง: เพิ่มคะแนนให้กับผู้เล่น
        player.AddItem(this);
        QuestManager questManager = player.GetComponent<QuestManager>(); // สมมติว่า QuestManager อยู่บน Player Object
        if (questManager != null)
        {
            questManager.TrackCollectItem(this.Name.ToString());
        }
    }

    
}