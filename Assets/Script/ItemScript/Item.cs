using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Item : Identity
{
    public ItemSO item;
    public NetworkVariable<int> amount = new NetworkVariable<int>(1,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public TextMeshProUGUI amountText;
    private Collider _collider;

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
        this.Name = item.itemName;
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (itemcollider != null)
        {
            itemcollider.enabled = true;
        }
        if (amountText) amountText.text = amount.Value.ToString();

        // สับสคริบการเปลี่ยนแปลงค่า amount
        amount.OnValueChanged += OnAmountChanged;
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
    private void OnAmountChanged(int previousValue, int newValue)
    {
        // อัพเดต UI เมื่อค่า amount เปลี่ยนแปลง
        if (amountText) amountText.text = newValue.ToString();
    }
    public void HandleDestroyed()
    {
        if (!IsServer) return;
        NetworkObject.Despawn();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (isOnLive.Value == false) return;

        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            GetComponent<Collider>().enabled = false;

            PickupItem(other.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void PickupItem(ulong playerId)
    {
        AddItemToInventoryLocal();

        if (IsServer)
        {
            isOnLive.Value = true;
            NetworkObject.Despawn(true);
        }
        else
        {
            MarkAsPickedUpServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void MarkAsPickedUpServerRpc()
    {
        if (isOnLive.Value) return;
        isOnLive.Value = true;
        NetworkObject.Despawn(true);
    }

    private void AddItemToInventoryLocal()
    {
        InventoryCanvas invCanvas = FindFirstObjectByType<InventoryCanvas>();
        if (invCanvas != null)
        {
            invCanvas.AddItem(item, amount.Value);
            Debug.Log("Local pickup: " + item.itemName + " x" + amount.Value);

            // ✅ แจ้ง Quest Manager
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnItemCollected(item);
            }

            // ปิดการแสดงผลทันที (client-side prediction)
            gameObject.SetActive(false);
        }
    }

    public void SetAmount(int newAmount)
    {
        amount.Value = newAmount;
        if (amountText) amountText.text = amount.Value.ToString();
    }

    public void RandomAmount()
    {
        if (IsServer)
        {
            amount.Value = Random.Range(1, item.maxStack + 1);
            if (amountText) amountText.text = amount.Value.ToString();
        }
        else
        {
            RandomAmountServerRpc();
        }

    }
    [ServerRpc(RequireOwnership = false)]
    private void RandomAmountServerRpc()
    {
        amount.Value = Random.Range(1, item.maxStack + 1);

    }



}