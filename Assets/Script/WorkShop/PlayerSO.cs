using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Game/PlayerSO")]
public class PlayerSO : ScriptableObject
{
    public int _initialMaxHealth = 100; 
    public int Damage = 10;
    public int baseDamage = 10;
    public int Defence = 10;
    public int baseDefence = 10;
    public float movementSpeed;
    public float sprint;
}
