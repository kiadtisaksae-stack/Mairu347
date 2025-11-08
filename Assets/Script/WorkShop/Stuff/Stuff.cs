using TMPro;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public abstract class Stuff : Identity
{
    public TMP_Text interactionTextUI;
    protected Collider _collider;
    public bool isLock = true;

    public override void SetUP()
    {
        interactionTextUI = GetComponentInChildren<TMP_Text>();
        _collider = GetComponent<Collider>();
    }
    public void Update()
    {
        if (LocalPlayerTransform == null) return;
        if (GetDistanPlayer() >= 2f || !isLock)
        {
            interactionTextUI.gameObject.SetActive(false);
        }
        else
        {
            interactionTextUI.gameObject.SetActive(true);
        }
    }
    public virtual void OnIsOnChanged(bool previousValue, bool newValue)
    {

    }
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void ToggleTorchServerRpc(bool newState)
    {
        
    }

}
