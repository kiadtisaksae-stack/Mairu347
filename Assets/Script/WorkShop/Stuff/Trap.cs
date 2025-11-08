using Unity.Netcode;
using UnityEngine;

public class Trap : Stuff, IInteractable
{
    public Trap() {
        Name = "Trap";
    }
    private readonly NetworkVariable<bool> _networkIsOn = new(
        false, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    public bool isInteractable { get => isLock; set => isLock = value; }
    public int Damage = 10;
    public GameObject spikes;
    public void Interact(Player player)
    {
        if (player.IsOwner)
        {
            Debug.Log($"[TORCH] Client (ID: {NetworkManager.Singleton.LocalClientId}) sending RPC. Current State: {_networkIsOn.Value}"); // ✅ Debug 2
            
            // ส่งคำขอเปลี่ยนค่าตรงข้ามกับสถานะปัจจุบันที่ซิงค์แล้ว
            ToggleTorchServerRpc(!_networkIsOn.Value);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LocalPlayer.TakeDamage(10);
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
