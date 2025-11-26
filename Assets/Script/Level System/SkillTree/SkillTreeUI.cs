using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillTreeUI : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public Skill skill;
    public SkillBook skillBook;

    [Header("UI Components")]
    public Button skillButton;
    public Image skillIcon;
    public Image borderIcon;
    public GameObject skilldes;
    private bool isHovering = false;
    private bool isPressed = false;
    public TMP_Text skillNameText;
    public TMP_Text skillDescriptionText;
    public TMP_Text skillPointRequireText;


    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f); 
    public Color unlockableColor = Color.white;                 
    public Color unlockedColor = new Color(1f, 0.8f, 0f, 1f);   

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        skillButton = GetComponent<Button>();
        skillIcon = GetComponent<Image>();
        Updatetext();
        skilldes.SetActive(false);
        skillButton.onClick.AddListener(OnSkillClicked);
        skillBook = FindFirstObjectByType<SkillBook>();
        UpdateVisual();
        SkillTreeManager.instance.OnSkillTreeChanged += UpdateVisual;
   
        //ใช้ไปก่อน
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnSkillClicked()
    {
        SkillTreeManager.instance.Unlock(skill , skillBook);
    }

    public void UpdateVisual()
    {
        if (skill == null || SkillTreeManager.instance == null) return;

        skillIcon.sprite = skill.skillIcon;
        if (SkillTreeManager.instance.unlockSkills.Contains(skill)) //unlock
        {
            skillIcon.color = Color.white;
            if (borderIcon != null) borderIcon.color = unlockedColor;
            skillButton.interactable = true;
        }
        else if (SkillTreeManager.instance.CanUnLock(skill)) //canunlock?
        {
            skillIcon.color = Color.white;
            if (borderIcon != null) borderIcon.color = unlockableColor;
            skillButton.interactable = true;
        }
        else
        {
            skillIcon.color = lockedColor; //lock
            if (borderIcon != null) borderIcon.color = lockedColor;
            skillButton.interactable = true;
        }
    }

    private void OnDestroy()
    {
        if (SkillTreeManager.instance != null)
        {
            SkillTreeManager.instance.OnSkillTreeChanged -= UpdateVisual;
        }
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        VisibilityUpdate();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        VisibilityUpdate();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Passss !!!");
        isPressed = true;
        VisibilityUpdate();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
        VisibilityUpdate();
    }
 
    private void Updatetext()
    {
        skillNameText.text = $" {skill.skillName} ";
        skillDescriptionText.text = $" {skill.skillDescription}";
        skillPointRequireText.text = $" Point Require : {skill.skillPointCost}";
    }

    private void VisibilityUpdate()
    {
        if (skilldes != null)
        {
            bool shouldShow = isHovering || isPressed;
            skilldes.SetActive(shouldShow);
        }
    }
}
