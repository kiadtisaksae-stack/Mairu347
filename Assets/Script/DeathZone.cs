using UnityEngine;
using Unity.Netcode;

public class DeathZone : MonoBehaviour
{
    private const int INSTANT_KILL_DAMAGE = 9999999;

    private void OnTriggerEnter(Collider other)
    {
        // ✅ ทำงานเฉพาะบน Server
        if (!NetworkManager.Singleton.IsServer) return;

        Debug.Log($"[DEATHZONE] Trigger entered by: {other.name}");

        // ✅ ตรวจสอบ Character
        if (other.TryGetComponent(out Character character))
        {
            Debug.Log($"[DEATHZONE] Applying {INSTANT_KILL_DAMAGE} damage to {character.Name}");
            character.TakeDamage(INSTANT_KILL_DAMAGE);
        }
    }
}