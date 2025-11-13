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
        OnCollected?.Invoke(NetworkObjectId);
        NetworkObject.Despawn();
    }
    public void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // ✅ ให้ Server เป็นคนจัดการเท่านั้น

        if (other.CompareTag("Player"))
        {
            Player collector = other.GetComponent<Player>();
            if (collector != null)
            {
                OnCollect(collector);
                HandleDestroyed();
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



}