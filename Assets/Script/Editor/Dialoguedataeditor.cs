// วางไฟล์นี้ใน Assets/Editor/DialogueDataEditor.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(DialogueData))]
public class DialogueDataEditor : Editor
{
    // foldout state ของแต่ละ step
    private List<bool> _stepFoldouts = new List<bool>();

    private SerializedProperty _npcName;
    private SerializedProperty _npcVoice;
    private SerializedProperty _steps;
    private SerializedProperty _questData;
    private SerializedProperty _questAcceptText;
    private SerializedProperty _questCompletedText;

    // สี header
    private static readonly Color HeaderColor = new Color(0.2f, 0.5f, 0.8f, 1f);
    private static readonly Color StepEvenColor = new Color(0.22f, 0.22f, 0.22f, 1f);
    private static readonly Color StepOddColor = new Color(0.26f, 0.26f, 0.26f, 1f);
    private static readonly Color DangerColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    private void OnEnable()
    {
        _npcName = serializedObject.FindProperty("npcName");
        _npcVoice = serializedObject.FindProperty("npcVoiceSound");
        _steps = serializedObject.FindProperty("steps");
        _questData = serializedObject.FindProperty("questData");
        _questAcceptText = serializedObject.FindProperty("questAcceptText");
        _questCompletedText = serializedObject.FindProperty("questCompletedText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Header ───────────────────────────────────────
        DrawSectionHeader("🗣 NPC Info");
        EditorGUILayout.PropertyField(_npcName, new GUIContent("NPC Name"));
        EditorGUILayout.PropertyField(_npcVoice, new GUIContent("Voice Sound"));

        EditorGUILayout.Space(8);

        // ── Quest ─────────────────────────────────────────
        DrawSectionHeader("📋 Quest (optional)");
        EditorGUILayout.PropertyField(_questData, new GUIContent("Quest Data"));

        if (_questData.objectReferenceValue != null)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(_questAcceptText, new GUIContent("Accept Text"));
            EditorGUILayout.PropertyField(_questCompletedText, new GUIContent("Completed Text"));

            // preview requireDelivery จาก quest SO
            QuestData q = _questData.objectReferenceValue as QuestData;
            if (q != null)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Toggle("Require Delivery", q.requireDelivery);
                EditorGUI.EndDisabledGroup();
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(8);

        // ── Steps ─────────────────────────────────────────
        DrawSectionHeader($"💬 Dialogue Steps  [{_steps.arraySize}]");

        // sync foldout list
        while (_stepFoldouts.Count < _steps.arraySize) _stepFoldouts.Add(true);
        while (_stepFoldouts.Count > _steps.arraySize) _stepFoldouts.RemoveAt(_stepFoldouts.Count - 1);

        for (int i = 0; i < _steps.arraySize; i++)
        {
            SerializedProperty step = _steps.GetArrayElementAtIndex(i);
            DrawStep(step, i);
        }

        EditorGUILayout.Space(4);

        // ── Add / Clear ───────────────────────────────────
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("＋ Add Step", GUILayout.Height(28)))
            {
                _steps.InsertArrayElementAtIndex(_steps.arraySize);
                _stepFoldouts.Add(true);
            }

            GUI.backgroundColor = DangerColor;
            if (GUILayout.Button("🗑 Clear All", GUILayout.Height(28), GUILayout.Width(100)))
            {
                if (EditorUtility.DisplayDialog("Clear All Steps?",
                    "ลบทุก step ใน Dialogue นี้?", "Yes", "No"))
                {
                    _steps.ClearArray();
                    _stepFoldouts.Clear();
                }
            }
            GUI.backgroundColor = Color.white;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────
    private void DrawStep(SerializedProperty step, int index)
    {
        // สีสลับแถว
        var bgRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(bgRect, index % 2 == 0 ? StepEvenColor : StepOddColor);

        // ── Foldout header ──
        using (new EditorGUILayout.HorizontalScope())
        {
            string preview = step.FindPropertyRelative("npcText").stringValue;
            if (preview.Length > 40) preview = preview.Substring(0, 40) + "…";
            if (string.IsNullOrEmpty(preview)) preview = "(empty)";

            _stepFoldouts[index] = EditorGUILayout.Foldout(
                _stepFoldouts[index],
                $"  Step {index + 1}  —  {preview}",
                true);

            GUILayout.FlexibleSpace();

            // ลูกศรเลื่อน
            GUI.enabled = index > 0;
            if (GUILayout.Button("▲", GUILayout.Width(24)))
                _steps.MoveArrayElement(index, index - 1);

            GUI.enabled = index < _steps.arraySize - 1;
            if (GUILayout.Button("▼", GUILayout.Width(24)))
                _steps.MoveArrayElement(index, index + 1);

            GUI.enabled = true;

            // ลบ step
            GUI.backgroundColor = DangerColor;
            if (GUILayout.Button("✕", GUILayout.Width(24)))
            {
                _steps.DeleteArrayElementAtIndex(index);
                _stepFoldouts.RemoveAt(index);
                EditorGUILayout.EndVertical();
                return;
            }
            GUI.backgroundColor = Color.white;
        }

        if (_stepFoldouts[index])
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Space(2);

            // Dialogue text
            SerializedProperty npcText = step.FindPropertyRelative("npcText");
            EditorGUILayout.LabelField("NPC Text", EditorStyles.boldLabel);
            npcText.stringValue = EditorGUILayout.TextArea(npcText.stringValue,
                GUILayout.MinHeight(50));

            EditorGUILayout.Space(4);

            // Sprites
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(step.FindPropertyRelative("npcFace"),
                    new GUIContent("NPC Face"), GUILayout.Width(EditorGUIUtility.currentViewWidth / 2 - 20));
                EditorGUILayout.PropertyField(step.FindPropertyRelative("playerFace"),
                    new GUIContent("Player Face"));
            }

            EditorGUILayout.Space(4);

            // Buttons text
            using (new EditorGUILayout.HorizontalScope())
            {
                SerializedProperty b1 = step.FindPropertyRelative("button1Text");
                SerializedProperty b2 = step.FindPropertyRelative("button2Text");
                EditorGUILayout.LabelField("Button 1", GUILayout.Width(60));
                b1.stringValue = EditorGUILayout.TextField(b1.stringValue);
                EditorGUILayout.LabelField("Button 2", GUILayout.Width(60));
                b2.stringValue = EditorGUILayout.TextField(b2.stringValue);
            }

            EditorGUILayout.Space(4);

            // Flags
            using (new EditorGUILayout.HorizontalScope())
            {
                SerializedProperty canQuit = step.FindPropertyRelative("canQuitHere");
                SerializedProperty ends = step.FindPropertyRelative("endsConversation");
                canQuit.boolValue = EditorGUILayout.ToggleLeft("Can Quit Here", canQuit.boolValue, GUILayout.Width(130));
                ends.boolValue = EditorGUILayout.ToggleLeft("Ends Conversation", ends.boolValue);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(2);
    }

    private void DrawSectionHeader(string title)
    {
        var rect = GUILayoutUtility.GetRect(1f, 22f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, HeaderColor);
        GUI.Label(rect, $"  {title}", new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = Color.white },
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        });
        EditorGUILayout.Space(2);
    }
}
#endif