using UnityEngine;
[CreateAssetMenu(fileName = "New Item", menuName = "Create new Item", order = 4)]

public class ItemSO : ScriptableObject
{
    
    public Sprite icon;
    [Header("Item Type\r\n  NONE = 0;\r\n CONSUMABLE = 1;\r\n HEAD = 2;\r\n BODY = 3;\r\n LEGS = 4;" +
        "\r\n  ONE_HAND_WEAPON = 5;\r\n TWO_HAND_WEAPON = 6;")]
    public int tybe ;
    public int id;
    public int Damage;
    public int Deffent;
    public string itemName;
    public string description;
    public int maxStack;
    [Header("Consumable Effects")]
    public int healAmount = 0;
    public int manaAmount = 0;
    [Header("In Game Obj")]
    public GameObject gamePrefab;
    
}
