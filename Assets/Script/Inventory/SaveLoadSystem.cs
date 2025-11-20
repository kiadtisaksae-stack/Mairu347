//using System.IO;
//using UnityEngine;

//public class SaveLoadSystem : MonoBehaviour
//{
//    [Header("Inventory Save")]
//    public string fileName = "InventorySave.json";
//    public InventoryCanvas inventoryCanvas;
//    private void Start()
//    {
//        inventoryCanvas = FindObjectOfType<InventoryCanvas>();
//        // ตรวจสอบ inventoryCanvas ก่อนโหลด
//        if (inventoryCanvas == null)
//        {
//            Debug.LogError("Inventory Canvas reference is missing!");
//            return;
//        }

//    }
    
//    private string GetSavePath()
//    {
//        return Path.Combine(Application.persistentDataPath, fileName);
//    }

//    public void SaveInventoryData()
//    {
//        string jsonData = inventoryCanvas.SaveData();
//        File.WriteAllText(GetSavePath(), jsonData);
//        Debug.Log("Inventory saved to: " + GetSavePath());
//    }

//    public void LoadInventoryData()
//    {
//        string path = GetSavePath();
//        if (File.Exists(path))
//        {
//            string jsonData = File.ReadAllText(path);
//            inventoryCanvas.LoadData(jsonData);
//            Debug.Log("Inventory loaded from: " + path);
//        }
//        else
//        {
//            Debug.LogWarning("No save file found at: " + path);
//        }
//    }

//    // สำหรับทดสอบใน Editor
//    private void OnGUI()
//    {
//        // กำหนดสไตล์ของปุ่ม
//        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
//        buttonStyle.fontSize = 16;
//        buttonStyle.fixedWidth = 70;
//        buttonStyle.fixedHeight = 25;
        
//        // กำหนดพื้นที่ด้านขวาของหน้าจอ
//        float buttonWidth = 80;
//        float buttonHeight = 20;
//        float margin = 5;
//        float xPos = Screen.width - buttonWidth - margin;
//        float yPos = margin;
        
//        // ปุ่ม Save
//        if (GUI.Button(new Rect(xPos, yPos, buttonWidth, buttonHeight), "Save", buttonStyle))
//        {
//            SaveInventoryData();
//        }
        
//        // ปุ่ม Load (วางใต้ปุ่ม Save)
//        yPos += buttonHeight + 10;
//        if (GUI.Button(new Rect(xPos, yPos, buttonWidth, buttonHeight), "Load", buttonStyle))
//        {
//            LoadInventoryData();
//        }
//    }

//    // ตัวอย่างการเซฟ/โหลดอัตโนมัติ
//    private void OnApplicationQuit()
//    {
//        SaveInventoryData();
//    }

//    // private void Start()
//    // {
//    //     LoadInventoryData();
//    // }
//}
