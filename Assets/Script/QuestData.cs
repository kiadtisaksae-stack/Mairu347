using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/QuestData Data")]
public class QuestData : ScriptableObject
{
    public string questName;
    public int requestCount;
    public int CurrentCount;
    
}
