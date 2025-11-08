using TMPro;
using Unity.Netcode;
using UnityEngine;

public class Switch : Stuff, IInteractable
{
    private readonly NetworkVariable<bool> _networkIsOn = new(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public bool isInteractable { get => isLock; set => isLock = value; }
    Animator animator;
    public Identity InteracTo;
    IInteractable IInterac { 
        get {
            return InteracTo as IInteractable;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        _networkIsOn.OnValueChanged += OnIsOnChanged;
        
        OnIsOnChanged(false, _networkIsOn.Value); 
        animator = GetComponent<Animator>();
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _networkIsOn.OnValueChanged -= OnIsOnChanged;
    }
    
    private void OnIsOnChanged(bool previousValue, bool newValue)
    {
        if (animator != null)
        {
            animator.SetBool("IsOn", newValue);
        }
        
        if (IsServer && InteracTo != null)
        {
            // interact to other object
            
        }
    }

    public void Interact(Player player)
    {
        if (player.IsOwner)
        {
            // 🚨 Client ส่ง ServerRpc เพื่อขอเปลี่ยนสถานะ
            ToggleSwitchServerRpc(!_networkIsOn.Value);
        }
    }
    
    // server change all client
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ToggleSwitchServerRpc(bool newState)
    {
        if (!IsServer) return;
        
        // Server เปลี่ยนค่า NetworkVariable โดยตรง
        _networkIsOn.Value = newState; 
        
    }
}