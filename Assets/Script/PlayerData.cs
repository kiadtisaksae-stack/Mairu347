using System.Numerics;

[System.Serializable]
public class PlayerData
{
    public int health;
    public int maxHealth;
    public int damage;
    public int defence;
 
    public PlayerData(int health, int maxHealth, int damage, int defence)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.damage = damage;
        this.defence = defence;
    }
}