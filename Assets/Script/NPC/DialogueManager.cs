using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager instance;

    [Header("UI References")]
    public GameObject dialogueCanvasPanel; // Panel
    public Image npcIcon;
    public Image playerIcon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;

    [Header("Button Objects")]
    public GameObject continueButton1; 
    public GameObject continueButton2;
    public Button questGiveBtn;
    public Button sendQuestBtn;

    public GameObject quitButton;

    [Header("Button Texts (Text ��١�ͧ����)")]
    public TextMeshProUGUI button1Text;
    public TextMeshProUGUI button2Text;

    [Header("Audio Settings")]
    public AudioSource audioSource;


    [HideInInspector] public bool isTalking = false;
    [HideInInspector] public bool isHaveQuest = false;

    [HideInInspector] public NPCInteractable currentNearbyNPC;

    private Queue<DialogueStep> stepsQueue;
    private AudioClip currentVoice;

    private void Awake()
    {
        if (instance == null) instance = this;
        stepsQueue = new Queue<DialogueStep>();
    }

    private void Start()
    {
        dialogueCanvasPanel.SetActive(false);

        quitButton.GetComponent<Button>().onClick.AddListener(EndDialogue);
        questGiveBtn.onClick.AddListener(QuestGiver);
        sendQuestBtn.onClick.AddListener(SendQuest);
    }
    public void QuestGiver()
    {
        QuestManager.Instance.StartQuest(currentNearbyNPC.questData);
        dialogueText.text = currentNearbyNPC.questDataText;
        
    }
    public void SendQuest()
    {
        QuestData[] newData = QuestManager.Instance.activeQuests.ToArray();
        if(newData.Any(q => q.questName == currentNearbyNPC.questData.questName))
        {
            QuestManager.Instance.CheckProgress(currentNearbyNPC.questData);
            if(currentNearbyNPC.questData.currentCount == currentNearbyNPC.questData.requestCount)
            {
                dialogueText.text = currentNearbyNPC.QuestcompletedText;
            }
            else
            {
                dialogueText.text = " Haha Try to Do Quest FF";
                
            }
        }
        else
        {
            dialogueText.text = " Check Your Quest & Progress";
        }
        
    }
    

    // --- for interact button ---
    public void PressInteractButton()
    {
        if (currentNearbyNPC != null && !isTalking)
        {
            currentNearbyNPC.TriggerDialogue();
        }
    }


    public void StartDialogue(string name, DialogueStep[] steps, AudioClip voice)
    {
        isTalking = true;
        dialogueCanvasPanel.SetActive(true);

        nameText.text = name;
        currentVoice = voice;

        stepsQueue.Clear();
        foreach (DialogueStep step in steps)
        {
            stepsQueue.Enqueue(step);
        }

        // Icon chatable NPC
        if (currentNearbyNPC != null) currentNearbyNPC.ShowIcon(false);

        DisplayNextStep();
    }

    public void DisplayNextStep()
    {
        if (stepsQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueStep currentStep = stepsQueue.Dequeue();

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentStep.npcText));

        // change sprite for different emotion
        if (currentStep.npcFace != null) npcIcon.sprite = currentStep.npcFace;
        if (currentStep.playerFace != null) playerIcon.sprite = currentStep.playerFace;
        npcIcon.preserveAspect = true;
        playerIcon.preserveAspect = true;

        // Quitable
        quitButton.SetActive(currentStep.canQuitHere);

        continueButton1.GetComponent<Button>().onClick.RemoveAllListeners();
        continueButton2.GetComponent<Button>().onClick.RemoveAllListeners();

        button1Text.text = currentStep.button1Text;
        button2Text.text = currentStep.button2Text;

        // end Conversation
        if (currentStep.endsConversation)
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
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;

            if (audioSource != null && currentVoice != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(currentVoice);
            }
            yield return new WaitForSeconds(0.03f); // Text speed
        }
    }

    public void EndDialogue()
    {
        isTalking = false;
        dialogueCanvasPanel.SetActive(false);

        if (currentNearbyNPC != null) currentNearbyNPC.ShowIcon(true);
    }
}