using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UICanvasControllerInput : MonoBehaviour
{
    public static UICanvasControllerInput Instance { get; private set; }

    private Player localPlayer;
    private SkillBook skillBook;

    [Header("Skill Buttons (slot 1-9)")]
    public GameObject[] skillButtonSlots = new GameObject[9];
    // แต่ละ slot คือ Button GameObject ที่มี child ชื่อ "Image_Icon"

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(TryFindLocalPlayer), 0f, 0.5f);
    }

    private void TryFindLocalPlayer()
    {
        if (localPlayer != null)
        {
            CancelInvoke(nameof(TryFindLocalPlayer));
            return;
        }

        var players = FindObjectsOfType<Player>();
        foreach (var player in players)
        {
            if (player != null && player.IsOwner)
            {
                localPlayer = player;
                skillBook = localPlayer.GetComponent<SkillBook>();
                CancelInvoke(nameof(TryFindLocalPlayer));
                return;
            }
        }
    }

    public static void RegisterLocalPlayer(Player player)
    {
        if (Instance != null && player.IsOwner)
        {
            Instance.localPlayer = player;
            Instance.skillBook = player.GetComponent<SkillBook>();
            Instance.CancelInvoke(nameof(TryFindLocalPlayer));
        }
    }

    // ─────────────────────────────────────────
    // เรียกจาก SkillTreeManager.Unlock() เมื่ออัพสกิล
    // หาช่องว่างแรกใน skillButtonSlots แล้วใส่ icon สกิลลงไป
    // ─────────────────────────────────────────
    public static void RegisterSkillToNextSlot(Skill skill)
    {
        if (Instance == null) return;

        for (int i = 0; i < Instance.skillButtonSlots.Length; i++)
        {
            GameObject slotObj = Instance.skillButtonSlots[i];
            if (slotObj == null) continue;

            // เช็คว่าช่องนี้ยังว่างอยู่ไหม (ดูจาก Image_Icon ที่ยังไม่มี sprite)
            Transform iconTransform = slotObj.transform.Find("Image_Icon");
            if (iconTransform == null) continue;

            Image iconImage = iconTransform.GetComponent<Image>();
            if (iconImage == null) continue;

            // ถ้า sprite ยังว่าง → ช่องนี้ว่าง ใส่สกิลได้
            if (iconImage.sprite == null)
            {
                iconImage.sprite = skill.skillIcon;
                iconImage.color = Color.white;
                Debug.Log($"[UICanvas] ใส่ '{skill.skillName}' ไว้ที่ slot {i + 1}");
                return;
            }
        }

        Debug.LogWarning("[UICanvas] ไม่มีช่องว่างสำหรับสกิลใหม่แล้ว");
    }

    // ─────────────────────────────────────────
    // Virtual inputs
    // ─────────────────────────────────────────
    public void VirtualInteract(bool isInterract) { if (localPlayer != null) localPlayer.SetInteractInput(isInterract); }
    public void VirtualAttack(bool isAttack) { if (localPlayer != null) localPlayer.SetAttackInput(isAttack); }
    public void VirtualMoveInput(Vector2 dir) { if (localPlayer != null) localPlayer.SetMoveInput(dir); }
    public void VirtualJumpInput(bool state) { if (localPlayer != null) localPlayer.SetJumpInput(state); }
    public void VirtualSprintInput(bool state) { if (localPlayer != null) localPlayer.SetSprintInput(state); }

    #region Skill inputs
    public void VirtualUseSkill1() { if (skillBook != null) skillBook.UseSkill(0); }
    public void VirtualUseSkill2() { if (skillBook != null) skillBook.UseSkill(1); }
    public void VirtualUseSkill3() { if (skillBook != null) skillBook.UseSkill(2); }
    public void VirtualUseSkill4() { if (skillBook != null) skillBook.UseSkill(3); }
    public void VirtualUseSkill5() { if (skillBook != null) skillBook.UseSkill(4); }
    public void VirtualUseSkill6() { if (skillBook != null) skillBook.UseSkill(5); }
    public void VirtualUseSkill7() { if (skillBook != null) skillBook.UseSkill(6); }
    public void VirtualUseSkill8() { if (skillBook != null) skillBook.UseSkill(7); }
    public void VirtualUseSkill9() { if (skillBook != null) skillBook.UseSkill(8); }
    #endregion
}