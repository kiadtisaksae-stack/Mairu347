using UnityEngine;

public class NPCInteractable : MonoBehaviour, IInteractable
{
    [Header("Dialogue Data")]
    public DialogueData dialogueData;

    [Header("Speech Icon")]
    public GameObject speechIcon;

    // IInteractable
    public bool isInteractable => true;

    bool IInteractable.isInteractable { get => isInteractable; set => throw new System.NotImplementedException(); }

    private void Start()
    {
        if (speechIcon != null) speechIcon.SetActive(false);

        if (dialogueData == null)
            Debug.LogWarning($"[NPC] {gameObject.name} ไม่มี DialogueData!");
    }

    public void Interact(Player player)
    {
        TriggerDialogue();
    }

    public void TriggerDialogue()
    {
        if (dialogueData == null) return;
        DialogueManager.instance.StartDialogue(this);
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
                DialogueManager.instance.currentNearbyNPC = null;

            ShowIcon(false);
            DialogueManager.instance.EndDialogue();
        }
    }
}