using UnityEngine;

[System.Serializable]
public class DialogueStep
{
    [Header("Dialogue")]
    [TextArea(3, 5)] public string npcText;

    [Header("Character Sprite")]
    public Sprite npcFace;
    public Sprite playerFace;

    [Header("Button Text")]
    public string button1Text;
    public string button2Text;

    [Header("Flags")]
    public bool canQuitHere;
    public bool endsConversation;
}