#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class EnemyCreatorWindow : EditorWindow
{
    // ─── State ───
    private List<EnemyType> allEnemies = new List<EnemyType>();
    private EnemyType selectedEnemy;
    private SerializedObject serializedEnemy;
    private Vector2 leftScrollPos;
    private Vector2 rightScrollPos;
    private string searchFilter = "";
    private string newEnemyName = "";

    // ─── Section Foldouts ───
    private bool showIdentity = true;
    private bool showStats = true;
    private bool showAI = true;
    private bool showXP = true;
    private bool showDropTable = true;

    // ─── Colors ───
    private static readonly Color HeaderColor = new Color(0.75f, 0.2f, 0.2f, 1f);
    private static readonly Color IdentityColor = new Color(0.3f, 0.65f, 0.85f, 1f);
    private static readonly Color StatsColor = new Color(0.85f, 0.3f, 0.3f, 1f);
    private static readonly Color AIColor = new Color(0.4f, 0.75f, 0.35f, 1f);
    private static readonly Color XPColor = new Color(0.9f, 0.75f, 0.2f, 1f);
    private static readonly Color DropColor = new Color(0.7f, 0.4f, 0.85f, 1f);
    private static readonly Color ListSelected = new Color(0.25f, 0.5f, 0.85f, 0.6f);
    private static readonly Color ListHover = new Color(0.3f, 0.3f, 0.3f, 0.4f);
    private static readonly Color PanelBg = new Color(0.17f, 0.17f, 0.17f, 1f);
    private static readonly Color DangerColor = new Color(0.8f, 0.2f, 0.2f, 1f);

    private const string SAVE_PATH = "Assets/ScriptableObject/EnemySO";

    [MenuItem("Window/Game Tools/Enemy Creator")]
    public static void ShowWindow()
    {
        var window = GetWindow<EnemyCreatorWindow>("👹 Enemy Creator");
        window.minSize = new Vector2(700, 500);
    }

    private void OnEnable()
    {
        RefreshEnemyList();
    }

    private void OnFocus()
    {
        RefreshEnemyList();
    }

    private void RefreshEnemyList()
    {
        allEnemies.Clear();
        string[] guids = AssetDatabase.FindAssets("t:EnemyType");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyType enemy = AssetDatabase.LoadAssetAtPath<EnemyType>(path);
            if (enemy != null)
                allEnemies.Add(enemy);
        }
        allEnemies.Sort((a, b) => string.Compare(a.enemyName, b.enemyName));
    }

    private void OnGUI()
    {
        DrawMainHeader();

        EditorGUILayout.BeginHorizontal();

        // ─── Left Panel ───
        DrawLeftPanel();

        // ─── Separator ───
        var sepRect = GUILayoutUtility.GetRect(2, 1, GUILayout.ExpandHeight(true), GUILayout.Width(2));
        EditorGUI.DrawRect(sepRect, new Color(0.3f, 0.3f, 0.3f));

        // ─── Right Panel ───
        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    // ═══════════════════════════════════════════════
    // LEFT PANEL — Enemy List
    // ═══════════════════════════════════════════════

    private void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(220), GUILayout.ExpandHeight(true));

        var panelRect = GUILayoutUtility.GetRect(220, position.height - 50);
        EditorGUI.DrawRect(panelRect, PanelBg);

        GUILayout.BeginArea(new Rect(panelRect.x, panelRect.y, panelRect.width, panelRect.height));

        // Search bar
        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        searchFilter = EditorGUILayout.TextField(searchFilter, EditorStyles.toolbarSearchField);
        GUILayout.Space(4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);

        // Enemy list
        leftScrollPos = EditorGUILayout.BeginScrollView(leftScrollPos);

        foreach (var enemy in allEnemies)
        {
            if (enemy == null) continue;
            string displayName = string.IsNullOrEmpty(enemy.enemyName) ? enemy.name : enemy.enemyName;

            if (!string.IsNullOrEmpty(searchFilter) &&
                !displayName.ToLower().Contains(searchFilter.ToLower()))
                continue;

            // Difficulty color indicator
            string diffIcon = GetDifficultyIcon(enemy);
            bool isSelected = (selectedEnemy == enemy);

            Rect itemRect = GUILayoutUtility.GetRect(1, 28, GUILayout.ExpandWidth(true));
            if (isSelected)
                EditorGUI.DrawRect(itemRect, ListSelected);

            // Hover effect
            if (itemRect.Contains(Event.current.mousePosition) && !isSelected)
            {
                EditorGUI.DrawRect(itemRect, ListHover);
                Repaint();
            }

            GUIStyle itemStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                normal = { textColor = isSelected ? Color.white : new Color(0.8f, 0.8f, 0.8f) },
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(8, 4, 0, 0)
            };

            if (GUI.Button(itemRect, $"{diffIcon} {displayName}", itemStyle))
            {
                selectedEnemy = enemy;
                serializedEnemy = new SerializedObject(selectedEnemy);
                GUI.FocusControl(null);
            }
        }

        EditorGUILayout.EndScrollView();

        EditorGUILayout.Space(4);

        // ─── Bottom buttons ───
        GUILayout.FlexibleSpace();

        // New enemy input
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        newEnemyName = EditorGUILayout.TextField(newEnemyName, GUILayout.Height(22));
        GUILayout.Space(4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);
        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        if (GUILayout.Button("＋ New Enemy", GUILayout.Height(26)))
        {
            CreateNewEnemy();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(4);

        GUI.enabled = selectedEnemy != null;
        if (GUILayout.Button("📋 Duplicate", GUILayout.Height(24)))
        {
            DuplicateEnemy();
        }

        GUI.backgroundColor = DangerColor;
        if (GUILayout.Button("🗑 Delete", GUILayout.Height(24)))
        {
            DeleteEnemy();
        }
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;

        GUILayout.Space(4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(6);

        GUILayout.EndArea();
        EditorGUILayout.EndVertical();
    }

    // ═══════════════════════════════════════════════
    // RIGHT PANEL — Enemy Config Editor
    // ═══════════════════════════════════════════════

    private void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        if (selectedEnemy == null || serializedEnemy == null)
        {
            GUILayout.FlexibleSpace();
            DrawCenteredMessage("👈 เลือก Enemy จากรายการด้านซ้าย\nหรือสร้างใหม่ด้วยปุ่ม + New Enemy");
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            return;
        }

        serializedEnemy.Update();

        rightScrollPos = EditorGUILayout.BeginScrollView(rightScrollPos);

        EditorGUILayout.Space(8);

        // ─── Identity Section ───
        DrawSectionHeader("🏷️ Identity", IdentityColor, ref showIdentity);
        if (showIdentity)
        {
            DrawSectionBg(() =>
            {
                EditorGUILayout.PropertyField(serializedEnemy.FindProperty("enemyName"), new GUIContent("Name"));
                EditorGUILayout.PropertyField(serializedEnemy.FindProperty("enemyId"), new GUIContent("ID"));
                EditorGUILayout.PropertyField(serializedEnemy.FindProperty("enemyPrefab"), new GUIContent("Prefab"));

                // Prefab preview
                SerializedProperty prefabProp = serializedEnemy.FindProperty("enemyPrefab");
                if (prefabProp.objectReferenceValue != null)
                {
                    Texture2D preview = AssetPreview.GetAssetPreview(prefabProp.objectReferenceValue);
                    if (preview != null)
                    {
                        EditorGUILayout.Space(4);
                        Rect previewRect = GUILayoutUtility.GetRect(80, 80, GUILayout.ExpandWidth(false));
                        previewRect.x = (EditorGUIUtility.currentViewWidth - 220) / 2 - 40 + 220;
                        GUI.DrawTexture(previewRect, preview, ScaleMode.ScaleToFit);
                    }
                }
            });
        }

        EditorGUILayout.Space(4);

        // ─── Stats Section ───
        DrawSectionHeader("❤️ Stats", StatsColor, ref showStats);
        if (showStats)
        {
            DrawSectionBg(() =>
            {
                DrawIntBar("Max HP", "initialMaxHealth", 1, 9999, StatsColor);
                DrawIntBar("Damage", "damage", 0, 999, new Color(0.9f, 0.5f, 0.2f));
                DrawIntBar("Defence", "defence", 0, 999, new Color(0.3f, 0.6f, 0.9f));
                DrawFloatField("Move Speed", "movementSpeed", 0f, 20f);
            });
        }

        EditorGUILayout.Space(4);

        // ─── AI Section ───
        DrawSectionHeader("🧠 AI Behavior", AIColor, ref showAI);
        if (showAI)
        {
            DrawSectionBg(() =>
            {
                EditorGUILayout.PropertyField(serializedEnemy.FindProperty("aiType"), new GUIContent("AI Type"));

                EnemyAIType currentAI = (EnemyAIType)serializedEnemy.FindProperty("aiType").enumValueIndex;

                EditorGUILayout.Space(4);

                DrawFloatField("Search Radius", "searchRadius", 0f, 50f);
                DrawFloatField("Attack Range", "attackRange", 0f, 30f);
                DrawFloatField("Attack Cooldown", "attackCooldown", 0.1f, 10f);

                // AI Type description
                EditorGUILayout.Space(4);
                string aiDesc = currentAI switch
                {
                    EnemyAIType.Melee => "🗡️ ไล่ตาม + โจมตีระยะประชิด",
                    EnemyAIType.Ranged => "🏹 ยืนห่าง + โจมตีระยะไกล",
                    EnemyAIType.Boss => "👑 Boss — มี pattern โจมตีพิเศษ",
                    EnemyAIType.Passive => "🕊️ ไม่โจมตีจนกว่าจะถูกโจมตี",
                    _ => ""
                };
                EditorGUILayout.HelpBox(aiDesc, MessageType.Info);
            });
        }

        EditorGUILayout.Space(4);

        // ─── XP Section ───
        DrawSectionHeader("⭐ Experience", XPColor, ref showXP);
        if (showXP)
        {
            DrawSectionBg(() =>
            {
                DrawIntBar("XP Value", "experience", 0, 9999, XPColor);
                DrawFloatField("Share Radius", "xpShareRadius", 0f, 50f);
            });
        }

        EditorGUILayout.Space(4);

        // ─── Drop Table Section ───
        DrawSectionHeader("🎁 Drop Table", DropColor, ref showDropTable);
        if (showDropTable)
        {
            DrawSectionBg(() =>
            {
                SerializedProperty dropTable = serializedEnemy.FindProperty("dropTable");
                SerializedProperty dropCount = serializedEnemy.FindProperty("dropCount");

                EditorGUILayout.PropertyField(dropCount, new GUIContent("Drop Count"));
                EditorGUILayout.Space(4);

                // Calculate total weight for visualization
                int totalWeight = 0;
                for (int i = 0; i < dropTable.arraySize; i++)
                {
                    totalWeight += dropTable.GetArrayElementAtIndex(i).FindPropertyRelative("weight").intValue;
                }

                for (int i = 0; i < dropTable.arraySize; i++)
                {
                    SerializedProperty entry = dropTable.GetArrayElementAtIndex(i);
                    SerializedProperty prefab = entry.FindPropertyRelative("prefab");
                    SerializedProperty weight = entry.FindPropertyRelative("weight");

                    EditorGUILayout.BeginHorizontal();

                    // Prefab field
                    EditorGUILayout.PropertyField(prefab, GUIContent.none, GUILayout.Width(150));

                    // Weight bar
                    float ratio = totalWeight > 0 ? (float)weight.intValue / totalWeight : 0f;
                    Rect barRect = GUILayoutUtility.GetRect(80, 18, GUILayout.ExpandWidth(true));
                    EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));
                    Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height);

                    // Color based on ratio
                    Color barColor = Color.Lerp(new Color(0.3f, 0.5f, 0.8f), new Color(0.9f, 0.7f, 0.2f), ratio);
                    EditorGUI.DrawRect(fillRect, barColor);

                    // Percentage label
                    string pctText = totalWeight > 0 ? $"{(ratio * 100f):F0}%" : "0%";
                    GUI.Label(barRect, $"  wt:{weight.intValue}  ({pctText})", new GUIStyle(EditorStyles.miniLabel)
                    {
                        normal = { textColor = Color.white },
                        alignment = TextAnchor.MiddleLeft
                    });

                    // Weight field
                    weight.intValue = EditorGUILayout.IntField(weight.intValue, GUILayout.Width(40));
                    weight.intValue = Mathf.Clamp(weight.intValue, 0, 100);

                    // Delete button
                    GUI.backgroundColor = DangerColor;
                    if (GUILayout.Button("✕", GUILayout.Width(22)))
                    {
                        dropTable.DeleteArrayElementAtIndex(i);
                        break;
                    }
                    GUI.backgroundColor = Color.white;

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space(2);
                }

                EditorGUILayout.Space(4);
                if (GUILayout.Button("＋ Add Drop Entry", GUILayout.Height(24)))
                {
                    dropTable.InsertArrayElementAtIndex(dropTable.arraySize);
                }
            });
        }

        EditorGUILayout.Space(12);

        // ─── Bottom Save Button ───
        EditorGUILayout.BeginHorizontal();

        GUI.backgroundColor = new Color(0.2f, 0.7f, 0.3f, 1f);
        if (GUILayout.Button("💾 Save", GUILayout.Height(30)))
        {
            serializedEnemy.ApplyModifiedProperties();
            EditorUtility.SetDirty(selectedEnemy);
            AssetDatabase.SaveAssets();
            Debug.Log($"✅ EnemyType '{selectedEnemy.enemyName}' saved!");
        }
        GUI.backgroundColor = Color.white;

        if (GUILayout.Button("🔄 Revert", GUILayout.Height(30), GUILayout.Width(100)))
        {
            serializedEnemy = new SerializedObject(selectedEnemy);
            Debug.Log("🔄 Reverted to last saved state");
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        serializedEnemy.ApplyModifiedProperties();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    // ═══════════════════════════════════════════════
    // CRUD Operations
    // ═══════════════════════════════════════════════

    private void CreateNewEnemy()
    {
        string enemyName = string.IsNullOrEmpty(newEnemyName) ? "NewEnemy" : newEnemyName;

        // Ensure directory exists
        if (!AssetDatabase.IsValidFolder(SAVE_PATH))
        {
            string[] parts = SAVE_PATH.Split('/');
            string currentPath = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string nextPath = currentPath + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, parts[i]);
                currentPath = nextPath;
            }
        }

        string assetPath = AssetDatabase.GenerateUniqueAssetPath($"{SAVE_PATH}/{enemyName}.asset");

        EnemyType newEnemy = ScriptableObject.CreateInstance<EnemyType>();
        newEnemy.enemyName = enemyName;
        newEnemy.enemyId = allEnemies.Count + 1;

        AssetDatabase.CreateAsset(newEnemy, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        RefreshEnemyList();
        selectedEnemy = newEnemy;
        serializedEnemy = new SerializedObject(selectedEnemy);
        newEnemyName = "";

        Debug.Log($"✅ Created new EnemyType '{enemyName}' at {assetPath}");
    }

    private void DuplicateEnemy()
    {
        if (selectedEnemy == null) return;

        string sourcePath = AssetDatabase.GetAssetPath(selectedEnemy);
        string newName = selectedEnemy.enemyName + "_Copy";
        string newPath = AssetDatabase.GenerateUniqueAssetPath($"{SAVE_PATH}/{newName}.asset");

        AssetDatabase.CopyAsset(sourcePath, newPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EnemyType duplicate = AssetDatabase.LoadAssetAtPath<EnemyType>(newPath);
        if (duplicate != null)
        {
            duplicate.enemyName = newName;
            EditorUtility.SetDirty(duplicate);
            AssetDatabase.SaveAssets();
        }

        RefreshEnemyList();
        selectedEnemy = duplicate;
        serializedEnemy = new SerializedObject(selectedEnemy);

        Debug.Log($"📋 Duplicated '{selectedEnemy.enemyName}' → '{newName}'");
    }

    private void DeleteEnemy()
    {
        if (selectedEnemy == null) return;

        string enemyName = selectedEnemy.enemyName;
        if (!EditorUtility.DisplayDialog("Delete Enemy?",
            $"ลบ '{enemyName}' ถาวร?\nไม่สามารถย้อนกลับได้!", "Delete", "Cancel"))
            return;

        string path = AssetDatabase.GetAssetPath(selectedEnemy);
        AssetDatabase.DeleteAsset(path);
        AssetDatabase.Refresh();

        selectedEnemy = null;
        serializedEnemy = null;
        RefreshEnemyList();

        Debug.Log($"🗑 Deleted EnemyType '{enemyName}'");
    }

    // ═══════════════════════════════════════════════
    // Drawing Helpers
    // ═══════════════════════════════════════════════

    private void DrawMainHeader()
    {
        var rect = GUILayoutUtility.GetRect(1f, 40f, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(rect, new Color(0.12f, 0.12f, 0.12f, 1f));

        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };
        GUI.Label(rect, "👹 Enemy Creator Tool", titleStyle);

        // Enemy count badge
        GUIStyle countStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            alignment = TextAnchor.MiddleRight,
            normal = { textColor = new Color(0.6f, 0.6f, 0.6f) },
            padding = new RectOffset(0, 12, 0, 0)
        };
        GUI.Label(rect, $"Total: {allEnemies.Count}", countStyle);
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

    private void DrawSectionBg(System.Action drawContent)
    {
        var bgRect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(bgRect, new Color(0.19f, 0.19f, 0.19f, 1f));
        EditorGUI.indentLevel++;
        EditorGUILayout.Space(4);
        drawContent();
        EditorGUILayout.Space(4);
        EditorGUI.indentLevel--;
        EditorGUILayout.EndVertical();
    }

    private void DrawIntBar(string label, string propertyName, int min, int max, Color barColor)
    {
        SerializedProperty prop = serializedEnemy.FindProperty(propertyName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));

        float ratio = Mathf.InverseLerp(min, max, prop.intValue);
        Rect barRect = GUILayoutUtility.GetRect(80, 16, GUILayout.ExpandWidth(true));
        EditorGUI.DrawRect(barRect, new Color(0.15f, 0.15f, 0.15f));
        Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * ratio, barRect.height);
        EditorGUI.DrawRect(fillRect, new Color(barColor.r, barColor.g, barColor.b, 0.7f));

        prop.intValue = EditorGUILayout.IntField(prop.intValue, GUILayout.Width(60));
        prop.intValue = Mathf.Clamp(prop.intValue, min, max);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawFloatField(string label, string propertyName, float min, float max)
    {
        SerializedProperty prop = serializedEnemy.FindProperty(propertyName);
        if (prop == null) return;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(100));
        prop.floatValue = EditorGUILayout.Slider(prop.floatValue, min, max);
        EditorGUILayout.EndHorizontal();
    }

    private string GetDifficultyIcon(EnemyType enemy)
    {
        int totalPower = enemy.initialMaxHealth + enemy.damage * 5 + enemy.defence * 3;
        if (totalPower > 1000) return "🔴";
        if (totalPower > 400) return "🟡";
        return "🟢";
    }

    private void DrawCenteredMessage(string message)
    {
        GUIStyle style = new GUIStyle(EditorStyles.label)
        {
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true,
            fontSize = 13,
            normal = { textColor = new Color(0.5f, 0.5f, 0.5f) }
        };
        EditorGUILayout.LabelField(message, style, GUILayout.Height(80));
    }
}
#endif
