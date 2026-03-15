using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI References")]
    public GameObject dialogueCanvasPanel;
    public Image npcIcon;
    public Image playerIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Buttons")]
    public GameObject continueButton1;
    public GameObject continueButton2;
    public Button questGiveBtn;
    public Button sendQuestBtn;
    public GameObject quitButton;

    [Header("Button Texts")]
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;

    [Header("Audio")]
    public AudioSource audioSource;

    [HideInInspector] public bool isTalking = false;
    [HideInInspector] public NPCInteractable currentNearbyNPC;

    private Queue<DialogueStep> stepsQueue = new Queue<DialogueStep>();
    private AudioClip currentVoice;

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    private void Start()
    {
        dialogueCanvasPanel.SetActive(false);
        quitButton.GetComponent<Button>().onClick.AddListener(EndDialogue);
        questGiveBtn.onClick.AddListener(OnQuestGiveClicked);
        sendQuestBtn.onClick.AddListener(OnSendQuestClicked);
    }

    // ─── เปิด Dialogue จาก NPC ────────────────────────────
    public void StartDialogue(NPCInteractable npc)
    {
        if (npc.dialogueData == null) return;

        isTalking = true;
        dialogueCanvasPanel.SetActive(true);

        DialogueData data = npc.dialogueData;
        nameText.text = data.npcName;
        currentVoice = data.npcVoiceSound;

        stepsQueue.Clear();
        foreach (var step in data.steps)
            stepsQueue.Enqueue(step);

        npc.ShowIcon(false);
        RefreshQuestButtons();
        DisplayNextStep();
    }

    // ─── Quest Buttons ─────────────────────────────────────
    private void OnQuestGiveClicked()
    {
        if (currentNearbyNPC?.dialogueData?.questData == null) return;

        QuestManager.Instance.StartQuest(currentNearbyNPC.dialogueData.questData);
        dialogueText.text = currentNearbyNPC.dialogueData.questAcceptText;
        RefreshQuestButtons();
    }

    private void OnSendQuestClicked()
    {
        if (currentNearbyNPC?.dialogueData?.questData == null) return;

        QuestData quest = currentNearbyNPC.dialogueData.questData;
        bool delivered = QuestManager.Instance.TryDeliverQuest(quest);

        if (delivered)
        {
            dialogueText.text = currentNearbyNPC.dialogueData.questCompletedText;
        }
        else
        {
            int progress = QuestManager.Instance.GetProgress(quest);
            dialogueText.text = $"ยังทำไม่เสร็จนะ ({progress}/{quest.requestCount})";
        }

        RefreshQuestButtons();
    }

    public void RefreshQuestButtons()
    {
        if (currentNearbyNPC?.dialogueData?.questData == null)
        {
            questGiveBtn.gameObject.SetActive(false);
            sendQuestBtn.gameObject.SetActive(false);
            return;
        }

        QuestData quest = currentNearbyNPC.dialogueData.questData;
        bool isCompleted = QuestManager.Instance.IsQuestCompleted(quest);
        bool isPending = QuestManager.Instance.IsPendingDelivery(quest);
        bool isActive = QuestManager.Instance.IsQuestActive(quest);

        if (isCompleted)
        {
            questGiveBtn.gameObject.SetActive(false);
            sendQuestBtn.gameObject.SetActive(false);
        }
        else if (isPending)
        {
            questGiveBtn.gameObject.SetActive(false);
            sendQuestBtn.gameObject.SetActive(true);
        }
        else if (isActive)
        {
            questGiveBtn.gameObject.SetActive(false);
            sendQuestBtn.gameObject.SetActive(quest.requireDelivery);
        }
        else
        {
            questGiveBtn.gameObject.SetActive(true);
            sendQuestBtn.gameObject.SetActive(false);
        }
    }

    // ─── Dialogue Steps ────────────────────────────────────
    public void PressInteractButton()
    {
        if (currentNearbyNPC != null && !isTalking)
            currentNearbyNPC.TriggerDialogue();
    }

    public void DisplayNextStep()
    {
        if (stepsQueue.Count == 0) { EndDialogue(); return; }

        DialogueStep step = stepsQueue.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(step.npcText));

        if (step.npcFace != null) npcIcon.sprite = step.npcFace;
        if (step.playerFace != null) playerIcon.sprite = step.playerFace;
        npcIcon.preserveAspect = true;
        playerIcon.preserveAspect = true;

        quitButton.SetActive(step.canQuitHere);

        continueButton1.GetComponent<Button>().onClick.RemoveAllListeners();
        continueButton2.GetComponent<Button>().onClick.RemoveAllListeners();

        button1Text.text = step.button1Text;
        button2Text.text = step.button2Text;

        if (step.endsConversation)
        {
            continueButton1.SetActive(true);
            continueButton2.SetActive(false);
            continueButton1.GetComponent<Button>().onClick.AddListener(EndDialogue);
        }
        else
        {
            continueButton1.SetActive(true);
            continueButton2.SetActive(true);
            continueButton1.GetComponent<Button>().onClick.AddListener(DisplayNextStep);
            continueButton2.GetComponent<Button>().onClick.AddListener(DisplayNextStep);
        }
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence)
        {
            dialogueText.text += letter;
            if (audioSource != null && currentVoice != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(currentVoice);
            }
            yield return new WaitForSeconds(0.03f);
        }
    }

    public void EndDialogue()
    {
        isTalking = false;
        dialogueCanvasPanel.SetActive(false);
        if (currentNearbyNPC != null) currentNearbyNPC.ShowIcon(true);
    }
}