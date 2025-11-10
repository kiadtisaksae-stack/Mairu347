using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using Unity.Collections;
using System; // สำหรับ Event

[RequireComponent(typeof(SphereCollider))]
public class Item : Identity
{
    // 💡 Event สำหรับแจ้ง ItemSpawnManager ว่าไอเทมนี้ถูกเก็บไปแล้ว
    public event Action<ulong> OnCollected;

    // ----------------------------------------------------
    // ⚙️ Component References & Initialization
    // ----------------------------------------------------

    private Collider _collider;
    protected Collider itemcollider
    {
        get
        {
            if (_collider == null)
            {
                // ต้องมี Collider เพื่อให้ OnTriggerEnter ทำงาน
                _collider = GetComponent<Collider>();
                _collider.isTrigger = true;
            }
            return _collider;
        }
    }

    public override void SetUP()
    {
        base.SetUP();
        // ตรวจสอบ/ตั้งค่า Collider ใน SetUP ด้วย
        _collider = GetComponent<Collider>();
        if (_collider != null)
        {
            _collider.isTrigger = true;
        }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // 🚨 Item ที่ถูก Spawn จะถือว่า 'เก็บได้' จนกว่าจะถูก Despawn
        // ลบ Logic การตรวจสอบ _isCollectable ออก (ใช้ Despawn/SpawnManager แทน)

        // ตรวจสอบสถานะเริ่มต้น (ถ้าถูก Spawn แล้วควรเปิด Collider)
        if (itemcollider != null)
        {
            itemcollider.enabled = true;
        }
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // 💡 รับประกันการทำลาย GameObject หลัง Netcode Despawn
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
        // ItemSpawnManager จะรับ Event นี้และบันทึก NetworkObjectId ลงใน NetworkList
        OnCollected?.Invoke(NetworkObjectId);

        // 2. สั่ง Despawn (จะเรียก OnNetworkDespawn บนทุกเครื่อง)
        NetworkObject.Despawn();
    }

    // ----------------------------------------------------
    // 🕹️ Gameplay Hooks
    // ----------------------------------------------------

    public void OnTriggerEnter(Collider other)
    {
        // ต้องเป็น Server หรือ Owner ที่ต้องการส่ง RPC
        if (NetworkManager.Singleton.IsClient)
        {
            if (other.tag == "Player")
            {
                Player collector = other.GetComponent<Player>();

                if (collector != null && collector.IsOwner) // 💡 ต้องเป็น Local Player ที่ชน
                {
                    // Client ส่งคำขอเก็บไปยัง Server
                    RequestCollectServerRpc(collector.NetworkObject);
                }
            }
        }
    }

    public virtual void OnCollect(Player player)
    {
        // 🚨 Logic การเก็บ Item จริงๆ จะรันบน Server
        player.AddItem(this); // สมมติว่า Player มี AddItem ที่จัดการ Inventory
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
    public void RequestCollectServerRpc(NetworkObjectReference collectorNetRef)
    {
        if (!IsServer) return;

        if (!collectorNetRef.TryGet(out NetworkObject collectorNetObj)) return;
        Player collector = collectorNetObj.GetComponent<Player>();

        // ตรวจสอบความถูกต้อง (ต้องมีผู้เก็บและ Object ต้องยังอยู่)
        if (collector == null || !NetworkObject.IsSpawned) return;
        // 💡 ไม่ต้องเช็ค _isCollectable.Value อีกต่อไป

        // 1. Server เรียก Hook การเก็บ
        OnCollect(collector);

        // 2. Server แจ้ง Log และสั่งทำลาย
        LogCollectedClientRpc(new FixedString32Bytes(collector.Name), new FixedString32Bytes(Name));

        HandleDestroyed(); // สั่ง Despawn/บันทึก ID
    }

    [ClientRpc]
    public virtual void LogCollectedClientRpc(FixedString32Bytes playerName, FixedString32Bytes itemName)
    {
        Debug.Log($"📢 Global Log: {playerName.ToString()} collected {itemName.ToString()}!");
    }

    // (Constructors ถูกลบออกเนื่องจากไม่จำเป็นใน MonoBehaviour)
}