using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button StartHost;
    public Button PlayOffLine;
    public Button Multiplayer;

    public GameObject mainMenuPanel;
    public GameObject characterSelectionPanel;

    public CharacterSelection characterSelection;

    void Start()
    {
        Multiplayer.onClick.AddListener(OpenCharacterSelection);
    }

    private void OpenCharacterSelection()
    {
        mainMenuPanel.SetActive(false);
        characterSelectionPanel.SetActive(true);

        characterSelection.UpdateCharacterNumber();
    }
}
