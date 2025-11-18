using System.Numerics;

[System.Serializable]
public class PlayerData
{
<<<<<<< Updated upstream
    public static PlayerData Instance;

    public string playerName = "Default";
    public int maxHealth = 100;
    public int damage = 10;
    public float speed = 5f;

    private void OnEnable() => Instance = this;
}
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;

//public class Player : BaseCharacter
//{
//    Vector3 _inputDirection;
//    public int value;
//    bool _isInteract = false;
//    [Header("CAMERA")]
//    public Transform mCameraT;
//    public float mouseSensitivity = 2f;
//    [Space]
//    public float minLookX = -90f;
//    public float maxLookX = 60f;
//    private float currentLookX = 0f;

//    [Header("Equipment")]
//    public Transform rightHandTransform;
//    [Header("Inventory")]
//    public List<Item> inventory = new List<Item>();
//    public Image[] inventoryIcons;
//    public GameObject inventoryCanvas;

//    public GameObject exitUi;

//    private void Start()
//    {
//        rb = GetComponent<Rigidbody>();
//        health = maxHealth;
//        Cursor.lockState = CursorLockMode.Locked;
//        Cursor.visible = false;
//    }



//    void Update()
//    {
//        HandleInput();
//        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
//        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

//        // ไม่ต้องคูณ Time.deltaTime ใน Update เพราะ MouseLook จะจัดการการหมุนทันที
//        if (mouseX != 0 || mouseY != 0)
//        {
//            MouseLook(mouseX, mouseY);
//        }
//    }

//    private void FixedUpdate()
//    {
//        Vector3 movementDirection = transform.TransformDirection(_inputDirection);
//        Move(movementDirection);
//        Interact(_isInteract);
//    }

//    private void HandleInput()
//    {
//        float x = Input.GetAxis("Horizontal");
//        float y = Input.GetAxis("Vertical");
//        _inputDirection = new Vector3(x, 0, y);
//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            _isInteract = true;
//        }
//        if (Input.GetKeyDown(KeyCode.Tab))
//        {
//            inventoryCanvas.SetActive(!inventoryCanvas.activeSelf);
//        }
//        if (Input.GetKeyDown(KeyCode.Escape))
//        {
//            Cursor.lockState = CursorLockMode.None;
//            Cursor.visible = true;
//            exitUi.SetActive(true);
//        }
//    }
//    private void MouseLook(float mouseX, float mouseY)
//    {
//        // การหมุนตัวผู้เล่นในแกน Y (ซ้าย-ขวา)
//        // ใช้ transform.Rotate เพื่อให้การหมุนเป็นไปทันที
//        transform.Rotate(0, mouseX, 0);

//        // การหมุนกล้องในแกน X (ขึ้น-ลง)
//        currentLookX -= mouseY;
//        currentLookX = Mathf.Clamp(currentLookX, minLookX, maxLookX);

//        mCameraT.localRotation = Quaternion.Euler(currentLookX, 0f, 0f);
//    }


//    public void Interact(bool interactable)
//    {
//        if (interactable)
//        {
//            IInteractable e = InFront as IInteractable;
//            if (e != null)
//            {
//                e.Interract(this);
//            }
//            _isInteract = false;
//        }
//    }
//    public void AddItem(Item item)
//    {
//        inventory.Add(item);
//        for (int i = 0; i < inventoryIcons.Length; i++)
//        {
//            if (inventoryIcons[i].sprite == null)
//            {
//                inventoryIcons[i].sprite = item.iconSprite;
//                return;
//            }
//        }
//    }
//}
=======
    public int health;
    public int maxHealth;
    public int damage;
    public int defence;
 
    public PlayerData(int health, int maxHealth, int damage, int defence)
    {
        this.health = health;
        this.maxHealth = maxHealth;
        this.damage = damage;
        this.defence = defence;
    }
}
>>>>>>> Stashed changes
