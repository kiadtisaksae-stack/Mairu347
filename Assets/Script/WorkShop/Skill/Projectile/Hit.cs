using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Hit : MonoBehaviour
{
    public List<Enemy> inRadiusEnemys = new List<Enemy>();
    public int damagePerTick = 5;
    public float tickInterval = 1.0f;
    public float radius = 4.5f;

    private SphereCollider sphereCollider;


    private void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.radius = this.radius;
        StartCoroutine(TakeDamageInRadius());
    }

    IEnumerator TakeDamageInRadius()
    {
        while (true)
        {
            yield return new WaitForSeconds(tickInterval);
            for (int i = inRadiusEnemys.Count - 1; i >= 0; i--) 
            {
                if (inRadiusEnemys[i] != null)
                {
                    inRadiusEnemys[i].TakeDamage(damagePerTick);
                }
                else
                {
                    inRadiusEnemys.RemoveAt(i);
                }
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && !inRadiusEnemys.Contains(enemy))
        {
            inRadiusEnemys.Add(enemy);
        }
    }
    void OnTriggerExit(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null && inRadiusEnemys.Contains(enemy))
        {
            inRadiusEnemys.Remove(enemy);
        }
    }
}
