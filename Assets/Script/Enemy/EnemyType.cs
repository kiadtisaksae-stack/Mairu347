using UnityEngine;

[CreateAssetMenu(fileName = "EnemyType", menuName = "Game/EnemyType")]
public class EnemyType : ScriptableObject
{
    public string enemyName;
    public int enemyId;
    public Sprite icon;
    public string description;
}