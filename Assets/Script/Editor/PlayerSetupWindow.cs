#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class PlayerSetupWindow : EditorWindow
{
    private PlayerSO selectedConfig;
    private SerializedObject serializedConfig;
    private Vector2 scrollPosition;

    // Section foldouts
    private bool showStats = true;
    private bool showMovement = true;
    private bool showInteract = true;
    private bool showCombat = true;
    private bool showLevelUp = true;

    // Section colors
    private static readonly Color StatsColor = new Color(0.85f, 0.25f, 0.25f, 1f);
    private static readonly Color MovementColor = new Color(0.2f, 0.6f, 0.9f, 1f);
    private static readonly Color InteractColor = new Color(0.3f, 0.75f, 0.4f, 1f);
    private static readonly Color CombatColor = new Color(0.9f, 0.55f, 0.1f, 1f);
    private static readonly Color LevelUpColor = new Color(0.7f, 0.4f, 0.9f, 1f);
    private static readonly Color BgDark = new Color(0.18f, 0.18f, 0.18f, 1f);
    private static readonly Color BgLight = new Color(0.22f, 0.22f, 0.22f, 1f);

    [MenuItem("Window/Game Tools/Player Setup")]
    public static void ShowWindow()
    {
        var window = GetWindow<PlayerSetupWindow>("🎮 Player Setup");
        window.minSize = new Vector2(400, 500);
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        DrawHeader();
        EditorGUILayout.Space(8);

        // Config asset selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Player Config Asset", EditorStyles.boldLabel, GUILayout.Width(140));
        selectedConfig = (PlayerSO)EditorGUILayout.ObjectField(selectedConfig, typeof(PlayerSO), false);
        EditorGUILayout.EndHorizontal();

        if (selectedConfig == null)
        {
            EditorGUILayout.Space(20);
            DrawCenteredMessage("📋 ลาก PlayerSO asset มาวางด้านบน\nหรือกด Create New เพื่อสร้างใหม่");
            EditorGUILayout.Space(10);
            if (GUILayout.Button("✨ Create New Player Config", GUILayout.Height(32)))
            {
                CreateNewPlayerConfig();
            }
            EditorGUILayout.EndScrollView();
            return;
        }

        // Refresh serialized object
        if (serializedConfig == null || serializedConfig.targetObject != selectedConfig)
            serializedConfig = new SerializedObject(selectedConfig);

        serializedConfig.Update();

        EditorGUILayout.Space(8);

        // ─── Stats Section ───
        DrawSectionHeader("❤️ Base Stats", StatsColor, ref showStats);
        if (showStats)
        {
            DrawSectionBackground(() =>
            {
                DrawIntSlider("Max Health", "initialMaxHealth", 1, 9999);
                DrawIntSlider("Base Damage", "baseDamage", 0, 999);
                DrawIntSlider("Base Defence", "baseDefence", 0, 999);
            });
        }

        EditorGUILayout.Space(4);

        // ─── Movement Section ───
        DrawSectionHeader("🏃 Movement", MovementColor, ref showMovement);
        if (showMovement)
        {
            DrawSectionBackground(() =>
            {
                DrawFloatSlider("Walk Speed", "walkSpeed", 0f, 20f);
                DrawFloatSlider("Sprint Speed", "sprintSpeed", 0f, 30f);
                DrawFloatSlider("Jump Force", "jumpForce", 0f, 30f);
                DrawFloatSlider("Gravity", "gravity", -50f, 0f);
                DrawFloatSlider("Rotation Smoothing", "rotationSmoothing", 1f, 30f);

                // Validation warning
                float walk = serializedConfig.FindProperty("walkSpeed").floatValue;
                float sprint = serializedConfig.FindProperty("sprintSpeed").floatValue;
                if (sprint <= walk)
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.HelpBox("⚠️ Sprint Speed ควรมากกว่า Walk Speed!", MessageType.Warning);
                }
            });
        }

        EditorGUILayout.Space(4);

        // ─── Interact Section ───
        DrawSectionHeader("🎯 Interact", InteractColor, ref showInteract);
        if (showInteract)
        {
            DrawSectionBackground(() =>
            {
                DrawFloatSlider("Sphere Radius", "interactSphereRadius", 0.1f, 5f);
                DrawFloatSlider("Max Distance", "interactMaxDistance", 0.5f, 10f);
            });
        }

        EditorGUILayout.Space(4);

        // ─── Combat Section ───
        DrawSectionHeader("⚔️ Combat", CombatColor, ref showCombat);
        if (showCombat)
        {
            DrawSectionBackground(() =>
            {
                SerializedProperty attackAnims = serializedConfig.FindProperty("attackAnimations");
                EditorGUILayout.LabelField("Attack Animations", EditorStyles.boldLabel);

                for (int i = 0; i < attackAnims.arraySize; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(attackAnims.GetArrayElementAtIndex(i), new GUIContent($"[{i}]"));
                    GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f, 1f);
                    if (GUILayout.Button("✕", GUILayout.Width(24)))
                    {
                        attackAnims.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUI.backgroundColor = Color.white;
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.Space(4);
                if (GUILayout.Button("＋ Add Animation", GUILayout.Height(24)))
                {
                    attackAnims.InsertArrayElementAtIndex(attackAnims.arraySize);
                }
            });
        }

        EditorGUILayout.Space(4);

        // ─── Level Up Section ───
        DrawSectionHeader("📈 Level Up Multipliers", LevelUpColor, ref showLevelUp);
        if (showLevelUp)
        {
            DrawSectionBackground(() =>
            {
                DrawFloatSlider("Health Multiplier", "healthMultiplier", 1f, 2f);
                DrawFloatSlider("Damage Multiplier", "damageMultiplier", 1f, 2f);
                DrawFloatSlider("Defence Multiplier", "defenceMultiplier", 1f, 2f);

                // Level Up Preview
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("📊 Level Up Preview (3 Levels)", EditorStyles.boldLabel);

                int hp = serializedConfig.FindProperty("initialMaxHealth").intValue;
                int dmg = serializedConfig.FindProperty("baseDamage").intValue;
                int def = serializedConfig.FindProperty("baseDefence").intValue;
                float hpMul = serializedConfig.FindProperty("healthMultiplier").floatValue;
                float dmgMul = serializedConfig.FindProperty("damageMultiplier").floatValue;
                float defMul = serializedConfig.FindProperty("defenceMultiplier").floatValue;

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(20);
                EditorGUILayout.BeginVertical("box");

                // Header
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(60));
                EditorGUILayout.LabelField("Lv.1", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("Lv.2", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("Lv.3", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                EditorGUILayout.LabelField("Lv.4", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();

                DrawLevelPreviewRow("❤️ HP", hp, hpMul);
                DrawLevelPreviewRow("⚔️ DMG", dmg, dmgMul);
                DrawLevelPreviewRow("🛡️ DEF", def, defMul);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            });
        }

        EditorGUILayout.Space(12);

        // ─── Bottom Buttons ───
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("🔄 Reset to Default", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Reset to Default?",
                "รีเซ็ตค่าทั้งหมดเป็นค่าเริ่มต้น?", "Yes", "No"))
            {
                ResetToDefault();
            }
        }

        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        if (GUILayout.Button("💾 Save Asset", GUILayout.Height(30)))
        {
            serializedConfig.ApplyModifiedProperties();
            EditorUtility.SetDirty(selectedConfig);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ PlayerSO '{selectedConfig.name}' saved!");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        serializedConfig.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
    }

    // ─────────────────────────────────────────────
    // Drawing Helpers
    // ─────────────────────────────────────────────

    private void DrawHeader()
    {
        var rect = GUILayoutUtility.GetRect(1f, 40f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0.15f, 0.15f, 0.15f, 1f));

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, "🎮 Player Setup Tool", titleStyle);
    }

    private void DrawSectionHeader(string title, Color color, ref bool foldout)
    {
        var rect = GUILayoutUtility.GetRect(1f, 24f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, color);

        GUIStyle style = new GUIStyle(EditorStyles.boldLabel)
        {
            normal = { textColor = Color.white },
            fontSize = 12,
            alignment = TextAnchor.MiddleLeft
        };

        string arrow = foldout ? "▼" : "▶";
        if (GUI.Button(rect, $"  {arrow}  {title}", style))
            foldout = !foldout;

        EditorGUILayout.Space(2);
    }

    private void DrawSectionBackground(System.Action drawContent)
    {
        var bgRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(bgRect, BgDark);
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);
        drawContent();
        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawIntSlider(string label, string propertyName, int min, int max)
    {
        SerializedProperty prop = serializedConfig.FindProperty(propertyName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(140));

        // Visual bar
        float ratio = Mathf.InverseLerp(min, max, prop.intValue);
        Rect barRect = GUILayoutUtility.GetRect(100, 16, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));
        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height);
        EditorGUI.DrawRect(fillRect, new Color(0.3f, 0.7f, 0.4f, 0.8f));

        prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(60));
        prop.intValue = Mathf.Clamp(prop.intValue, min, max);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFloatSlider(string label, string propertyName, float min, float max)
    {
        SerializedProperty prop = serializedConfig.FindProperty(propertyName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(140));

        prop.floatValue = EditorGUILayout.Slider(prop.floatValue, min, max);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawLevelPreviewRow(string label, int baseValue, float multiplier)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(60));

        int v1 = baseValue;
        int v2 = Mathf.RoundToInt(v1 * multiplier);
        int v3 = Mathf.RoundToInt(v2 * multiplier);
        int v4 = Mathf.RoundToInt(v3 * multiplier);

        GUIStyle centered = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.LabelField(v1.ToString(), centered, GUILayout.Width(50));
        EditorGUILayout.LabelField("→", centered, GUILayout.Width(15));
        EditorGUILayout.LabelField(v2.ToString(), centered, GUILayout.Width(50));
        EditorGUILayout.LabelField("→", centered, GUILayout.Width(15));
        EditorGUILayout.LabelField(v3.ToString(), centered, GUILayout.Width(50));
        EditorGUILayout.LabelField("→", centered, GUILayout.Width(15));
        EditorGUILayout.LabelField(v4.ToString(), centered, GUILayout.Width(50));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCenteredMessage(string message)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = 13,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
        };
        EditorGUILayout.LabelField(message, style, GUILayout.Height(60));
    }

    private void CreateNewPlayerConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Player Config", "NewPlayerConfig", "asset",
            "เลือกตำแหน่งบันทึก PlayerSO ใหม่",
            "Assets/ScriptableObject");

        if (string.IsNullOrEmpty(path)) return;

        PlayerSO newConfig = ScriptableObject.CreateInstance<PlayerSO>();
        AssetDatabase.CreateAsset(newConfig, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        selectedConfig = newConfig;
        serializedConfig = new SerializedObject(selectedConfig);
        Debug.Log($"✅ Created new PlayerSO at {path}");
    }

    private void ResetToDefault()
    {
        serializedConfig.FindProperty("initialMaxHealth").intValue = 100;
        serializedConfig.FindProperty("baseDamage").intValue = 10;
        serializedConfig.FindProperty("baseDefence").intValue = 10;
        serializedConfig.FindProperty("walkSpeed").floatValue = 5f;
        serializedConfig.FindProperty("sprintSpeed").floatValue = 8f;
        serializedConfig.FindProperty("jumpForce").floatValue = 8f;
        serializedConfig.FindProperty("gravity").floatValue = -20f;
        serializedConfig.FindProperty("rotationSmoothing").floatValue = 15f;
        serializedConfig.FindProperty("interactSphereRadius").floatValue = 0.8f;
        serializedConfig.FindProperty("interactMaxDistance").floatValue = 2f;
        serializedConfig.FindProperty("healthMultiplier").floatValue = 1.2f;
        serializedConfig.FindProperty("damageMultiplier").floatValue = 1.1f;
        serializedConfig.FindProperty("defenceMultiplier").floatValue = 1.1f;
        serializedConfig.ApplyModifiedProperties();
        Debug.Log("🔄 PlayerSO reset to defaults");
    }
}
#endif
