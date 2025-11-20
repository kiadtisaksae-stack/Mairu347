// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class ChainDamageItem : item
// {
//     public int chainCount = 2;
//     public float chainRadius = 5f;
//     public GameObject chainEffectPrefab;
//     //public AudioClip chainSound; // ���§�Ϳ�硵���
//     public GameObject lineEffectPrefab;

//     public override void itemEffect(GameObject target)
//     {
//         ApplyChainDamage(target);
//         currrentDurability--;
//         if (currrentDurability <= 0)
//         {
//             currrentDurability = 0;
//             Destroy(gameObject);
//         }
//     }
//     void ApplyChainDamage(GameObject initialTarget)
//     {
//         List<GameObject> hitEnemies = new List<GameObject>();
//         hitEnemies.Add(initialTarget);

//         Enemy initialEnemy = initialTarget.GetComponent<Enemy>();
//         if (initialEnemy != null)
//         {
//             initialEnemy.TakeDamage(damageAmount);
//             Debug.Log("Damage applied to initial target");
//         }
//         else
//         {
//             Debug.LogError("Initial target does not have EnemyHealth component");
//         }

//         for (int i = 0; i < chainCount; i++)
//         {
//             Collider[] hitColliders = Physics.OverlapSphere(hitEnemies[hitEnemies.Count - 1].transform.position, chainRadius);
//             GameObject nextTarget = null;
//             float closestDistance = Mathf.Infinity;

//             foreach (var hitCollider in hitColliders)
//             {
//                 if (hitCollider.CompareTag("Enemy") && !hitEnemies.Contains(hitCollider.gameObject))
//                 {
//                     float distance = Vector3.Distance(hitEnemies[hitEnemies.Count - 1].transform.position, hitCollider.transform.position);
//                     if (distance < closestDistance)
//                     {
//                         closestDistance = distance;
//                         nextTarget = hitCollider.gameObject;
//                     }
//                 }
//             }
//             if (nextTarget != null)
//             {
//                 Enemy nextEnemy = nextTarget.GetComponent<Enemy>();
//                 if (nextEnemy != null)
//                 {
//                     nextEnemy.TakeDamage(damageAmount);
//                     hitEnemies.Add(nextTarget);
//                     Debug.Log("Damage chained to next target");

//                     // ���ҧ�Ϳ�硵���
//                     if (chainEffectPrefab != null)
//                     {
//                         GameObject chainEffect = Instantiate(chainEffectPrefab, nextTarget.transform.position, Quaternion.identity);
//                         Destroy(chainEffect, 1f);
//                     }

//                     // ������§�Ϳ�硵���
//                     //if (chainSound != null)
//                     //{
//                     //    AudioSource.PlayClipAtPoint(chainSound, nextTarget.transform.position);
//                     //}

//                     // ���ҧ LineRenderer
//                     if (lineEffectPrefab != null)
//                     {
//                         GameObject lineEffect = Instantiate(lineEffectPrefab);
//                         LineRenderer lineRenderer = lineEffect.GetComponent<LineRenderer>();
//                         if (lineRenderer != null)
//                         {
//                             lineRenderer.SetPosition(0, hitEnemies[hitEnemies.Count - 2].transform.position);
//                             lineRenderer.SetPosition(1, nextTarget.transform.position);
//                             Destroy(lineEffect, 0.5f); // ����� LineRenderer ��ѧ�ҡ 0.5 �Թҷ� (��Ѻ�����ͧ���)
//                         }
//                     }
//                 }
//             }
//             else
//             {
//                 break;
//             }
//         }
//     }
// }
