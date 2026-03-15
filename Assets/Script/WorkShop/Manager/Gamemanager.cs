using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("GameManager instance is null! Is it in the scene?");
            return _instance;
        }
    }

    [Header("Player Stats")]
    public TextMeshProUGUI playerDamage;
    public TextMeshProUGUI playerDefence;

    [Header("Quest UI Slots")]
    public List<TextMeshProUGUI> questTextSlots = new List<TextMeshProUGUI>();
    public List<QuestData> questDatas = new List<QuestData>();

    [Header("Game State")]
    public int currentScore = 0;
    public bool isGamePaused = false;

    [Header("UI Game")]
    public GameObject pauseMenuUI;
    public GameObject exitConfirmationUI;
    public TMP_Text scoreText;
    public Slider HPBar;
    public InputSystem_Actions inputActions;
    public Button exitGameButton;
    public Button confirmExitButton;
    public Button cancelExitButton;

    public Slider Xpbar;
    public TMP_Text XpText;
    public TMP_Text LevelText;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupExitButtons();
    }

    private void OnEnable()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.Cancel.performed += ctx => TogglePause();
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.UI.Cancel.performed -= ctx => TogglePause();
        inputActions.Disable();
    }

    private void SetupExitButtons()
    {
        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
            exitGameButton.onClick.AddListener(ShowExitConfirmation);
        }
        if (confirmExitButton != null)
        {
            confirmExitButton.onClick.RemoveAllListeners();
            confirmExitButton.onClick.AddListener(ConfirmExitGame);
        }
        if (cancelExitButton != null)
        {
            cancelExitButton.onClick.RemoveAllListeners();
            cancelExitButton.onClick.AddListener(HideExitConfirmation);
        }
    }

    public void ShowExitConfirmation()
    {
        if (exitConfirmationUI != null)
        {
            exitConfirmationUI.SetActive(true);
            if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        }
    }

    public void HideExitConfirmation()
    {
        if (exitConfirmationUI != null)
            exitConfirmationUI.SetActive(false);
    }

    public void ConfirmExitGame()
    {
        HideExitConfirmation();
        if (pauseMenuUI != null) pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isGamePaused = false;

        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
                ExitAsHost();
            else if (NetworkManager.Singleton.IsClient)
                ExitAsClient();
            else
                ExitAsSinglePlayer();
        }
        else
        {
            ExitAsSinglePlayer();
        }
    }

    private void ExitAsHost()
    {
        Debug.Log("Exiting as Host...");

        // แก้ปัญหา #20 — ลบ NetworkManager.Singleton.Shutdown() ออกจากตรงนี้
        // เดิมเรียก Shutdown ที่นี่ แล้ว ShutdownAfterDelay ก็เรียกอีกครั้ง → Shutdown 2 รอบ
        // ให้ NotifyClientsBeforeShutdown → ShutdownAfterDelay เป็นคนเรียก Shutdown แทน

        NotifyClientsBeforeShutdown();

        // Application.Quit จะถูกเรียกหลัง Shutdown เสร็จใน ShutdownAfterDelay
    }

    private void ExitAsClient()
    {
        Debug.Log("Exiting as Client...");
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ExitAsSinglePlayer()
    {
        Debug.Log("Exiting as Single Player...");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void NotifyClientsBeforeShutdown()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NotifyShutdownClientRpc();
            StartCoroutine(ShutdownAfterDelay(2f));
        }
    }

    [ClientRpc]
    private void NotifyShutdownClientRpc()
    {
        Debug.Log("Server is shutting down...");
    }

    private IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Shutdown ครั้งเดียวที่นี่เท่านั้น
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        ReturnToLobby();

        // Quit หลัง Shutdown และ Return to Lobby
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ReturnToLobby()
    {
        if (SceneTransitionHandler.Instance != null)
            SceneTransitionHandler.Instance.GoToLobbyScene();
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");

        CleanupBeforeExit();
    }

    private void CleanupBeforeExit()
    {
        currentScore = 0;
        isGamePaused = false;
        questDatas.Clear();
        RefreshQuestUI();
    }

    public void OnExitButtonPressed() => ShowExitConfirmation();

    public void HandleNetworkDisconnect()
    {
        Time.timeScale = 1f;
        isGamePaused = false;
        ReturnToLobby();
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
                questDatas.RemoveAt(i);
        }
        RefreshQuestUI();
    }

    public void RefreshQuestUI()
    {
        for (int i = 0; i < questTextSlots.Count; i++)
        {
            if (i < questDatas.Count)
            {
                // แก้ให้ sync progress จาก QuestManager ก่อน refresh
                if (QuestManager.Instance != null)
                    QuestManager.Instance.SyncProgressToQuestData(questDatas[i]);

                questTextSlots[i].text = questDatas[i].questName + " (" +
                                        questDatas[i].currentCount + "/" +
                                        questDatas[i].requestCount + ")";
                questTextSlots[i].gameObject.SetActive(true);
            }
            else
            {
                questTextSlots[i].text = "None Quest";
            }
        }
    }

    public void UpdateStatus(int damage, int defence)
    {
        if (playerDamage != null) playerDamage.text = damage.ToString();
        if (playerDefence != null) playerDefence.text = defence.ToString();
    }

    public void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        if (HPBar != null)
        {
            HPBar.maxValue = maxHealth;
            HPBar.value = currentHealth;
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        if (scoreText != null) scoreText.text = currentScore.ToString();
    }

    public void TogglePause()
    {
        if (exitConfirmationUI != null && exitConfirmationUI.activeInHierarchy) return;

        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0f : 1f;
        if (pauseMenuUI != null) pauseMenuUI.SetActive(isGamePaused);
    }

    public void UpdateXpbar(int currentXP, int XpRequire)
    {
        if (Xpbar != null)
        {
            Xpbar.maxValue = XpRequire;
            Xpbar.value = currentXP;
        }
        if (XpText != null) XpText.text = currentXP + "/" + XpRequire;
    }

    public void UpdateLevel(int level)
    {
        if (LevelText != null) LevelText.text = "Level : " + level;
    }
}