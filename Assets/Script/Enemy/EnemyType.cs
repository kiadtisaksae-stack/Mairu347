using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Game/EnemyType")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public int enemyId;
    
    [Header("status")]
    [SerializeField]
    public int _initialMaxHealth = 100; 
    public int Damage ;
    public int Defence;
    public float movementSpeed;    
    
}