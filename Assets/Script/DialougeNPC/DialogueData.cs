using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData", menuName = "Game/DialogueData")]
public class DialogueData : ScriptableObject
{
    [Header("NPC Info")]
    public string npcName;
    public AudioClip npcVoiceSound;

    [Header("Conversation Steps")]
    public List<DialogueStep> steps = new List<DialogueStep>();

    [Header("Quest")]
    public QuestData questData;
    // ข้อความที่แสดงหลังรับเควส
    public string questAcceptText;
    // ข้อความที่แสดงเมื่อส่งเควสสำเร็จ
    public string questCompletedText;
}