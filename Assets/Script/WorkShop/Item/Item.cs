using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;
using System;
using UnityEngine.UI;

[RequireComponent(typeof(SphereCollider))]
public class Item : Identity
{
    public virtual Equipment GetEquipment()
    {
        return Equipment.None;
    }
    public event Action<ulong> OnCollected;
    private Collider _collider;
    public Sprite sprite;
    public string itemName;
    protected Collider itemcollider
    {
        get
        {
            if (_collider == null)
            {
                _collider = GetComponent<Collider>();
                _collider.isTrigger = true;
            }
            return _collider;
        }
    }
   

    public override void SetUP()
    {
        base.SetUP();
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
        this.Name = itemName;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (itemcollider != null)
        {
            itemcollider.enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    // ----------------------------------------------------
    // 💣 Destruction Logic (Server Authority)
    // ----------------------------------------------------

    public void HandleDestroyed()
    {
        if (!IsServer) return;
        // 1. Host/Server เรียก Event เพื่อให้ Manager บันทึก ID
        OnCollected?.Invoke(NetworkObjectId);
        // 2. สั่ง Despawn (จะเรียก OnNetworkDespawn บนทุกเครื่อง)
        NetworkObject.Despawn();
    }
    public void OnTriggerEnter(Collider other)
    {
        // ต้องเป็น Server หรือ Owner ที่ต้องการส่ง RPC
        if (NetworkManager.Singleton.IsClient)
        {
            if (other.tag == "Player")
            {
                Player collector = other.GetComponent<Player>();

                if (collector != null && collector.IsOwner) 
                {
                    RequestCollectServerRpc(collector.NetworkObject);
                }
            }
        }
    }

    public virtual void OnCollect(Player player)
    {
        player.AddItem(this);
        Debug.Log($"Collected {Name}");
    }

    public virtual void Use(Player player)
    {
        Debug.Log($"Using {Name}");
    }

    // *** SERVER SIDE: การตัดสินใจ (Called by Client) ***
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void RequestCollectServerRpc(NetworkObjectReference collectorNetRef)
    {
        if (!IsServer) return;

        if (!collectorNetRef.TryGet(out NetworkObject collectorNetObj)) return;
        Player collector = collectorNetObj.GetComponent<Player>();

        if (collector == null || !NetworkObject.IsSpawned) return;
        
        //ดึงชื่อ Item ใหม่เพื่อตรวจสอบ
        string newItemName = Name;
        if (collector.IsItemEquipped(newItemName))
        {
            //เขียนlogic text ได้
            return; 
        }
        
        OnCollect(collector);
        LogCollectedClientRpc(new FixedString32Bytes(collector.Name), new FixedString32Bytes(Name));
        HandleDestroyed();
    }

    [ClientRpc]
    public virtual void LogCollectedClientRpc(FixedString32Bytes playerName, FixedString32Bytes itemName)
    {
        Debug.Log($"📢 Global Log: {playerName.ToString()} collected {itemName.ToString()}!");
    }

}