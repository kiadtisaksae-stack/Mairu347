using System;
using UnityEngine;

namespace Theory.Week10
{
    public enum WeaponType
    {
        Sword,
        Bow,
        Gun
    }

    public class Weapon : MonoBehaviour
    {
        public WeaponType Type { get; set; }
        public int BaseDamage { get; set; }

        public Weapon(WeaponType type, int baseDamage)
        {
            Type = type;
            BaseDamage = baseDamage;
        }
    }

   
}
