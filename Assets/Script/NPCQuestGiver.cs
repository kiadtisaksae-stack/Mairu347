
public class NPCQuestGiver : Character, IInteractable
{
    public QuestData questToGive;
    public bool canTalk = true;
    public bool isInteractable { get => canTalk; set => canTalk = value; }

    public void Interact(Player player)
    {
        QuestManager.Instance.StartQuest(questToGive);
    }
}
