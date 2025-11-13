using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerData : ScriptableObject
{
    public static PlayerData Instance;

    public string playerName = "Default";
    public int maxHealth = 100;
    public int damage = 10;
    public float speed = 5f;

    private void OnEnable() => Instance = this;
}
