using UnityEngine;

public class SkillButton : MonoBehaviour
{
    public GameObject SkillTire1;
    public GameObject SkillTire2;
    public GameObject SkillTire3;
    void Start()
    {
        SkillTire1.SetActive(false);
        SkillTire2.SetActive(false);
        SkillTire3.SetActive(true);
    }

    public void OnClick()
    {
        if (SkillTire1.activeSelf)
        {
            SkillTire1.SetActive(false);
            SkillTire2.SetActive(true);
            SkillTire3.SetActive(false);
        }
        else if (SkillTire2.activeSelf)
        {
            SkillTire1.SetActive(false);
            SkillTire2.SetActive(false);
            SkillTire3.SetActive(true);
        }
        else if (SkillTire3.activeSelf)
        {
            SkillTire1.SetActive(true);
            SkillTire2.SetActive(false);
            SkillTire3.SetActive(false);
        }
    }
}
