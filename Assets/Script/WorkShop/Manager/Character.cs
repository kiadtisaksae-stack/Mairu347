using System;
using Unity.Netcode;
using UnityEngine;

public class Character : Identity, Idestoryable
{
    private readonly NetworkVariable<int> _networkHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private readonly NetworkVariable<int> _networkMaxHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

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
    private int _initialMaxHealth = 100; 

    // MaxHealth Property Readonly
    public int maxHealth { get => _networkMaxHealth.Value; }
    public int Damage = 10;
    public int Deffent = 10;
    public float movementSpeed;
    protected Animator animator;

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
        
        _networkHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _networkHealth.OnValueChanged -= HandleHealthChanged;
    }
    
    private void HandleHealthChanged(int previousValue, int newValue)
    {
        
        if (newValue <= 0)
        {
            // Logic Client die
        }
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
    }
    
    public virtual void TakeDamage(int amount)
    {
        if (!IsServer)
        {
            // Client ไม่ควรเรียก TakeDamage ตรงๆ แต่คุณจัดการแล้ว
            return;
        }

        int actualDamage = Mathf.Clamp(amount - Deffent, 1, amount);
        health -= actualDamage;


        ShowDamageClientRpc(actualDamage, transform.position); 

        if (health <= 0)
        {

            OnDestory?.Invoke(this);
            GetComponent<NetworkObject>().Despawn();
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (!IsServer) return;
        health += amount;
        HealClientRpc(amount, transform.position);
    }
    
    

    #region ---RPC Calls---
    [ClientRpc]
    public void ShowDamageClientRpc(int actualDamage, Vector3 damagePosition)
    {
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} sees {gameObject.name} take {actualDamage} damage at {damagePosition}");

    }
    [ClientRpc]
    public void HealClientRpc(int amount, Vector3 healPosition)
    {

        if (animator != null)
        {
            animator.SetTrigger("Heal");
        }

        Debug.Log($"Client {NetworkManager.Singleton.LocalClientId} sees {gameObject.name} heal {amount} at {healPosition}");


    }
    #endregion
}