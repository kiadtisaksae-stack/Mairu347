using System;
using UnityEngine;

public class GoodLExample : MonoBehaviour
{
    public class Animal // คลาสพื้นฐานสำหรับสัตว์
    {
        public virtual void Eat()
        {
            Console.WriteLine("Animal is eating.");
        }
    }


   
}
