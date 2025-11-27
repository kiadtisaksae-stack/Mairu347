using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

// ✅ Enum สำหรับ Equipment Slots
public enum EquipmentSlot
{
    Head,
    Body,
    Legs,
    RightHand,
    LeftHand
}
[RequireComponent(typeof(CharacterController))]
public class Player : Character
{
    // UI Input Setters
    public void SetMoveInput(Vector2 input) => _uiMoveInput = input;
    public void SetJumpInput(bool input) => _uiJumpInput = input;
    public void SetSprintInput(bool input) => _uiSprintInput = input;
    public void SetInteractInput(bool input) => _isInteract = input;
    public void SetAttackInput(bool input) => _isAttacking = input;
    // end UI Input Setters
    [Header("Equipment")]
    public List<GameObject> WeaponRigthHand;
    public List<GameObject> WeaponLeftHand;
    public List<GameObject> HeadEquitp;
    public List<GameObject> BodyEquitp;
    public List<GameObject> LegEquitp;
    [Header("Movement Con")]
    private Vector2 _uiMoveInput;
    private bool _uiJumpInput;
    private bool _uiSprintInput;

    
    bool _isAttacking = false;
    bool _isInteract = false;
    [Header("Movement Settings")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    

    private Vector3 velocity;

    private InputSystem_Actions inputActions;
    private CharacterController characterController;
    [Header("Animation Settings")]
    public List<string> attackAnimations;
    public List<GameObject> effect;

    private bool isNetworkReady = false;
    private bool isTeleporting = false;

    public QuestData questDataTest;
    [Header("Inventory")]
    public InventoryCanvas iventory;
    private PlayerData myData;
    public PlayerSO playerSO;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        }

        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        inputActions.Player.Attack.performed += ctx => _isAttacking = true;
        inputActions.Player.Interact.performed += ctx => _isInteract = true;
        inputActions.Player.Interact.canceled += ctx => _isInteract = false;
        inputActions.Player.Sprint.performed += ctx => _uiSprintInput = true;
        inputActions.Player.Sprint.canceled += ctx => _uiSprintInput= false;
        inputActions.Player.Jump.performed += ctx => _uiJumpInput = true;
        inputActions.Player.Jump.canceled += ctx => _uiJumpInput = false;
        inputActions.Player.Q.performed += ctx => TestQuest();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
        inputActions?.Player.Attack.Disable();
    }
    
    public override void OnNetworkSpawn()
    {
        this.Damage = playerSO.Damage;
        this.baseDamage = playerSO.baseDamage;
        this.Defence = playerSO.Defence;
        this.baseDefence = playerSO.baseDefence;
        this.movementSpeed = playerSO.movementSpeed;
        this.sprintSpeed = playerSO.sprint;
        this._initialMaxHealth = playerSO._initialMaxHealth;
        base.OnNetworkSpawn();
        GameManager.Instance.UpdateStatus(Damage, Defence);

        if (IsOwner)
        {
            LoadMyData();
            enabled = true;
            UICanvasControllerInput.RegisterLocalPlayer(this);
            inputActions?.Player.Enable();
            InitializeEquipment();
            // Link InventoryCanvas ของผู้เล่น
            iventory = FindFirstObjectByType<InventoryCanvas>();
            if (iventory != null)
            {
                iventory.playerController = this;
            }

            isNetworkReady = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;
        }
        else
        {
            enabled = false;
        }

        health = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
        if (TeleportManager.Instance != null)
        {
            TeleportManager.Instance.UnregisterPlayer(this);
        }
        if (IsOwner)
        {
            SaveMyData();
        }
        base.OnNetworkDespawn();
        inputActions?.Player.Disable();
        isNetworkReady = false;
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

    private void OnServerStarted()
    {
        isNetworkReady = true;
    }

    void Start()
    {
        Debug.Log($"🚀 Start - IsOwner: {IsOwner}, IsServer: {IsServer}, " +
                  $"CharacterController: {characterController != null}");

        // ✅ ตรวจสอบ components อีกครั้ง
        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // ✅ ตั้งค่า health
        health = maxHealth;

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            isNetworkReady = true;
        }
    }

    public void FixedUpdate()
    {
        if (!IsOwner) return;

        Move();
        Attack(_isAttacking);
        Interact(_isInteract);
        Jump(_uiJumpInput);
    }

    public void Update()
    {

        if (!IsOwner) return;
        ApplyGravity();
        UpdateInFrontCache();
    }
    public void TestQuest()
    {
        QuestManager.Instance.StartQuest(questDataTest);
    }
    #region save load data
    public void SaveMyData()
    {
        if (!IsOwner) return;

        myData = new PlayerData(
            health,
            maxHealth,
            Damage,
            Defence
        );

    }

    // ✅ โหลดข้อมูลของตัวเอง
    private void LoadMyData()
    {
        if (!IsOwner) return;

        // PlayerData saveData = 
        // if (savedData != null)
        // {
        //     ApplyMyData(savedData);
        // }
    }

    // ✅ นำข้อมูลมาใช้
    private void ApplyMyData(PlayerData data)
    {
        // ✅ ตั้งค่าโดยตรงผ่าน Property ที่มีอยู่แล้ว
        health = data.health;
    }

    #endregion
    #region --- interactable Logic ---
    // 🚨 Override method นี้เพื่อใช้พารามิเตอร์ที่เหมาะสมกับ Player
    public override RaycastHit GetClosestInfornt()
    {
        // ใช้พารามิเตอร์ที่ใหญ่ขึ้นสำหรับ Player
        float playerSphereRadius = 0.8f;
        float playerMaxDistance = 2.0f;
        
        Vector3 origin = transform.position + Vector3.up * 0.5f; // ย้าย origin ขึ้นเล็กน้อย
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(origin, playerSphereRadius, direction, playerMaxDistance);
        RaycastHit closestHit = new RaycastHit();
        float minDistance = float.MaxValue;


        bool foundValid = false;
        
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var hitObject = hit.collider.gameObject;

            // ตรวจสอบว่าไม่ใช่ตัวตัวเอง
            if (hitObject == gameObject)
            {
                continue;
            }

            // ตรวจสอบ Identity
            Identity identity = hit.collider.GetComponent<Identity>();
            if (identity == null)
            {
                continue;
            }

            // ตรวจสอบ IInteractable
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
                foundValid = true;
            }
        }

        if (!foundValid)
        {
            Debug.DrawRay(origin, direction * playerMaxDistance, Color.yellow, 0.1f);
        }
        else
        {
        
            Debug.DrawRay(origin, direction * playerMaxDistance, Color.green, 0.1f);
        }

        return closestHit;
    }

    // 🚨 เรียกใช้ cache update ใน Player
    private new void UpdateInFrontCache()
    {
        RaycastHit hit = GetClosestInfornt();
        if (hit.collider != null)
        {
            _cachedIdentityInFront = hit.collider.GetComponent<Identity>();
        }
        else
        {
            _cachedIdentityInFront = null;
        }
    }

    private void Interact(bool interactable)
    {
        if (!IsOwner) return;

        if (interactable)
        {
            if (InFront == null)
            {
                Debug.Log($"[PLAYER INTERACT] No object in front");
                _isInteract = false;
                return;
            }

            IInteractable e = InFront as IInteractable;
            if (e != null && e.isInteractable)
            {
                e.Interact(this);
            }
            else
            {
                Debug.Log($"[PLAYER INTERACT] Object not interactable");
            }

            _isInteract = false;
        }
    }
    #endregion
    #region --- Equipment RPC System ---

    // ✅ Client ใส่ Equipment และแจ้ง Server
    public void EquipHead()
    {
        ItemSO itemToEquip = iventory.headSlot.item;

        // ตรวจสอบว่ามี ItemSO หรือไม่ (headSlot อาจว่างเปล่า)
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            // หากไม่มี Item ให้ปิด Visuals ทั้งหมด
            foreach (var head in HeadEquitp)
            {
                if (head != null)
                {
                    head.SetActive(false);
                }
            }
            
            // ✅ แจ้ง Server ให้ปิด Head
            NotifyEquipmentChangeServerRpc(EquipmentSlot.Head, "");
            return;
        }

        // ✅ เปิดเฉพาะ Head ที่ตรงกับชื่อไอเทม
        string equippedItemName = "";
        foreach (var head in HeadEquitp)
        {
            if (head != null)
            {
                bool shouldActive = head.name.Contains(itemToEquip.itemName);
                head.SetActive(shouldActive);
                
                if (shouldActive)
                {
                    equippedItemName = itemToEquip.itemName;
                }
            }
        }

        // ✅ แจ้ง Server ให้ซิงค์
        NotifyEquipmentChangeServerRpc(EquipmentSlot.Head, equippedItemName);
    }

    public void EquipBody()
    {
        ItemSO itemToEquip = iventory.bodySlot.item;
        
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var body in BodyEquitp)
            {
                if (body != null)
                {
                    body.SetActive(false);
                }
            }
            
            NotifyEquipmentChangeServerRpc(EquipmentSlot.Body, "");
            return;
        }

        string equippedItemName = "";
        foreach (var body in BodyEquitp)
        {
            if (body != null)
            {
                bool shouldActive = body.name.Contains(itemToEquip.itemName);
                body.SetActive(shouldActive);
                
                if (shouldActive)
                {
                    equippedItemName = itemToEquip.itemName;
                }
            }
        }

        NotifyEquipmentChangeServerRpc(EquipmentSlot.Body, equippedItemName);
    }

    public void EquipLeg()
    {
        ItemSO itemToEquip = iventory.legSlot.item;
        
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var leg in LegEquitp)
            {
                if (leg != null)
                {
                    leg.SetActive(false);
                }
            }
            
            NotifyEquipmentChangeServerRpc(EquipmentSlot.Legs, "");
            return;
        }

        string equippedItemName = "";
        foreach (var leg in LegEquitp)
        {
            if (leg != null)
            {
                bool shouldActive = leg.name.Contains(itemToEquip.itemName);
                leg.SetActive(shouldActive);
                
                if (shouldActive)
                {
                    equippedItemName = itemToEquip.itemName;
                }
            }
        }

        NotifyEquipmentChangeServerRpc(EquipmentSlot.Legs, equippedItemName);
    }

    public void EquipWeapon()
    {
        ItemSO itemToEquip = iventory.rightHandSlots.item;
        ItemSO itemToEquipLeft = iventory.leftHandSlots.item;

        // ✅ Right Hand
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var weapon in WeaponRigthHand)
            {
                if (weapon != null)
                {
                    weapon.SetActive(false);
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.RightHand, "");
        }
        else
        {
            string equippedItemName = "";
            foreach (var weapon in WeaponRigthHand)
            {
                if (weapon != null)
                {
                    bool shouldActive = weapon.name.Contains(itemToEquip.itemName);
                    weapon.SetActive(shouldActive);
                    
                    if (shouldActive)
                    {
                        equippedItemName = itemToEquip.itemName;
                    }
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.RightHand, equippedItemName);
        }

        // ✅ Left Hand
        if (itemToEquipLeft == null || itemToEquipLeft.itemName == null)
        {
            foreach (var weapon in WeaponLeftHand)
            {
                if (weapon != null)
                {
                    weapon.SetActive(false);
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.LeftHand, "");
        }
        else
        {
            string equippedItemName = "";
            foreach (var weapon in WeaponLeftHand)
            {
                if (weapon != null)
                {
                    bool shouldActive = weapon.name.Contains(itemToEquipLeft.itemName);
                    weapon.SetActive(shouldActive);
                    
                    if (shouldActive)
                    {
                        equippedItemName = itemToEquipLeft.itemName;
                    }
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.LeftHand, equippedItemName);
        }
    }

    // ✅ ServerRpc เพื่อแจ้งการเปลี่ยนแปลง Equipment
    [ServerRpc]
    private void NotifyEquipmentChangeServerRpc(EquipmentSlot slot, string itemName, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"🔄 Server: Player {OwnerClientId} changing {slot} to {itemName}");
        
        // ✅ ส่งให้ Client คนอื่นเห็น
        SyncEquipmentToClientsClientRpc(OwnerClientId, slot, itemName);
    }

    // ✅ ClientRpc สำหรับซิงค์ Equipment ให้ Client คนอื่น
    [ClientRpc]
    private void SyncEquipmentToClientsClientRpc(ulong targetClientId, EquipmentSlot slot, string itemName)
    {
        // ✅ ข้ามเจ้าของ (เพราะเจ้าของตั้งค่าไปแล้ว)
        if (IsOwner) return;

        Debug.Log($"👀 Other client sees player {targetClientId} equipping {itemName} on {slot}");

        // ✅ ตั้งค่า Equipment ให้ Client คนอื่นเห็น
        switch (slot)
        {
            case EquipmentSlot.Head:
                SetHeadEquipmentForOther(itemName);
                break;
            case EquipmentSlot.Body:
                SetBodyEquipmentForOther(itemName);
                break;
            case EquipmentSlot.Legs:
                SetLegEquipmentForOther(itemName);
                break;
            case EquipmentSlot.RightHand:
                SetRightHandEquipmentForOther(itemName);
                break;
            case EquipmentSlot.LeftHand:
                SetLeftHandEquipmentForOther(itemName);
                break;
        }
    }

    // ✅ Methods สำหรับตั้งค่า Equipment ให้ Client คนอื่นเห็น
    private void SetHeadEquipmentForOther(string itemName)
    {
        foreach (var head in HeadEquitp)
        {
            if (head != null)
            {
                head.SetActive(!string.IsNullOrEmpty(itemName) && head.name.Contains(itemName));
            }
        }
    }

    private void SetBodyEquipmentForOther(string itemName)
    {
        foreach (var body in BodyEquitp)
        {
            if (body != null)
            {
                body.SetActive(!string.IsNullOrEmpty(itemName) && body.name.Contains(itemName));
            }
        }
    }

    private void SetLegEquipmentForOther(string itemName)
    {
        foreach (var leg in LegEquitp)
        {
            if (leg != null)
            {
                leg.SetActive(!string.IsNullOrEmpty(itemName) && leg.name.Contains(itemName));
            }
        }
    }

    private void SetRightHandEquipmentForOther(string itemName)
    {
        foreach (var weapon in WeaponRigthHand)
        {
            if (weapon != null)
            {
                weapon.SetActive(!string.IsNullOrEmpty(itemName) && weapon.name.Contains(itemName));
            }
        }
    }

    private void SetLeftHandEquipmentForOther(string itemName)
    {
        foreach (var weapon in WeaponLeftHand)
        {
            if (weapon != null)
            {
                weapon.SetActive(!string.IsNullOrEmpty(itemName) && weapon.name.Contains(itemName));
            }
        }
    }
    public void InitializeEquipment()
    {
        Debug.Log("🎮 Initializing equipment - disabling all");
        
        DisableAllEquipment(WeaponRigthHand, "Right Hand");
        DisableAllEquipment(WeaponLeftHand, "Left Hand");
        DisableAllEquipment(HeadEquitp, "Head");
        DisableAllEquipment(BodyEquitp, "Body");
        DisableAllEquipment(LegEquitp, "Leg");
        
        Debug.Log("✅ All equipment disabled on game start");
    }

    // ✅ ปิดทั้งหมดใน List
    private void DisableAllEquipment(List<GameObject> equipmentList, string slotName)
    {
        if (equipmentList == null)
        {
            Debug.LogWarning($"⚠️ {slotName} equipment list is null!");
            return;
        }

        int disabledCount = 0;
        foreach (var item in equipmentList)
        {
            if (item != null)
            {
                item.SetActive(false);
                disabledCount++;
            }
        }
        
        Debug.Log($"🔧 {slotName}: Disabled {disabledCount}/{equipmentList.Count} items");
    }

    // ✅ เปิดเฉพาะไอเทมที่ตรงกับชื่อ
    private void EnableMatchingEquipment(List<GameObject> equipmentList, string itemName, string slotName)
    {
        if (equipmentList == null || string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning($"⚠️ Cannot enable equipment - list null or item name empty for {slotName}");
            return;
        }

        int enabledCount = 0;
        foreach (var item in equipmentList)
        {
            if (item != null)
            {
                bool shouldEnable = item.name.Contains(itemName);
                item.SetActive(shouldEnable);
                
                if (shouldEnable)
                {
                    enabledCount++;
                    Debug.Log($"✅ Enabled {item.name} on {slotName}");
                }
            }
        }
        
        if (enabledCount == 0)
        {
            Debug.LogWarning($"⚠️ No matching equipment found for '{itemName}' in {slotName}");
        }
    }

    #endregion
    #region --- Equipment ---
    
    public void UpdateEquipmentStats()
    {
        if (!IsOwner) return;

        int equipmentDamage = 0;
        int equipmentDefence = 0;

        // ✅ คำนวณค่าจาก Equipment ที่สวมใส่
        equipmentDamage += GetSlotDamage(iventory.headSlot);
        equipmentDefence += GetSlotDefence(iventory.headSlot);

        equipmentDamage += GetSlotDamage(iventory.bodySlot);
        equipmentDefence += GetSlotDefence(iventory.bodySlot);

        equipmentDamage += GetSlotDamage(iventory.legSlot);
        equipmentDefence += GetSlotDefence(iventory.legSlot);

        equipmentDamage += GetSlotDamage(iventory.rightHandSlots);
        equipmentDefence += GetSlotDefence(iventory.rightHandSlots);

        equipmentDamage += GetSlotDamage(iventory.leftHandSlots);
        equipmentDefence += GetSlotDefence(iventory.leftHandSlots);

        // ✅ อัพเดตค่าสถานะ
        UpdateStatsServerRpc(baseDamage + equipmentDamage, baseDefence + equipmentDefence);
        GameManager.Instance.UpdateStatus(Damage, Defence);
    }

    private int GetSlotDamage(InventorySlot slot)
    {
        return (slot != null && slot.item != iventory.Empty_Item) ? slot.item.Damage : 0;
    }

    private int GetSlotDefence(InventorySlot slot)
    {
        return (slot != null && slot.item != iventory.Empty_Item) ? slot.item.Deffent : 0;
    }

    [ServerRpc]
    private void UpdateStatsServerRpc(int newDamage, int newDefence)
    {

        UpdateStatusClientRpc(newDamage, newDefence);
    }
    [ClientRpc]
    public void UpdateStatusClientRpc(int newDamage, int newDefence)
    {
        if (!IsOwner) return;
        Damage = newDamage;
        Defence = newDefence;
        GameManager.Instance.UpdateStatus(Damage, Defence);

    }

    #endregion
    #region --- Movement Logic ---
    private void Move()
    {

        Vector2 moveInput = inputActions.Player.Move.ReadValue<Vector2>();
        if (_uiMoveInput != Vector2.zero)
            moveInput = _uiMoveInput; 
        Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        float speed = _uiSprintInput ? sprintSpeed : movementSpeed;
        MoveLocally(inputDir, speed);
    
    }

    private void MoveLocally(Vector3 inputDirection, float currentSpeed)
    {
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, 15f * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, smoothAngle, 0f);

            Vector3 moveDir = transform.forward;
            characterController.Move(moveDir * currentSpeed * Time.deltaTime);

            ReportMovementServerRpc(currentSpeed);
        }
        else
        {
            ReportMovementServerRpc(0f);
        }
    }

    private void ApplyGravity()
    {
        if (!IsOwner) return;
        if (characterController == null)
        {
            return;
        }
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -0.5f;
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }
    public void Jump(bool jump)
    {
        if (!IsOwner) return;
        if (jump && characterController.isGrounded)
        {
            velocity.y = jumpForce;   
        }
    }
    #endregion
    #region --- Attack Logic ---
    public void Attack(bool isAttacking)
    {
        if (isAttacking)
        {
            animator.SetTrigger("Attack");
            RequestPlayAttackAnimServerRpc();

            var e = InFront as Idestoryable;
            if (e is Player)
            {
                _isAttacking = false;
                Debug.Log("Cannot attack self.");
                return;
            }
            else if (e != null)
            {
                Enemy enemy = e as Enemy;
                if (enemy != null)
                {
                    ulong targetId = enemy.NetworkObjectId;
                    DealDamageServerRpc(targetId, Damage);
                }
            }
            _isAttacking = false;
        }
    }
    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        if (IsOwner)
        {
            GameManager.Instance.UpdateHealthBar(health, maxHealth);
        }
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        if (IsOwner)
        {
            GameManager.Instance.UpdateHealthBar(health, maxHealth);
        }
    }
    public override void Die()
    {
        if (!IsServer) return; // ตายต้องเป็น Server เท่านั้น
        if (!isOnLive.Value) return; // ถ้าตายแล้ว ห้ามซ้ำ

        isOnLive.Value = false;
        OnDieClientRpc(); // ให้ Client ทำ animation / ปิด control
        Revive(new Vector3(0, 5, 0));
    }

    [ServerRpc]
    public void RequestPlayAttackAnimServerRpc()
    {
        if (!IsServer) return;
        PlayAttackAnimClientRpc();
    }
    [ClientRpc]
    public void PlayAttackAnimClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }
    #endregion
    #region Teleportation Logic
    public void SimpleTeleport(Vector3 newPosition, Quaternion newRotation)
    {
        if (isTeleporting) return;

        StartCoroutine(TeleportCoroutine(newPosition, newRotation));
    }

    private IEnumerator TeleportCoroutine(Vector3 newPosition, Quaternion newRotation)
    {
        isTeleporting = true;

        if (characterController != null)
            characterController.enabled = false;


        transform.position = newPosition;
        transform.rotation = newRotation;

        // ✅ รอ 1 frame
        yield return null;

        if (characterController != null)
            characterController.enabled = true;

        isTeleporting = false;

        Debug.Log($"📍 Teleported to {newPosition}");
    }
    #endregion
    #region RPC Methods
    [ServerRpc]
    public void DealDamageServerRpc(ulong targetNetworkObjectId, int damage)
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject networkObject))
        {
            if (networkObject.TryGetComponent(out Idestoryable target))
            {
                target.TakeDamage(damage);

                if (target is Enemy enemy)
                {
                    Debug.Log($"[SERVER] {Name} dealt {damage} damage to {enemy.gameObject.name}. Health remaining: {enemy.health}");
                }
            }
        }
    }

    [ServerRpc]
    private void ReportMovementServerRpc(float speed)
    {
        UpdateAnimationClientRpc(speed);
    }

    [ClientRpc]
    private void UpdateAnimationClientRpc(float speed)
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }
    #endregion

    public void OnLevelUp()
    {
        _initialMaxHealth = Mathf.RoundToInt(maxHealth * 1.2f);
        health = maxHealth;
        baseDamage = Mathf.RoundToInt(baseDamage * 1.1f);
        baseDefence = Mathf.RoundToInt(baseDefence * 1.1f);
        Damage = baseDamage;
        Defence = baseDefence;
        GameManager.Instance.UpdateHealthBar(health, maxHealth);
        GameManager.Instance.UpdateStatus(Damage, Defence);
        UpdateEquipmentStats();
    }

}