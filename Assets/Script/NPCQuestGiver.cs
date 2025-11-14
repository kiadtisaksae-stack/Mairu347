using UnityEngine;

public class NPCQuestGiver : Character, IInteractable
{
    public QuestData questToGive;
    public bool canTalk = true;
    public bool isInteractable { get => canTalk; set => canTalk = value; }

    public void Interact(Player player)
    {
        DialogNPC();
        QuestManager.Instance.StartQuest(questToGive);
    }
    // Player เก็บของ / ฆ่ามอน / ทำกิจกรรม
    //QuestManager.Instance.AddProgress(questToGive, 1); 

    public void AddProgress(QuestData quest, int amount = 1)
    {
        
    }
    public void DialogNPC()
    {
        Debug.Log("ascaswfkjaehfkeafue");
        
    }


}
