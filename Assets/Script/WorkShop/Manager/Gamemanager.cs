using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.Netcode;

public sealed class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
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
    public List<QuestData> questDatas = new List<QuestData>();

    [Header("Game State")]
    public int currentScore = 0;
    public bool isGamePaused = false;

    [Header("UI Game")]
    public GameObject pauseMenuUI;
    public GameObject exitConfirmationUI; // ✅ เพิ่ม UI ยืนยันการออกเกม
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
            Debug.Log("GameManager Singleton Initialized.");
        }
        else
        {
            Debug.Log("Duplicate GameManager found. Destroying self.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // ✅ ตั้งค่า Event Listeners สำหรับปุ่ม Exit
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

    // ✅ ตั้งค่า Event Listeners สำหรับปุ่ม Exit
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

    // ✅ แสดงหน้าต่างยืนยันการออกเกม
    public void ShowExitConfirmation()
    {
        if (exitConfirmationUI != null)
        {
            exitConfirmationUI.SetActive(true);
            if (pauseMenuUI != null)
            {
                pauseMenuUI.SetActive(false);
            }
            Debug.Log("Exit confirmation shown");
        }
    }

    // ✅ ซ่อนหน้าต่างยืนยันการออกเกม
    public void HideExitConfirmation()
    {
        if (exitConfirmationUI != null)
        {
            exitConfirmationUI.SetActive(false);
            Debug.Log("Exit confirmation hidden");
        }
    }

    // ✅ ยืนยันการออกเกม - จัดการทั้ง Host และ Client
    public void ConfirmExitGame()
    {
        Debug.Log("Confirming exit game...");

        // ✅ ปิด UI
        HideExitConfirmation();
        if (pauseMenuUI != null)
        {
            pauseMenuUI.SetActive(false);
        }

        // ✅ คืนค่า Time Scale
        Time.timeScale = 1f;
        isGamePaused = false;

        // ✅ จัดการ Netcode ตามบทบาท
        if (NetworkManager.Singleton != null)
        {
            if (NetworkManager.Singleton.IsServer)
            {
                ExitAsHost();
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                ExitAsClient();
            }
            else
            {
                ExitAsSinglePlayer();
            }
        }
        else
        {
            ExitAsSinglePlayer();
        }
    }

    // ✅ ออกเกมในฐานะ Host (ปิดทั้ง Server)
    private void ExitAsHost()
    {
        Debug.Log("Exiting as Host...");

        // ✅ ส่ง notification ไปยัง clients ก่อน (optional)
        NotifyClientsBeforeShutdown();

        // ✅ ปิด NetworkManager
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("NetworkManager shut down");
        }

        Application.Quit();

            // สำหรับ Editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ✅ ออกเกมในฐานะ Client
    private void ExitAsClient()
    {
        Debug.Log("Exiting as Client...");

        // ✅ ปิดการเชื่อมต่อ
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Client disconnected");
        }

        Application.Quit();

        // สำหรับ Editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ✅ ออกเกมในโหมด Single Player
    private void ExitAsSinglePlayer()
    {
        Debug.Log("Exiting as Single Player...");
        Application.Quit();

        // สำหรับ Editor
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // ✅ แจ้งเตือน Clients ก่อนที่ Host จะปิด Server (Optional)
    private void NotifyClientsBeforeShutdown()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // ✅ ส่ง ClientRpc เพื่อแจ้งเตือน clients
            NotifyShutdownClientRpc();

            // ✅ รอสักครู่ก่อน shutdown จริง
            StartCoroutine(ShutdownAfterDelay(2f));
        }
    }

    [ClientRpc]
    private void NotifyShutdownClientRpc()
    {
        Debug.Log("Server is shutting down...");
        // ✅ สามารถแสดง UI แจ้งเตือนผู้เล่นได้ที่นี่
        if (GameManager.Instance != null)
        {
            // แสดงข้อความว่า Server กำลังปิด
        }
    }

    private System.Collections.IEnumerator ShutdownAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }

        ReturnToLobby();
    }

    // ✅ กลับไปยังหน้า Lobby
    private void ReturnToLobby()
    {
        Debug.Log("Returning to Lobby...");

        // ✅ ใช้ SceneTransitionHandler ถ้ามี
        if (SceneTransitionHandler.Instance != null)
        {
            SceneTransitionHandler.Instance.GoToLobbyScene();
        }
        else
        {
            // ✅ สำรอง: โหลด Scene โดยตรง
            UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
        }

        // ✅ ทำความสะอาด GameManager (optional)
        CleanupBeforeExit();
    }

    // ✅ ทำความสะอาดก่อนออกจากเกม
    private void CleanupBeforeExit()
    {
        // ✅ รีเซ็ต game state
        currentScore = 0;
        isGamePaused = false;
        questDatas.Clear();

        // ✅ รีเฟรช UI
        RefreshQuestUI();

        Debug.Log("GameManager cleaned up");
    }

    // ✅ อัพเดท UI เมื่อกดปุ่ม Exit (สำหรับใน Pause Menu)
    public void OnExitButtonPressed()
    {
        ShowExitConfirmation();
    }

    // ✅ Handle เมื่อมีการ disconnect จาก network
    public void HandleNetworkDisconnect()
    {
        Debug.Log("Network disconnected, returning to lobby...");

        // ✅ คืนค่า Time Scale
        Time.timeScale = 1f;
        isGamePaused = false;

        // ✅ กลับไป Lobby
        ReturnToLobby();
    }

    // ------------------- ฟังก์ชันเดิมที่เหลือ -------------------
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
        for (int i = 0; i < questTextSlots.Count; i++)
        {
            if (i < questDatas.Count)
            {
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
        if (scoreText != null)
        {
            scoreText.text = currentScore.ToString();
        }
        Debug.Log($"Score updated: {currentScore}");
    }

    public void TogglePause()
    {
        // ✅ ไม่ให้ pause เมื่อกำลังยืนยันการออกเกม
        if (exitConfirmationUI != null && exitConfirmationUI.activeInHierarchy)
        {
            return;
        }

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
        if (Xpbar != null)
        {
            Xpbar.maxValue = XpRequire;
            Xpbar.value = currentXP;
        }
        if (XpText != null)
        {
            XpText.text = currentXP + "/" + XpRequire;
        }
    }

    public void UpdateLevel(int level)
    {
        if (LevelText != null)
        {
            LevelText.text = "Level : " + level;
        }
    }
}