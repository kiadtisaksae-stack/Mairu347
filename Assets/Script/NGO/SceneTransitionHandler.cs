using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class SceneTransitionHandler : MonoBehaviour
{
    public static SceneTransitionHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// สั่งให้ Host/Server โหลด Scene ใหม่ และซิงค์ไปยัง Clients
    /// </summary>
    /// <param name="sceneName">ชื่อ Scene ที่ต้องการโหลด</param>
    public void LoadGameScene(string sceneName)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("Scene loading must be initiated by the Host/Server.");
            return;
        }

        // ใช้ NetworkSceneManager เพื่อซิงค์การโหลด Scene
        NetworkManager.Singleton.SceneManager.LoadScene(
            sceneName,
            LoadSceneMode.Single);
    }

    /// <summary>
    /// นำผู้เล่นกลับไปที่ Lobby โดยสั่ง Shutdown Network ก่อน
    /// </summary>
    public void GoToLobbyScene()
    {
        // 1. หยุด Network Session
        if (NetworkManager.Singleton.IsListening)
        {
            // Shutdown จะทำให้ Client หลุดการเชื่อมต่อและปิด Transport
            NetworkManager.Singleton.Shutdown();
        }

        // 2. โหลด Lobby Scene แบบ Local
        SceneManager.LoadScene("Lobby");
    }
}