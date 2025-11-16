using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Create new Item", order = 4)]

public class ItemSO : ScriptableObject
{
    public Sprite icon;
    public int tybe ;
    public int id;
    public int Damage;
    public int Deffent;
    public string itemName;
    public string description;
    public int maxStack;
    [Header("In Game Obj")]
    public GameObject gamePrefab;
    
}
