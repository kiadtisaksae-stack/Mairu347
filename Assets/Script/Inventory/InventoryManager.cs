using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryPanel; 
    public GameObject BagPack; 
    public Button questBtn;
    public GameObject questPanel; 

    void Start()
    {
        if (questBtn != null)
        {
            questBtn.onClick.AddListener(onClickQuestBtn);
        }
    }

    public void onClickQuestBtn()
    {
        if (questPanel != null)
        {
            bool isCurrentlyActive = questPanel.activeSelf;
            questPanel.SetActive(!isCurrentlyActive);
        }
    }
}