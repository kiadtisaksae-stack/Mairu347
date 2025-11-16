// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class DotItem : item
// {
//     public float damagePerSecond = 1f;
//     public float dotDuration = 3f;

//     public override void itemEffect(GameObject target)
//     {
//         if (gameObject !=null)
//         {
//             StartCoroutine(DestroyAfterDamage(target.GetComponent<Enemy>()));
//         }
//     }
//     IEnumerator DestroyAfterDamage(Enemy enemy)
//     {
//         yield return StartCoroutine(ApplyDotDamage(enemy)); // ����� ApplyDotDamage() ���稡�͹
//         currrentDurability--;
//         if (currrentDurability <= 0)
//         {
//             currrentDurability = 0;
//             Destroy(gameObject);
//         }
//     }

//     IEnumerator ApplyDotDamage(Enemy enemy)
//     {
//         float timer = 0f;
//         while (timer < dotDuration)
//         {
//             if (enemy != null) 
//             {
//                 enemy.TakeDamage((float)damagePerSecond);
//             }
//             timer += Time.deltaTime;
//             yield return null;
//         }
//     }
// }
