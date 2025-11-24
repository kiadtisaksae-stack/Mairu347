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
        get { return _networkHealth.Value; }
        set 
        { 
            if (IsServer) 
            {
                // Use Hp sync (_networkMaxHealth.Value)
                _networkHealth.Value = Mathf.Clamp(value, 0, maxHealth); 
            }
        }
    }
    
    [Header("status")]
    [SerializeField]
    [HideInInspector] protected int _initialMaxHealth = 100; 

    // MaxHealth Property Readonly
    [HideInInspector] public int maxHealth { get => _networkMaxHealth.Value; }
    [HideInInspector] public int Damage = 10;
    [HideInInspector] public int baseDamage = 10;
    [HideInInspector] public int Defence = 10;
    [HideInInspector] public int baseDefence = 10;
    [HideInInspector] public float movementSpeed;
    protected Animator animator;
    protected NetworkAnimator networkAnimator;

    public event Action<Idestoryable> OnDestory;
    protected void InvokeOnDestroy()
    {
        OnDestory?.Invoke(this);
    }


    [Header("Quests")]
    protected QuestManager questManager;

    // Netcode Lifecycle: OnNetworkSpawn/OnDespawn 

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if (IsServer)
        {
            //Sevrer Set initial values
            _networkMaxHealth.Value = _initialMaxHealth; 
            _networkHealth.Value = _initialMaxHealth;
        }
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
    }
    
  
    // TakeDamage & Heal
    public override void SetUP()
    {
        base.SetUP();
        
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
        networkAnimator = GetComponent<NetworkAnimator>();
        networkAnimator.Animator = animator;
    }
    
    public virtual void TakeDamage(int amount)
    {
        if(isOnLive.Value == false) return;
        if (!IsServer)
        {
            return;
        }

        int actualDamage = Mathf.Clamp(amount - Defence, 1, amount);
        health -= actualDamage;


        ShowDamageClientRpc(actualDamage, transform.position);
        if (TryGetComponent<NetworkObject>(out var netObject))
        {
            UpdateHealthUIForOwnerClientRpc(health, maxHealth,
                new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { netObject.OwnerClientId } } });
        }
        if (health <= 0)
        {
            Die();
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (!IsServer) return;
        health += amount;
        HealClientRpc(amount, transform.position);
    }
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



    #region ---RPC Calls---
    [ClientRpc]
    private void UpdateHealthUIForOwnerClientRpc(int currentHealth, int currentMaxHealth, ClientRpcParams rpcParams = default)
    {
        if (this is Player)
        {
            GameManager.Instance.UpdateHealthBar(currentHealth, currentMaxHealth);
            Debug.Log($"🩸 Client UI Updated: {currentHealth}/{currentMaxHealth}");
        }
    }
    [ClientRpc]
    public void ShowDamageClientRpc(int actualDamage, Vector3 damagePosition)
    {
        if (animator != null)
        {
            //
        }

    }
    [ClientRpc]
    public void HealClientRpc(int amount, Vector3 healPosition)
    {

        if (animator != null)
        {
            //
        }

        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} sees {gameObject.name} heal {amount} at {healPosition}");


    }
    [ClientRpc]
    protected void OnDieClientRpc()
    {
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("Die");

        // ถ้าใช้ script เดินให้ปิด
        //var player = GetComponent<Player>();
        //if (player) player.enabled = false;
    }
    [ClientRpc]
    private void OnReviveClientRpc(Vector3 spawnPoint)
    {
        var cc = GetComponent<CharacterController>();
        if (cc)
        {
            cc.enabled = false; // ต้องปิดก่อนย้ายตำแหน่ง
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
            anim.Update(0f); // reset state ของ Animator
        }

        //var move = GetComponent<PlayerMovement>();
        //if (move) move.enabled = true;
    }
    #endregion
}