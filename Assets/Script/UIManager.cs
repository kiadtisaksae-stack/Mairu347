using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Button playGame;
    void Start()
    {
        playGame.onClick.AddListener(OnclickPlay);

    }

    void Update()
    {

    }

    void OnclickPlay()
    {
        Destroy(playGame.gameObject, 1f);

    }

}
