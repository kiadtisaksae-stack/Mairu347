using System;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

[RequireComponent(typeof(NetworkTransform))]
[RequireComponent(typeof(NetworkAnimator))]
public class Character : Identity, Idestoryable
{
    private readonly NetworkVariable<int> _networkHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _networkMaxHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    protected float sprintSpeed = 8f;

    public int health
    {
        get => _networkHealth.Value;
        set
        {
            if (IsServer)
                _networkHealth.Value = Mathf.Clamp(value, 0, maxHealth);
        }
    }

    [Header("Status")]
    [SerializeField] protected int _initialMaxHealth = 100;

    public int maxHealth => _networkMaxHealth.Value;
    public int Damage = 10;
    public int baseDamage = 10;
    public int Defence = 10;
    [HideInInspector] public int baseDefence = 10;
    [HideInInspector] public float movementSpeed;
    protected Animator animator;
    protected NetworkAnimator networkAnimator;

    public event Action<Idestoryable> OnDestory;
    protected void InvokeOnDestroy() => OnDestory?.Invoke(this);

    [Header("Quests")]
    protected QuestManager questManager;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            _networkMaxHealth.Value = _initialMaxHealth;
            _networkHealth.Value = _initialMaxHealth;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }

    public override void SetUP()
    {
        base.SetUP();
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
            Debug.LogError("Animator not found on " + gameObject.name);

        networkAnimator = GetComponent<NetworkAnimator>();
        networkAnimator.Animator = animator;
    }

    // -------- TakeDamage --------

    public virtual void TakeDamage(int amount)
    {
        if (isOnLive.Value == false) return;
        if (!IsServer) return;

        int actualDamage = Mathf.Clamp(amount - Defence, 1, amount);
        health -= actualDamage;

        ShowDamageClientRpc(actualDamage, transform.position);

        // ส่ง UI update ให้ Owner เท่านั้น
        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            UpdateHealthUIForOwnerClientRpc(health, maxHealth,
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { netObj.OwnerClientId } } });
        }

        if (health <= 0) Die();
    }

    // -------- Heal --------

    public virtual void Heal(int amount)
    {
        if (!IsServer) return;

        health += amount;  // NetworkVariable sync อัตโนมัติ

        Debug.Log($"[SERVER] {gameObject.name} healed {amount} → HP {health}/{maxHealth}");

        // ✅ ส่ง UI update กลับ Owner (เหมือนที่ TakeDamage ทำ)
        if (TryGetComponent<NetworkObject>(out var netObj))
        {
            UpdateHealthUIForOwnerClientRpc(health, maxHealth,
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { netObj.OwnerClientId } } });
        }

        HealClientRpc(amount, transform.position);
    }

    // -------- Die / Revive --------

    public virtual void Die()
    {
        OnDestory?.Invoke(this);
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
        else
            Destroy(gameObject);
    }

    public virtual void Revive(Vector3 spawnPoint)
    {
        if (!IsServer) return;
        isOnLive.Value = true;
        transform.position = spawnPoint;
        health = maxHealth;
        OnReviveClientRpc(spawnPoint);
    }

    #region RPC

    // ส่ง HP bar ให้ Owner เท่านั้น — ใช้ทั้ง TakeDamage และ Heal
    [ClientRpc]
    private void UpdateHealthUIForOwnerClientRpc(int currentHealth, int currentMaxHealth, ClientRpcParams rpcParams = default)
    {
        if (this is Player)
        {
            GameManager.Instance.UpdateHealthBar(currentHealth, currentMaxHealth);
            Debug.Log($"[CLIENT UI] HP updated: {currentHealth}/{currentMaxHealth}");
        }
    }

    [ClientRpc]
    public void ShowDamageClientRpc(int actualDamage, Vector3 damagePosition)
    {
        // TODO: แสดง damage number
    }

    [ClientRpc]
    public void HealClientRpc(int amount, Vector3 healPosition)
    {
        // TODO: แสดง heal effect / animation
        Debug.Log($"[CLIENT] {gameObject.name} heal {amount} at {healPosition}");
    }

    [ClientRpc]
    protected void OnDieClientRpc()
    {
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Die");
    }

    [ClientRpc]
    private void OnReviveClientRpc(Vector3 spawnPoint)
    {
        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false;
            transform.position = spawnPoint;
            cc.enabled = true;
        }
        else
        {
            transform.position = spawnPoint;
        }

        var anim = GetComponent<Animator>();
        if (anim)
        {
            anim.Rebind();
            anim.Update(0f);
        }
    }

    #endregion
}