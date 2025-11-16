using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ItemOBJ : NetworkBehaviour
{
    public ItemSO item;
    public NetworkVariable<int> amount = new NetworkVariable<int>(1,
    NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public TextMeshProUGUI amountText;

    private Rigidbody rb;
    private bool locallyPickedUp = false;
    private NetworkVariable<bool> isPickedUp = new NetworkVariable<bool>(false);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            rb = GetComponent<Rigidbody>();
        }

        // ตรวจสอบว่า item ถูกเก็บไปแล้วหรือไม่
        if (isPickedUp.Value)
        {
            gameObject.SetActive(false);
        }

        // เริ่มต้นอัพเดต UI ด้วยค่าปัจจุบัน
        if (amountText) amountText.text = amount.Value.ToString();

        // สับสคริบการเปลี่ยนแปลงค่า amount
        amount.OnValueChanged += OnAmountChanged;
    }
    private void OnAmountChanged(int previousValue, int newValue)
    {
        // อัพเดต UI เมื่อค่า amount เปลี่ยนแปลง
        if (amountText) amountText.text = newValue.ToString();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (locallyPickedUp || isPickedUp.Value) return;

        if (other.CompareTag("Player") && other.GetComponent<NetworkObject>().IsOwner)
        {
            // ป้องกันการเรียกซ้ำ
            locallyPickedUp = true;
            GetComponent<Collider>().enabled = false;

            PickupItem(other.GetComponent<NetworkObject>().OwnerClientId);
        }
    }

    private void PickupItem(ulong playerId)
    {
        AddItemToInventoryLocal();

        if (IsServer)
        {
            isPickedUp.Value = true;
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
        if (isPickedUp.Value) return;
        isPickedUp.Value = true;
        NetworkObject.Despawn(true);
    }

    private void AddItemToInventoryLocal()
    {
        InventoryCanvas invCanvas = FindFirstObjectByType<InventoryCanvas>();
        if (invCanvas != null)
        {
            invCanvas.AddItem(item, amount.Value);
            Debug.Log("Local pickup: " + item.itemName + " x" + amount.Value);

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