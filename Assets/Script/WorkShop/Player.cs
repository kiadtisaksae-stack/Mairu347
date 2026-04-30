using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : Character
{
    public void SetMoveInput(Vector2 input) => _uiMoveInput = input;
    public void SetJumpInput(bool input) => _uiJumpInput = input;
    public void SetSprintInput(bool input) => _uiSprintInput = input;
    public void SetInteractInput(bool input) => _isInteract = input;
    public void SetAttackInput(bool input) => _isAttacking = input;

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

    // Movement settings are read from playerSO

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
        inputActions.Player.Sprint.canceled += ctx => _uiSprintInput = false;
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
        this.Damage = playerSO.baseDamage;
        this.baseDamage = playerSO.baseDamage;
        this.Defence = playerSO.baseDefence;
        this.baseDefence = playerSO.baseDefence;
        this.movementSpeed = playerSO.walkSpeed;
        this.sprintSpeed = playerSO.sprintSpeed;
        this._initialMaxHealth = playerSO.initialMaxHealth;
        base.OnNetworkSpawn();
        GameManager.Instance.UpdateStatus(Damage, Defence);

        if (IsOwner)
        {
            LoadMyData();
            enabled = true;
            UICanvasControllerInput.RegisterLocalPlayer(this);
            inputActions?.Player.Enable();
            InitializeEquipment();

            iventory = FindFirstObjectByType<InventoryCanvas>();
            if (iventory != null)
            {
                iventory.playerController = this;
            }

            isNetworkReady = NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening;

            // แก้ปัญหา #12 — ย้ายมาที่ OnNetworkSpawn แทน OnNetworkDespawn
            // เดิมเรียกตอน Despawn → SkillTree ถูก setup หลัง Player หายไปแล้ว ไม่มีประโยชน์
            if (SkillTreeManager.instance != null)
            {
                SkillTreeManager.instance.SetupForLocalPlayer(this.GetComponent<PlayerLevel>());
            }
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
        // ลบ SetupForLocalPlayer ออกจากตรงนี้ — ย้ายไป OnNetworkSpawn แล้ว (#12)
    }

    private void OnServerStarted()
    {
        isNetworkReady = true;
    }

    void Start()
    {
        Debug.Log($"🚀 Start - IsOwner: {IsOwner}, IsServer: {IsServer}, " +
                  $"CharacterController: {characterController != null}");

        if (characterController == null)
            characterController = GetComponent<CharacterController>();
        if (animator == null)
            animator = GetComponent<Animator>();

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
        myData = new PlayerData(health, maxHealth, Damage, Defence);
    }

    private void LoadMyData()
    {
        if (!IsOwner) return;
    }

    private void ApplyMyData(PlayerData data)
    {
        health = data.health;
    }
    #endregion

    #region --- interactable Logic ---
    public override RaycastHit GetClosestInfornt()
    {
        float playerSphereRadius = playerSO.interactSphereRadius;
        float playerMaxDistance = playerSO.interactMaxDistance;
        Vector3 origin = transform.position + Vector3.up * 0.5f;
        Vector3 direction = transform.forward;
        RaycastHit[] hits = Physics.SphereCastAll(origin, playerSphereRadius, direction, playerMaxDistance);
        RaycastHit closestHit = new RaycastHit();
        float minDistance = float.MaxValue;
        bool foundValid = false;

        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            var hitObject = hit.collider.gameObject;
            if (hitObject == gameObject) continue;
            Identity identity = hit.collider.GetComponent<Identity>();
            if (identity == null) continue;
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
                foundValid = true;
            }
        }

        if (!foundValid)
            Debug.DrawRay(origin, direction * playerMaxDistance, Color.yellow, 0.1f);
        else
            Debug.DrawRay(origin, direction * playerMaxDistance, Color.green, 0.1f);

        return closestHit;
    }

    private new void UpdateInFrontCache()
    {
        RaycastHit hit = GetClosestInfornt();
        if (hit.collider != null)
            _cachedIdentityInFront = hit.collider.GetComponent<Identity>();
        else
            _cachedIdentityInFront = null;
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
                e.Interact(this);
            else
                Debug.Log($"[PLAYER INTERACT] Object not interactable");
            _isInteract = false;
        }
    }
    #endregion

    #region --- Equipment RPC System ---
    public void EquipHead()
    {
        ItemSO itemToEquip = iventory.headSlot.item;
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var head in HeadEquitp)
                if (head != null) head.SetActive(false);
            NotifyEquipmentChangeServerRpc(EquipmentSlot.Head, "");
            return;
        }
        string equippedItemName = "";
        foreach (var head in HeadEquitp)
        {
            if (head != null)
            {
                bool shouldActive = head.name.Contains(itemToEquip.itemName);
                head.SetActive(shouldActive);
                if (shouldActive) equippedItemName = itemToEquip.itemName;
            }
        }
        NotifyEquipmentChangeServerRpc(EquipmentSlot.Head, equippedItemName);
    }

    public void EquipBody()
    {
        ItemSO itemToEquip = iventory.bodySlot.item;
        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var body in BodyEquitp)
                if (body != null) body.SetActive(false);
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
                if (shouldActive) equippedItemName = itemToEquip.itemName;
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
                if (leg != null) leg.SetActive(false);
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
                if (shouldActive) equippedItemName = itemToEquip.itemName;
            }
        }
        NotifyEquipmentChangeServerRpc(EquipmentSlot.Legs, equippedItemName);
    }

    public void EquipWeapon()
    {
        ItemSO itemToEquip = iventory.rightHandSlots.item;
        ItemSO itemToEquipLeft = iventory.leftHandSlots.item;

        if (itemToEquip == null || itemToEquip.itemName == null)
        {
            foreach (var weapon in WeaponRigthHand)
                if (weapon != null) weapon.SetActive(false);
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
                    if (shouldActive) equippedItemName = itemToEquip.itemName;
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.RightHand, equippedItemName);
        }

        if (itemToEquipLeft == null || itemToEquipLeft.itemName == null)
        {
            foreach (var weapon in WeaponLeftHand)
                if (weapon != null) weapon.SetActive(false);
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
                    if (shouldActive) equippedItemName = itemToEquipLeft.itemName;
                }
            }
            NotifyEquipmentChangeServerRpc(EquipmentSlot.LeftHand, equippedItemName);
        }
    }

    [ServerRpc]
    private void NotifyEquipmentChangeServerRpc(EquipmentSlot slot, string itemName, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"🔄 Server: Player {OwnerClientId} changing {slot} to {itemName}");
        SyncEquipmentToClientsClientRpc(OwnerClientId, slot, itemName);
    }

    [ClientRpc]
    private void SyncEquipmentToClientsClientRpc(ulong targetClientId, EquipmentSlot slot, string itemName)
    {
        if (IsOwner) return;
        switch (slot)
        {
            case EquipmentSlot.Head: SetHeadEquipmentForOther(itemName); break;
            case EquipmentSlot.Body: SetBodyEquipmentForOther(itemName); break;
            case EquipmentSlot.Legs: SetLegEquipmentForOther(itemName); break;
            case EquipmentSlot.RightHand: SetRightHandEquipmentForOther(itemName); break;
            case EquipmentSlot.LeftHand: SetLeftHandEquipmentForOther(itemName); break;
        }
    }

    private void SetHeadEquipmentForOther(string itemName)
    {
        foreach (var head in HeadEquitp)
            if (head != null) head.SetActive(!string.IsNullOrEmpty(itemName) && head.name.Contains(itemName));
    }

    private void SetBodyEquipmentForOther(string itemName)
    {
        foreach (var body in BodyEquitp)
            if (body != null) body.SetActive(!string.IsNullOrEmpty(itemName) && body.name.Contains(itemName));
    }

    private void SetLegEquipmentForOther(string itemName)
    {
        foreach (var leg in LegEquitp)
            if (leg != null) leg.SetActive(!string.IsNullOrEmpty(itemName) && leg.name.Contains(itemName));
    }

    private void SetRightHandEquipmentForOther(string itemName)
    {
        foreach (var weapon in WeaponRigthHand)
            if (weapon != null) weapon.SetActive(!string.IsNullOrEmpty(itemName) && weapon.name.Contains(itemName));
    }

    private void SetLeftHandEquipmentForOther(string itemName)
    {
        foreach (var weapon in WeaponLeftHand)
            if (weapon != null) weapon.SetActive(!string.IsNullOrEmpty(itemName) && weapon.name.Contains(itemName));
    }

    public void InitializeEquipment()
    {
        DisableAllEquipment(WeaponRigthHand, "Right Hand");
        DisableAllEquipment(WeaponLeftHand, "Left Hand");
        DisableAllEquipment(HeadEquitp, "Head");
        DisableAllEquipment(BodyEquitp, "Body");
        DisableAllEquipment(LegEquitp, "Leg");
    }

    private void DisableAllEquipment(List<GameObject> equipmentList, string slotName)
    {
        if (equipmentList == null) return;
        foreach (var item in equipmentList)
            if (item != null) item.SetActive(false);
    }
    #endregion

    #region --- Equipment Stats ---
    public void UpdateEquipmentStats()
    {
        if (!IsOwner) return;
        int equipmentDamage = 0;
        int equipmentDefence = 0;
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
        UpdateStatsServerRpc(baseDamage + equipmentDamage, baseDefence + equipmentDefence);
        GameManager.Instance.UpdateStatus(Damage, Defence);
    }

    private int GetSlotDamage(InventorySlot slot) =>
        (slot != null && slot.item != iventory.Empty_Item) ? slot.item.Damage : 0;

    private int GetSlotDefence(InventorySlot slot) =>
        (slot != null && slot.item != iventory.Empty_Item) ? slot.item.Deffent : 0;

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
        float speed = _uiSprintInput ? playerSO.sprintSpeed : playerSO.walkSpeed;
        MoveLocally(inputDir, speed);
    }

    private void MoveLocally(Vector3 inputDirection, float currentSpeed)
    {
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
            float smoothAngle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, playerSO.rotationSmoothing * Time.deltaTime);
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
        if (characterController == null) return;
        bool isGrounded = characterController.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -0.5f;
        velocity.y += playerSO.gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);
    }

    public void Jump(bool jump)
    {
        if (!IsOwner) return;
        if (jump && characterController.isGrounded)
            velocity.y = playerSO.jumpForce;
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
            GameManager.Instance.UpdateHealthBar(health, maxHealth);
    }

    public override void Heal(int amount)
    {
        base.Heal(amount);
        if (IsOwner)
            GameManager.Instance.UpdateHealthBar(health, maxHealth);
    }

    public override void Die()
    {
        if (!IsServer) return;
        if (!isOnLive.Value) return;
        isOnLive.Value = false;
        OnDieClientRpc();
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
            animator.SetTrigger("Attack");
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
        if (characterController != null) characterController.enabled = false;
        transform.position = newPosition;
        transform.rotation = newRotation;
        yield return null;
        if (characterController != null) characterController.enabled = true;
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
                    Debug.Log($"[SERVER] {Name} dealt {damage} damage to {enemy.gameObject.name}. Health remaining: {enemy.health}");
            }
        }
    }

    // ✅ ใช้จาก InventorySlot เมื่อ Client กิน consumable
    [ServerRpc]
    public void HealServerRpc(int amount, ServerRpcParams rpcParams = default)
    {
        Heal(amount);
        Debug.Log($"[SERVER] {Name} healed for {amount} HP. Current HP: {health}");
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
            animator.SetFloat("Speed", speed);
    }
    #endregion

    public void OnLevelUp()
    {
        _initialMaxHealth = Mathf.RoundToInt(maxHealth * playerSO.healthMultiplier);
        health = maxHealth;
        baseDamage = Mathf.RoundToInt(baseDamage * playerSO.damageMultiplier);
        baseDefence = Mathf.RoundToInt(baseDefence * playerSO.defenceMultiplier);
        Damage = baseDamage;
        Defence = baseDefence;
        GameManager.Instance.UpdateHealthBar(health, maxHealth);
        GameManager.Instance.UpdateStatus(Damage, Defence);
        UpdateEquipmentStats();
    }
}