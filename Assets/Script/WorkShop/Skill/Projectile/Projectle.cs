using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectle : MonoBehaviour
{
    public int damage;
    public float speed;
    public float lifetime = 3f;

    public Character character;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (character == null)
        {
            Debug.Log("Not have caster");
            return;
        }
        ActiveProjectile(character);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"{enemy.name} take damage {damage}");
                Destroy(gameObject);
            }
        }
    }

    public void ActiveProjectile(Character caster) 
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = caster.transform.forward * speed;
        Destroy(gameObject, lifetime);
    }
}
