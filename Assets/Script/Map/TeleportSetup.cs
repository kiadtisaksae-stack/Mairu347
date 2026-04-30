using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// TeleportSetup — วางไว้ในทุก Scene เพื่อรวบจุด warp ทั้งหมด
/// ลาก Empty GameObject เปล่าๆ มาใส่ใน Inspector ได้เลย
/// กด "Setup All Points" แล้วระบบจะเพิ่ม Collider + TeleportPoint ให้อัตโนมัติ
/// </summary>
public class TeleportSetup : MonoBehaviour
{
    [Header("Teleport Links")]
    public List<TeleportLink> links = new List<TeleportLink>();

    [Header("Default Settings")]
    [Tooltip("ขนาด BoxCollider ที่จะสร้างให้แต่ละจุด")]
    public Vector3 defaultColliderSize = new Vector3(2f, 3f, 2f);

    [Tooltip("ต้องการให้ teleport สองทาง (A↔B) หรือทางเดียว (A→B)")]
    public bool defaultBidirectional = true;

    /// <summary>
    /// Setup ทุกจุดใน links — ล้างของเก่าก่อน แล้วเพิ่ม Collider + TeleportPoint ใหม่
    /// ต้องกด Setup ใหม่ทุกครั้งที่แก้ค่าใน Editor
    /// </summary>
    public void SetupAllPoints()
    {
        // ─── ล้างของเก่าทั้งหมดก่อน ───
        // สำคัญ! ถ้าไม่ล้าง → เปลี่ยน Two-Way เป็น One-Way จะไม่มีผล
        // เพราะ TeleportPoint ตัวเก่ายังค้างอยู่ที่จุด B
        ClearAllPoints();

        int setupCount = 0;

        foreach (var link in links)
        {
            // A → B (ทำเสมอ)
            if (link.pointA != null && link.pointB != null)
            {
                SetupPoint(link.pointA, link.pointB, link.colliderSize);
                setupCount++;
            }

            // B → A (ทำเฉพาะเมื่อ Two-Way = true)
            if (link.isBidirectional && link.pointB != null && link.pointA != null)
            {
                SetupPoint(link.pointB, link.pointA, link.colliderSize);
                setupCount++;
            }
        }

        Debug.Log($"✅ TeleportSetup: Setup {setupCount} teleport points from {links.Count} links");
    }

    /// <summary>
    /// ลบ component และ collider ที่ Setup สร้างไว้ทั้งหมด
    /// </summary>
    public void ClearAllPoints()
    {
        foreach (var link in links)
        {
            ClearPoint(link.pointA);
            ClearPoint(link.pointB);
        }
        Debug.Log("🗑 TeleportSetup: Cleared all teleport points");
    }

    private void SetupPoint(Transform source, Transform destination, Vector3 colliderSize)
    {
        if (source == null) return;

        // เพิ่ม BoxCollider (ถ้ายังไม่มี)
        BoxCollider col = source.GetComponent<BoxCollider>();
        if (col == null)
            col = source.gameObject.AddComponent<BoxCollider>();

        col.isTrigger = true;
        col.size = colliderSize;

        // เพิ่ม TeleportPoint component (ถ้ายังไม่มี)
        TeleportPoint tp = source.GetComponent<TeleportPoint>();
        if (tp == null)
            tp = source.gameObject.AddComponent<TeleportPoint>();

        tp.destination = destination;

        // ตั้งค่า tag
        source.gameObject.tag = "Untagged";

        // ตั้งค่า layer (ถ้าต้องการ)
        // source.gameObject.layer = LayerMask.NameToLayer("Teleport");
    }

    private void ClearPoint(Transform point)
    {
        if (point == null) return;

        TeleportPoint tp = point.GetComponent<TeleportPoint>();
        if (tp != null) DestroyImmediate(tp);

        BoxCollider col = point.GetComponent<BoxCollider>();
        if (col != null) DestroyImmediate(col);
    }

    // ─────────────────────────────────────────────
    // Gizmos — แสดงเส้นเชื่อมจุดใน Scene View
    // ─────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (links == null) return;

        foreach (var link in links)
        {
            if (link.pointA == null || link.pointB == null) continue;

            Vector3 posA = link.pointA.position;
            Vector3 posB = link.pointB.position;

            // เส้นเชื่อม
            Gizmos.color = link.gizmoColor;
            Gizmos.DrawLine(posA, posB);

            // ลูกศร
            DrawArrowGizmo(posA, posB, link.gizmoColor);
            if (link.isBidirectional)
                DrawArrowGizmo(posB, posA, link.gizmoColor);

            // Collider preview
            Gizmos.color = new Color(link.gizmoColor.r, link.gizmoColor.g, link.gizmoColor.b, 0.3f);
            Gizmos.DrawWireCube(posA, link.colliderSize);
            Gizmos.DrawWireCube(posB, link.colliderSize);

            // Labels
            Gizmos.color = Color.white;
            Gizmos.DrawSphere(posA, 0.15f);
            Gizmos.DrawSphere(posB, 0.15f);
        }
    }

    private void DrawArrowGizmo(Vector3 from, Vector3 to, Color color)
    {
        Gizmos.color = color;
        Vector3 direction = (to - from);
        if (direction.sqrMagnitude < 0.01f) return;

        Vector3 midPoint = from + direction * 0.7f;
        float arrowSize = 0.4f;
        Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 200, 0) * Vector3.forward;
        Gizmos.DrawRay(midPoint, right * arrowSize);
        Gizmos.DrawRay(midPoint, left * arrowSize);
    }
}

// ─────────────────────────────────────────────────────
// Data Classes
// ─────────────────────────────────────────────────────

[Serializable]
public class TeleportLink
{
    [Tooltip("จุดต้นทาง — ลาก Empty GameObject มาวาง")]
    public Transform pointA;

    [Tooltip("จุดปลายทาง — ลาก Empty GameObject มาวาง")]
    public Transform pointB;

    [Tooltip("Teleport สองทาง (A↔B) หรือทางเดียว (A→B)")]
    public bool isBidirectional = true;

    [Tooltip("ขนาด BoxCollider ที่จุด warp")]
    public Vector3 colliderSize = new Vector3(2f, 3f, 2f);

    [Tooltip("สีเส้นใน Scene View")]
    public Color gizmoColor = Color.cyan;

    [Tooltip("ชื่อ link สำหรับแสดงใน Editor")]
    public string linkName = "";
}
