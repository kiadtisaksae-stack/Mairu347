using System;
using UnityEngine;

public class BadLExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        BirdHandler handler = new BirdHandler();
        handler.MakeBirdFly(new Bird()); // OK
        try
        {
            handler.MakeBirdFly(new Ostrich()); // ERROR! Program crashes
        }
        catch (InvalidOperationException ex)
        {
            Debug.Log($"LSP Violation: {ex.Message}");
        }
    }

    public class Bird // คลาสแม่: นก
    {
        public virtual void Fly()
        {
            Debug.Log("Bird is flying.");
        }

        public virtual void LayEggs()
        {
            Debug.Log("Bird is laying eggs.");
        }
    }

    public class Ostrich : Bird // นกกระจอกเทศเป็นนก แต่บินไม่ได้
    {
        public override void Fly()
        {
            // ปัญหา: นกกระจอกเทศบินไม่ได้ แต่ต้อง implement เมธอดนี้
            // อาจจะโยน Exception หรือทำอะไรที่ผิดปกติ
            throw new InvalidOperationException("Ostrich cannot fly!");
            // Console.WriteLine("Ostrich waddles instead of flying.");
        }
    }

    public class BirdHandler
    {
        public void MakeBirdFly(Bird bird)
        {
            bird.Fly(); // ถ้าส่ง Ostrich มาตรงนี้จะพัง
        }
    }

}
