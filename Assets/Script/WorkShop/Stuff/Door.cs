using System.Collections;
using TMPro;
using UnityEngine;

public class Door : Stuff, IInteractable
{
    public Door() {
        Name = "Door";
    }
    // ตัวแปรสำหรับระบุว่าประตูถูกเปิดอยู่หรือไม่
    private bool isOpen = false;

    // กำหนดตำแหน่งที่ประตูจะเลื่อนไป
    public Vector3 openOffset = new Vector3(0, 0, 2f);

    // ความเร็วในการเลื่อนประตู
    public float slideSpeed = 2f;
    public Transform door;

    public bool isInteractable { get => isLock; set => isLock = value; }

    public void Interact(Player player)
    {
        // หยุด Coroutine เก่าก่อนเริ่ม Coroutine ใหม่
        StopAllCoroutines();

        // ถ้าประตูเปิดอยู่ ให้ปิดประตู
        if (isOpen)
        {
            StartCoroutine(SlideDoor(door.position - openOffset));
        }
        // ถ้าประตูปิดอยู่ ให้เปิดประตู
        else
        {
            StartCoroutine(SlideDoor(door.position + openOffset));
        }

        // สลับสถานะของประตู
        isOpen = !isOpen;
    }

    private IEnumerator SlideDoor(Vector3 targetPosition)
    {
        Vector3 startPosition = door.position;
        float timeElapsed = 0;

        // ลูปนี้จะทำงานไปเรื่อยๆ ตราบเท่าที่ประตูยังเลื่อนไปไม่ถึงตำแหน่งเป้าหมาย
        while (timeElapsed < 1)
        {
            // คำนวณตำแหน่งใหม่ของประตูแบบนุ่มนวล
            timeElapsed += Time.deltaTime * slideSpeed;
            door.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed);
            yield return null; // รอจนกว่าจะถึงเฟรมถัดไป
        }

        // ตรวจสอบให้แน่ใจว่าประตูอยู่ที่ตำแหน่งสุดท้ายที่ถูกต้อง
        door.position = targetPosition;
    }

}
