using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("inventory")]
    public List<ItemData> inventory = new List<ItemData>();
    public List<GameObject> WeaponRigthHand;
    public List<GameObject> ShiledLeftHand;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void FixedUpdate()
    {

    }
    public void ActivateRightHandWeapon(int indexToActivate)
    {
        for (int i = 0; i < WeaponRigthHand.Count; i++)
        {
            if (WeaponRigthHand[i] == null) continue;
            WeaponRigthHand[i].SetActive(i == indexToActivate);
        }
    }
    
    public void ActivateLeftHandWeapon(int indexToActivate)
    {
        for (int i = 0; i < ShiledLeftHand.Count; i++)
        {
            if (ShiledLeftHand[i] == null) continue;
            ShiledLeftHand[i].SetActive(i == indexToActivate);
        }
    }

}
