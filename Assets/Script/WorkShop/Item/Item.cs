using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Item : Identity
{
    private Collider _collider;
    protected Collider itemcollider {
        get {
            if (_collider == null) {
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
        _collider.isTrigger = true;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // 💡 รับประกันการทำลาย GameObject หลังจากการ Despawn ของ Netcode
        if (gameObject != null)
        {
            Destroy(gameObject);
        }

    }

    public Item() { 
    }
    public Item(Item item)
    {
        this.Name = item.Name;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        { 
            // 2. ดึงคอมโพเนนต์ Player จาก GameObject ที่ชน
            Player collector = other.GetComponent<Player>();
            
            // 3. ตรวจสอบความถูกต้องและสั่งเก็บ
            if (collector != null)
            {
                RequestCollectServerRpc(collector.NetworkObject);
            }
        }
    }
    public virtual void OnCollect(Player player) 
    { 
        Debug.Log($"Collected {Name}");
    }
    public virtual void Use(Player player)
    {
        Debug.Log($"Using {Name}");
    }

    
    // ******************************************************
    // *** 🎯 SERVER SIDE: การตัดสินใจ (Called by Client) 🎯 ***
    // ******************************************************

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public  void RequestCollectServerRpc(NetworkObjectReference collectorNetRef)
    {

        if (!IsServer) return;

        if (!collectorNetRef.TryGet(out NetworkObject collectorNetObj)) return;
        Player collector = collectorNetObj.GetComponent<Player>();
        
        // 🚨 1. ตรวจสอบความถูกต้องของ Player ก่อนเรียก OnCollect
        if (collector == null || !NetworkObject.IsSpawned) 
        {
            // ถ้าไม่ผ่านการตรวจสอบ ควรเปิด Collider คืน (ถ้ามี logic การเปิด)
            return; 
        }
        
        // 2. Server เรียก Hook
        OnCollect(collector); 

        // 3. Server แจ้ง Log และ Despawn
        LogCollectedClientRpc(new FixedString32Bytes(collector.Name), new FixedString32Bytes(Name));
        NetworkObject.Despawn();
    }

    [ClientRpc]
    public virtual void LogCollectedClientRpc(FixedString32Bytes playerName, FixedString32Bytes itemName)
    {
        //text editor UI
        Debug.Log($"📢 Global Log: {playerName.ToString()} collected {itemName.ToString()}!");
    }
}
