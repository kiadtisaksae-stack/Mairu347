using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Item : Identity
{
    private const float COLLECT_COOLDOWN_TIME = 2f;

    private readonly NetworkVariable<bool> _isCollectable = new NetworkVariable<bool>(
        true, // Default: collectable
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
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
        // Subscribe เพื่ออัปเดต Collider บนทุก Client เมื่อค่าเปลี่ยน
        _isCollectable.OnValueChanged += OnCollectableStateChanged;

        if (IsServer)
        {
            ApplyCollectCooldown();
        }

        // ตั้งค่าสถานะเริ่มต้นของ Collider บน Client ที่เข้ามาก่อน/หลัง
        UpdateColliderState(_isCollectable.Value);

    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        if (gameObject != null)
        {
            Destroy(gameObject);
        }

    }

    private void ApplyCollectCooldown()
    {
        if (!IsServer) return;

        //Server ตั้งค่าเป็น false ทันที (ซิงค์ไปยัง Client ทุกคน)
        _isCollectable.Value = false;

        Invoke(nameof(SetCollectableTrue), COLLECT_COOLDOWN_TIME);
    }
    private void SetCollectableTrue()
    {
        if (IsServer)
        {
            _isCollectable.Value = true;
        }
    }
    private void OnCollectableStateChanged(bool oldValue, bool newValue)
    {
        UpdateColliderState(newValue);
    }

    private void UpdateColliderState(bool isCollectable)
    {
        if (itemcollider != null)
        {
            // เปิด/ปิด Collider ตามสถานะที่ซิงค์มา
            itemcollider.enabled = isCollectable;

            if (isCollectable)
            {
                Debug.Log($"[ITEM] {Name} collider enabled (Collectable).");
            }
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
        if (!_isCollectable.Value) return;
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
