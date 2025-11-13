using UnityEngine;
using Unity.Netcode;

public class UICanvasControllerInput : MonoBehaviour
{
    public static UICanvasControllerInput Instance { get; private set; }

    private Player localPlayer;
    private SkillBook skillBook;

    private bool isSearching = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("✅ UICanvasControllerInput Singleton created");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        Debug.Log("🎮 UICanvasControllerInput Started");
        StartCoroutine(FindLocalPlayerCoroutine());
    }
    private void Update()
    {
        // ถ้ายังหา Player ไม่เจอ และไม่ได้กำลังหาอยู่
        if (localPlayer == null && !isSearching)
        {
            StartCoroutine(FindLocalPlayerCoroutine());
        }
    }

    private System.Collections.IEnumerator FindLocalPlayerCoroutine()
    {
        if (isSearching) yield break;

        isSearching = true;

        int attempts = 0;
        while (attempts < 50) // พยายามหา 5 วินาที
        {
            var players = FindObjectsOfType<Player>();

            foreach (var player in players)
            {
                // ✅ ตรวจสอบ NetworkObject และ IsOwner
                if (player != null && player.IsOwner)
                {
                    localPlayer = player;
                    skillBook = localPlayer.GetComponent<SkillBook>();
                    Debug.Log($"✅ Found local player: {player.name} (Client ID: {player.OwnerClientId})");
                    isSearching = false;
                    yield break;
                }
            }

            attempts++;
            yield return new WaitForSeconds(0.1f);
        }

 
        isSearching = false;
    }
    public void VirtualInteract(bool isInterract)
    {
        if (localPlayer != null)
        {
            localPlayer.SetInteractInput(isInterract);

        }
  
    }
    public void VirtualAttack(bool isAttack)
    {
        if (localPlayer != null)
        {
            localPlayer.SetAttackInput(isAttack);
        }

    }
    public void VirtualMoveInput(Vector2 virtualMoveDirection)
    {
        if (localPlayer != null)
        {
            localPlayer.SetMoveInput(virtualMoveDirection);
            
        }
  
    }

    public void VirtualJumpInput(bool state)
    {
        if (localPlayer != null)
        {
            localPlayer.SetJumpInput(state);
            
        }
    }

    public void VirtualSprintInput(bool state)
    {
        if (localPlayer != null)
        {
            localPlayer.SetSprintInput(state);
            
        }

    }

    public static void RegisterLocalPlayer(Player player)
    {
        if (Instance != null && player.IsOwner)
        {
            Instance.localPlayer = player;
        }
    }
    #region skill input

    public void VirtualUseSkill1()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(0);
            Debug.Log("🎮 UI Skill 1 Activated");
        }
    }
    public void VirtualUseSkill2()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(1);
            Debug.Log("🎮 UI Skill 2 Activated");
        }
    }
    public void VirtualUseSkill3()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(2);
            Debug.Log("🎮 UI Skill 3 Activated");
        }
    }
    public void VirtualUseSkill4()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(3);
            Debug.Log("🎮 UI Skill 4 Activated");
        }
    }
    public void VirtualUseSkill5()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(4);
            Debug.Log("🎮 UI Skill 5 Activated");
        }
    }
    public void VirtualUseSkill6()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(5);
            Debug.Log("🎮 UI Skill 6 Activated");
        }
    }
    public void VirtualUseSkill7()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(6);
            Debug.Log("🎮 UI Skill 7 Activated");
        }
    }
    public void VirtualUseSkill8()
    {
        if (skillBook != null)
        {
            skillBook.UseSkill(7);
            Debug.Log("🎮 UI Skill 8 Activated");
        }
    }
    public void VirtualUseSkill9()
    {        
        if (skillBook != null)
        {
            skillBook.UseSkill(8);
            Debug.Log("🎮 UI Skill 9 Activated");
        }
    }

    #endregion

}