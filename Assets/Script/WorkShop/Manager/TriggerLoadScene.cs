using UnityEngine;

public class TriggerLoadScene : Item
{
    public AudioClip BackgroundMusic;
    public string LoadSceneName;

    public override void SetUP()
    {
        base.SetUP();
        
    }
    public override void OnCollect(Player player)
    {
        base.OnCollect(player);
        SoundManager.Instance.PlayMusic(BackgroundMusic);
        LoadSceneManager.Instance.LoadNewScene(LoadSceneName);
    }
}
