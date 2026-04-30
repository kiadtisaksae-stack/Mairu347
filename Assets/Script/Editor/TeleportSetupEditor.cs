#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(TeleportSetup))]
public class TeleportSetupEditor : Editor
{
    private SerializedProperty linksProperty;
    private SerializedProperty defaultColliderSize;
    private SerializedProperty defaultBidirectional;

    private List<bool> linkFoldouts = new List<bool>();

    // Colors
    private static readonly Color HeaderColor = new Color(0.2f, 0.55f, 0.85f, 1f);
    private static readonly Color LinkEvenColor = new Color(0.2f, 0.2f, 0.22f, 1f);
    private static readonly Color LinkOddColor = new Color(0.24f, 0.24f, 0.26f, 1f);
    private static readonly Color SetupBtnColor = new Color(0.2f, 0.75f, 0.35f, 1f);
    private static readonly Color ClearBtnColor = new Color(0.85f, 0.3f, 0.3f, 1f);
    private static readonly Color AddBtnColor = new Color(0.3f, 0.65f, 0.85f, 1f);
    private static readonly Color DangerColor = new Color(0.85f, 0.2f, 0.2f, 1f);
    private static readonly Color WarningColor = new Color(0.95f, 0.75f, 0.1f, 1f);

    private void OnEnable()
    {
        linksProperty = serializedObject.FindProperty("links");
        defaultColliderSize = serializedObject.FindProperty("defaultColliderSize");
        defaultBidirectional = serializedObject.FindProperty("defaultBidirectional");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Title Header ──
        DrawBigHeader("🌀 Teleport Setup");

        EditorGUILayout.Space(6);

        // ── Default Settings ──
        DrawSectionHeader("⚙️ Default Settings");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(defaultColliderSize, new GUIContent("📦 Collider Size"));
        EditorGUILayout.PropertyField(defaultBidirectional, new GUIContent("↔️ Bidirectional"));
        EditorGUI.indentLevel--;

        EditorGUILayout.Space(8);

        // ── Action Buttons ──
        DrawSectionHeader("🔧 Actions");
        EditorGUILayout.Space(4);

        EditorGUILayout.BeginHorizontal();

        // Setup button
        GUI.backgroundColor = SetupBtnColor;
        if (GUILayout.Button("✅ Setup All Points", GUILayout.Height(32)))
        {
            TeleportSetup setup = (TeleportSetup)target;
            Undo.RegisterFullObjectHierarchyUndo(setup.gameObject, "Setup Teleport Points");

            // Register undo for all linked objects too
            foreach (var link in setup.links)
            {
                if (link.pointA != null)
                    Undo.RegisterFullObjectHierarchyUndo(link.pointA.gameObject, "Setup Teleport Point");
                if (link.pointB != null)
                    Undo.RegisterFullObjectHierarchyUndo(link.pointB.gameObject, "Setup Teleport Point");
            }

            setup.SetupAllPoints();
            EditorUtility.SetDirty(setup);
        }

        // Clear button
        GUI.backgroundColor = ClearBtnColor;
        if (GUILayout.Button("🗑 Clear All", GUILayout.Height(32), GUILayout.Width(100)))
        {
            if (EditorUtility.DisplayDialog("Clear All Points?",
                "ลบ Collider + TeleportPoint ทั้งหมดที่ Setup สร้างไว้?", "Yes", "No"))
            {
                TeleportSetup setup = (TeleportSetup)target;
                setup.ClearAllPoints();
                EditorUtility.SetDirty(setup);
            }
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Quick create empty GameObjects
        GUI.backgroundColor = new Color(0.6f, 0.4f, 0.8f, 1f);
        if (GUILayout.Button("🆕 Create Empty Teleport Pair (A + B)", GUILayout.Height(26)))
        {
            CreateEmptyTeleportPair();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // ── Links List ──
        int linkCount = linksProperty.arraySize;
        DrawSectionHeader($"🔗 Teleport Links  [{linkCount}]");

        // Sync foldouts
        while (linkFoldouts.Count < linkCount) linkFoldouts.Add(true);
        while (linkFoldouts.Count > linkCount) linkFoldouts.RemoveAt(linkFoldouts.Count - 1);

        // Validation summary
        int warningCount = CountWarnings();
        if (warningCount > 0)
        {
            EditorGUILayout.HelpBox($"⚠️ {warningCount} link(s) มีจุดที่ยังไม่ได้กำหนด!", MessageType.Warning);
        }

        EditorGUILayout.Space(4);

        for (int i = 0; i < linkCount; i++)
        {
            DrawLinkItem(i);
        }

        EditorGUILayout.Space(6);

        // Add link button
        GUI.backgroundColor = AddBtnColor;
        if (GUILayout.Button("＋ Add Teleport Link", GUILayout.Height(28)))
        {
            linksProperty.InsertArrayElementAtIndex(linksProperty.arraySize);
            SerializedProperty newLink = linksProperty.GetArrayElementAtIndex(linksProperty.arraySize - 1);

            // Set defaults
            newLink.FindPropertyRelative("pointA").objectReferenceValue = null;
            newLink.FindPropertyRelative("pointB").objectReferenceValue = null;
            newLink.FindPropertyRelative("isBidirectional").boolValue =
                defaultBidirectional.boolValue;
            newLink.FindPropertyRelative("colliderSize").vector3Value =
                defaultColliderSize.vector3Value;
            newLink.FindPropertyRelative("gizmoColor").colorValue =
                GetNextColor(linksProperty.arraySize - 1);
            newLink.FindPropertyRelative("linkName").stringValue = $"Link {linksProperty.arraySize}";

            linkFoldouts.Add(true);
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(8);

        // ── Save & Apply ──
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.95f, 0.7f, 0.1f, 1f);
        if (GUILayout.Button("💾 Save & Apply", GUILayout.Height(34)))
        {
            // 1. Apply serialized changes
            serializedObject.ApplyModifiedProperties();

            // 2. Mark dirty
            TeleportSetup setup = (TeleportSetup)target;
            EditorUtility.SetDirty(setup);

            // 3. Register undo for linked objects
            foreach (var link in setup.links)
            {
                if (link.pointA != null)
                    Undo.RegisterFullObjectHierarchyUndo(link.pointA.gameObject, "Save Teleport Setup");
                if (link.pointB != null)
                    Undo.RegisterFullObjectHierarchyUndo(link.pointB.gameObject, "Save Teleport Setup");
            }

            // 4. Re-setup all points (ล้างของเก่า + สร้างใหม่ตาม config ปัจจุบัน)
            setup.SetupAllPoints();

            // 5. Save scene
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());

            Debug.Log("💾 Teleport Setup saved & applied! Colliders rebuilt.");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Info box
        EditorGUILayout.HelpBox(
            "📌 ขั้นตอนการใช้:\n" +
            "1. ลาก Empty GameObject ไปใส่ช่อง Source (A) / Destination (B)\n" +
            "2. ตั้งค่า Two-Way, Collider Size ตามต้องการ\n" +
            "3. กด 💾 Save & Apply เพื่อสร้าง Collider + TeleportPoint ให้อัตโนมัติ\n" +
            "⚠️ ต้องกด Save & Apply ทุกครั้งที่แก้ค่า!",
            MessageType.Info);

        EditorGUILayout.Space(10);

        serializedObject.ApplyModifiedProperties();
    }

    // ═══════════════════════════════════════════════
    // Draw Link Item
    // ═══════════════════════════════════════════════

    private void DrawLinkItem(int index)
    {
        SerializedProperty link = linksProperty.GetArrayElementAtIndex(index);
        SerializedProperty pointA = link.FindPropertyRelative("pointA");
        SerializedProperty pointB = link.FindPropertyRelative("pointB");
        SerializedProperty biDir = link.FindPropertyRelative("isBidirectional");
        SerializedProperty colSize = link.FindPropertyRelative("colliderSize");
        SerializedProperty gizmoColor = link.FindPropertyRelative("gizmoColor");
        SerializedProperty linkName = link.FindPropertyRelative("linkName");

        // Background
        var bgRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(bgRect, index % 2 == 0 ? LinkEvenColor : LinkOddColor);

        // ── Header Row ──
        EditorGUILayout.BeginHorizontal();

        // Color indicator
        Rect colorRect = GUILayoutUtility.GetRect(6, 20, GUILayout.Width(6));
        EditorGUI.DrawRect(colorRect, gizmoColor.colorValue);

        // Foldout
        string displayName = string.IsNullOrEmpty(linkName.stringValue) ? $"Link {index + 1}" : linkName.stringValue;
        string pointAName = pointA.objectReferenceValue != null ? ((Transform)pointA.objectReferenceValue).name : "???";
        string pointBName = pointB.objectReferenceValue != null ? ((Transform)pointB.objectReferenceValue).name : "???";
        string arrow = biDir.boolValue ? "↔" : "→";
        string preview = $"  {displayName}  ({pointAName} {arrow} {pointBName})";

        bool hasWarning = pointA.objectReferenceValue == null || pointB.objectReferenceValue == null;
        if (hasWarning)
        {
            GUIStyle warnStyle = new GUIStyle(EditorStyles.foldout) { normal = { textColor = WarningColor } };
            linkFoldouts[index] = EditorGUILayout.Foldout(linkFoldouts[index], preview, true, warnStyle);
        }
        else
        {
            linkFoldouts[index] = EditorGUILayout.Foldout(linkFoldouts[index], preview, true);
        }

        GUILayout.FlexibleSpace();

        // Move buttons
        GUI.enabled = index > 0;
        if (GUILayout.Button("▲", GUILayout.Width(22)))
            linksProperty.MoveArrayElement(index, index - 1);

        GUI.enabled = index < linksProperty.arraySize - 1;
        if (GUILayout.Button("▼", GUILayout.Width(22)))
            linksProperty.MoveArrayElement(index, index + 1);

        GUI.enabled = true;

        // Delete button
        GUI.backgroundColor = DangerColor;
        if (GUILayout.Button("✕", GUILayout.Width(22)))
        {
            linksProperty.DeleteArrayElementAtIndex(index);
            linkFoldouts.RemoveAt(index);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            return;
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        // ── Expanded Content ──
        if (linkFoldouts[index])
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(4);

            // Link name
            EditorGUILayout.PropertyField(linkName, new GUIContent("🏷 Link Name"));

            EditorGUILayout.Space(4);

            // Points A → B (visual layout)
            EditorGUILayout.BeginHorizontal();

            // Point A
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(100));
            EditorGUILayout.LabelField("📍 Source (A)", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.PropertyField(pointA, GUIContent.none);
            if (pointA.objectReferenceValue != null)
            {
                Transform t = (Transform)pointA.objectReferenceValue;
                EditorGUILayout.LabelField($"Pos: {t.position.x:F1}, {t.position.y:F1}, {t.position.z:F1}",
                    EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            // Arrow
            GUILayout.Label(biDir.boolValue ? "  ↔️  " : "  →  ",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter
                },
                GUILayout.Width(50), GUILayout.Height(50));

            // Point B
            EditorGUILayout.BeginVertical("box", GUILayout.MinWidth(100));
            EditorGUILayout.LabelField("🎯 Destination (B)", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.PropertyField(pointB, GUIContent.none);
            if (pointB.objectReferenceValue != null)
            {
                Transform t = (Transform)pointB.objectReferenceValue;
                EditorGUILayout.LabelField($"Pos: {t.position.x:F1}, {t.position.y:F1}, {t.position.z:F1}",
                    EditorStyles.miniLabel);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            // Settings row
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(biDir, new GUIContent("↔ Two-Way"));
            EditorGUILayout.PropertyField(gizmoColor, new GUIContent("🎨 Color"));
            EditorGUILayout.EndHorizontal();

            // Collider size
            EditorGUILayout.PropertyField(colSize, new GUIContent("📦 Collider Size"));

            // Distance info
            if (pointA.objectReferenceValue != null && pointB.objectReferenceValue != null)
            {
                Transform tA = (Transform)pointA.objectReferenceValue;
                Transform tB = (Transform)pointB.objectReferenceValue;
                float dist = Vector3.Distance(tA.position, tB.position);
                EditorGUILayout.LabelField($"📏 Distance: {dist:F2} units", EditorStyles.miniLabel);
            }

            // Quick setup single link
            EditorGUILayout.Space(2);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (pointA.objectReferenceValue != null)
            {
                if (GUILayout.Button("📍 Select A", GUILayout.Width(80)))
                    Selection.activeTransform = (Transform)pointA.objectReferenceValue;
            }
            if (pointB.objectReferenceValue != null)
            {
                if (GUILayout.Button("🎯 Select B", GUILayout.Width(80)))
                    Selection.activeTransform = (Transform)pointB.objectReferenceValue;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    // ═══════════════════════════════════════════════
    // Helpers
    // ═══════════════════════════════════════════════

    private void CreateEmptyTeleportPair()
    {
        TeleportSetup setup = (TeleportSetup)target;

        int pairNum = setup.links.Count + 1;

        // สร้าง parent container
        GameObject container = new GameObject($"TeleportPair_{pairNum}");
        container.transform.SetParent(setup.transform);
        container.transform.localPosition = Vector3.zero;
        Undo.RegisterCreatedObjectUndo(container, "Create Teleport Pair");

        // สร้าง Point A
        GameObject pointA = new GameObject($"WarpPoint_A_{pairNum}");
        pointA.transform.SetParent(container.transform);
        pointA.transform.localPosition = new Vector3(-3f, 0f, 0f);
        Undo.RegisterCreatedObjectUndo(pointA, "Create Teleport Point A");

        // สร้าง Point B
        GameObject pointB = new GameObject($"WarpPoint_B_{pairNum}");
        pointB.transform.SetParent(container.transform);
        pointB.transform.localPosition = new Vector3(3f, 0f, 0f);
        Undo.RegisterCreatedObjectUndo(pointB, "Create Teleport Point B");

        // เพิ่ม link ใหม่
        serializedObject.Update();
        linksProperty.InsertArrayElementAtIndex(linksProperty.arraySize);
        SerializedProperty newLink = linksProperty.GetArrayElementAtIndex(linksProperty.arraySize - 1);
        newLink.FindPropertyRelative("pointA").objectReferenceValue = pointA.transform;
        newLink.FindPropertyRelative("pointB").objectReferenceValue = pointB.transform;
        newLink.FindPropertyRelative("isBidirectional").boolValue = defaultBidirectional.boolValue;
        newLink.FindPropertyRelative("colliderSize").vector3Value = defaultColliderSize.vector3Value;
        newLink.FindPropertyRelative("gizmoColor").colorValue = GetNextColor(linksProperty.arraySize - 1);
        newLink.FindPropertyRelative("linkName").stringValue = $"Pair {pairNum}";
        serializedObject.ApplyModifiedProperties();

        linkFoldouts.Add(true);

        // Select container
        Selection.activeGameObject = container;
        EditorUtility.SetDirty(setup);

        Debug.Log($"🆕 Created Teleport Pair {pairNum} with empty GameObjects");
    }

    private int CountWarnings()
    {
        int count = 0;
        for (int i = 0; i < linksProperty.arraySize; i++)
        {
            SerializedProperty link = linksProperty.GetArrayElementAtIndex(i);
            if (link.FindPropertyRelative("pointA").objectReferenceValue == null ||
                link.FindPropertyRelative("pointB").objectReferenceValue == null)
                count++;
        }
        return count;
    }

    private Color GetNextColor(int index)
    {
        Color[] palette = {
            Color.cyan,
            new Color(0.4f, 0.9f, 0.4f),
            new Color(0.9f, 0.6f, 0.2f),
            new Color(0.8f, 0.4f, 0.9f),
            new Color(0.9f, 0.9f, 0.3f),
            new Color(0.3f, 0.7f, 0.9f),
            new Color(0.9f, 0.4f, 0.5f),
        };
        return palette[index % palette.Length];
    }

    private void DrawBigHeader(string title)
    {
        var rect = GUILayoutUtility.GetRect(1f, 36f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, title, style);
    }

    private void DrawSectionHeader(string title)
    {
        var rect = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, HeaderColor);

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = Color.white },
            fontSize = 11,
            alignment = TextAnchor.MiddleLeft
        };
        GUI.Label(rect, $"  {title}", style);
        EditorGUILayout.Space(2);
    }
}
#endif
