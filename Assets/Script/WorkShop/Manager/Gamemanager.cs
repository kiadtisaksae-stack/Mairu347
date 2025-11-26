using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// ¡ÓË¹´ãËéà»ç¹ sealed à¾×èÍ»éÍ§¡Ñ¹¡ÒÃÊ×º·Í´
public sealed class GameManager : MonoBehaviour 
{
    // 1. Private Static Field (The Singleton Instance)
    // ãªé backing field à¾×èÍ¤Çº¤ØÁ¡ÒÃà¢éÒ¶Ö§
    private static GameManager _instance;

    // 2. Public Static Property (Global Access Point)
    public static GameManager Instance
    {
        get
        {
            // ¶éÒ Instance ÂÑ§à»ç¹ null (¡Ã³Õ¶Ù¡àÃÕÂ¡ãªé¡èÍ¹ Awake)
            if (_instance == null)
            {
                Debug.LogError("GameManager instance is null! Is it in the scene?");
            }
            return _instance;
        }
    }
    [Header("Player Stats")]
    public TextMeshProUGUI playerDamage;
    public TextMeshProUGUI playerDefence;
    [Header("Quest UI Slots")]
    public List<TextMeshProUGUI> questTextSlots = new List<TextMeshProUGUI>();
    public List< QuestData > questDatas = new List<QuestData>();
    [Header("Game State")]
    public int currentScore = 0;
    public bool isGamePaused = false;

    [Header("UI Game")]
    public GameObject pauseMenuUI;
    public TMP_Text scoreText;
    public Slider HPBar;
    public InputSystem_Actions inputActions;


    public Slider Xpbar;
    public TMP_Text XpText;
    public TMP_Text LevelText;

    // 3. Private Constructor Logic (ãªé Awake() á·¹ Constructor »¡µÔã¹ Unity)
    private void Awake()
    {
        // µÃÇ¨ÊÍºÇèÒÁÕ Instance ÍÂÙèáÅéÇËÃ×ÍäÁè
        if (_instance == null)
        {
            // ¡ÓË¹´ãËé Instance ¹Õéà»ç¹ Singleton
            _instance = this;

            // »éÍ§¡Ñ¹äÁèãËé Object ¹Õé¶Ù¡·ÓÅÒÂàÁ×èÍÁÕ¡ÒÃâËÅ´ Scene ãËÁè
            DontDestroyOnLoad(gameObject);

            Debug.Log("GameManager Singleton Initialized.");
        }
        else
        {
            // ¶éÒÁÕ Instance Í×è¹ÍÂÙèáÅéÇ (ÁÒ¨Ò¡ Scene ¡èÍ¹Ë¹éÒ) ãËé·ÓÅÒÂµÑÇàÍ§·Ôé§
            Debug.Log("Duplicate GameManager found. Destroying self.");
            Destroy(gameObject);
        }
    }

    // ------------------- Singleton Functionality -------------------
    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Cancel.performed += ctx => TogglePause();
    }
    public void UpdateQuestUI(QuestData questData)
    {
        questDatas.Add(questData);
        RefreshQuestUI();
    }
    public void ClearQuest(QuestData questData)
    {
        for (int i = questDatas.Count - 1; i >= 0; i--)
        {
            if (questDatas[i].questName == questData.questName)
            {
                questDatas.RemoveAt(i);
            }
        }

        RefreshQuestUI();
    }
    public void RefreshQuestUI()
    {
        // วนลูปช่อง UI ตามจำนวนที่มี
        for (int i = 0; i < questTextSlots.Count; i++)
        {
            if (i < questDatas.Count)
            {
                // แสดงข้อความเควสตามลำดับ index
                questTextSlots[i].text = questDatas[i].questName + " (" +
                                        questDatas[i].currentCount + "/" +
                                        questDatas[i].requestCount + ")";
                questTextSlots[i].gameObject.SetActive(true);
            }
            else
            {
                // ถ้าไม่มีเควสในช่องนี้ ให้ซ่อน
                questTextSlots[i].text = "None Quest";
            }
        }
    }
    public void UpdateStatus(int damage, int defence)
    {
        if (playerDamage != null)
        {
            playerDamage.text = damage.ToString();
        }
        if (playerDefence != null)
        {
            playerDefence.text = defence.ToString();
        }
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (HPBar != null)
        {
            HPBar.maxValue = maxHealth;
            HPBar.value = currentHealth;
            Debug.Log($"Health updated: {currentHealth}/{maxHealth}");
        }
        else
        {
            Debug.LogWarning("HPBar reference is missing in GameManager.");
        }
    }
    public void AddScore(int amount)
    {
        currentScore += amount;
        scoreText.text = currentScore.ToString();
        Debug.Log($"Score updated: {currentScore}");
        // â¤é´ÊÓËÃÑºÍÑ»à´µ UI, ºÑ¹·Ö¡¤Ðá¹¹ ÏÅÏ
    }

    public void TogglePause()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(isGamePaused);
        }
        Debug.Log($"Game Paused: {isGamePaused}");
    }

    public void UpdateXpbar(int currentXP, int XpRequire)
    {
        Xpbar.maxValue = XpRequire;
        Xpbar.value = currentXP;
        XpText.text = currentXP + "/" + XpRequire; 
    }

    public void UpdateLevel(int level)
    {
        LevelText.text = "Level : " + level;
    }

    public void Update()
    {
        
    }

}