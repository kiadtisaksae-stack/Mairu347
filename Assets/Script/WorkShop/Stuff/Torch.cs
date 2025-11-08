using Unity.Netcode;
using UnityEngine;

public class Torch : Stuff, IInteractable
{
    private readonly NetworkVariable<bool> _networkIsOn = new(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public bool isInteractable { get => isLock; set => isLock = value; }
    public GameObject Firelight;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        _networkIsOn.OnValueChanged += OnIsOnChanged;
        OnIsOnChanged(false, _networkIsOn.Value); 
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        _networkIsOn.OnValueChanged -= OnIsOnChanged;
    }
    
    public override void SetUP()
    {
        base.SetUP();
    }
    
    public override void OnIsOnChanged(bool previousValue, bool newValue)
    {
        if (Firelight != null)
        {
            Firelight.gameObject.SetActive(newValue);
            Debug.Log($"[TORCH SYNC] State Changed and Visual Updated on Client (ID: {NetworkManager.Singleton.LocalClientId}). New State: {newValue}"); // ✅ Debug 4
        }
    }

    public void Interact(Player player)
    {
        if (player.IsOwner)
        {
            Debug.Log($"[TORCH] Client (ID: {NetworkManager.Singleton.LocalClientId}) sending RPC. Current State: {_networkIsOn.Value}"); // ✅ Debug 2
            
            // ส่งคำขอเปลี่ยนค่าตรงข้ามกับสถานะปัจจุบันที่ซิงค์แล้ว
            ToggleTorchServerRpc(!_networkIsOn.Value);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public override void ToggleTorchServerRpc(bool newState)
    {
        if (!IsServer)
        {
            Debug.LogError("[TORCH ERROR] ToggleTorchServerRpc called but not running on Server!");
            return;
        }
        
        Debug.Log($"[TORCH SERVER] Server received request. Changing state to: {newState}"); // ✅ Debug 3
        
        // Server เปลี่ยนค่า NetworkVariable โดยตรง
        _networkIsOn.Value = newState; 
    }
    
}