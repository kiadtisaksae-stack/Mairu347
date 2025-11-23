using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueStep
{
    [Header("Dialogue")]
    [TextArea(3, 5)] public string npcText;

    [Header("Character Sprite")]
    public Sprite npcFace;    
    public Sprite playerFace;

    [Header("ButtonText")]
    public string button1Text; 
    public string button2Text;

    [Header("Quitable")]
    public bool canQuitHere;      // Quitable
    public bool endsConversation; // Endconversation
}

public class NPCInteractable : MonoBehaviour
{
    [Header("NPC Setting")]
    public string npcName;
    public AudioClip npcVoiceSound;

    [Header("speech Icon")]
    public GameObject speechIcon;

    [Header("DialogStep")]
    public DialogueStep[] conversationSteps;

    private void Start()
    {
        if (speechIcon != null) speechIcon.SetActive(false);
    }

    public void TriggerDialogue()
    {
        DialogueManager.instance.StartDialogue(npcName, conversationSteps, npcVoiceSound);
    }


    public void ShowIcon(bool isVisible)
    {
        if (speechIcon != null) speechIcon.SetActive(isVisible);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DialogueManager.instance.currentNearbyNPC = this;
            ShowIcon(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (DialogueManager.instance.currentNearbyNPC == this)
            {
                DialogueManager.instance.currentNearbyNPC = null;
            }
            ShowIcon(false);
        }
    }
}