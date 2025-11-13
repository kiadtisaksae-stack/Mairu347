using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class Player : Character
{
    [Header("Movement Con")]
    private Vector2 _uiMoveInput;
    private bool _uiJumpInput;
    private bool _uiSprintInput;
    [Header("Network Movement Settings")]
    [SerializeField] private float movementSyncInterval = 0.1f; // Sync 10 ครั้ง/วินาที
    [SerializeField] private float positionCorrectionStrength = 5f;
    
    private float _lastMovementSyncTime = 0f;
    private Vector3 _serverPosition;
    private bool _needsPositionCorrection = false;

    // UI Input Setters
    public void SetMoveInput(Vector2 input) => _uiMoveInput = input;
    public void SetJumpInput(bool input) => _uiJumpInput = input;
    public void SetSprintInput(bool input) => _uiSprintInput = input;
    public void SetInteractInput(bool input) => _isInteract = input;
    public void SetAttackInput(bool input) => _isAttacking = input;
    // end UI Input Setters

    [Header("Hand setting")]
    public Transform RightHand;
    public Transform LeftHand;
    [Header("inventory")]
    public List<ItemData> inventory = new List<ItemData>();
    
    [Header("Weapon & Equitpment")]
    public List<GameObject> WeaponVisuals = new List<GameObject>();
    public List<GameObject> equitpMentvis = new List<GameObject>();
    bool _isAttacking = false;
    bool _isInteract = false;
    [Header("Movement Settings")]
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float rotationSmoothTime = 0.2f;

    private Vector3 velocity;

    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float comboResetTime = 1f;

    private InputSystem_Actions inputActions;
    private CharacterController characterController;
    [Header("Animation Settings")]
    public List<string> attackAnimations;
    public List<GameObject> effect;

    private bool isNetworkReady = false;

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
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
        inputActions?.Player.Attack.Disable();
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner)
        {
            enabled = true;
            UICanvasControllerInput.RegisterLocalPlayer(this);
            inputActions?.Player.Enable();

            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
            {
                isNetworkReady = true;
            }

            Debug.Log($"✅ Owner player enabled - Client ID: {OwnerClientId}");

            // ตั้งค่าเริ่มต้น
            _serverPosition = transform.position;
        }
        else
        {
            enabled = false;
            Debug.Log($"👀 Other player disabled - Client ID: {OwnerClientId}");
        }

        health = maxHealth;
    }

    public override void OnNetworkDespawn()
    {
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
        
        // 🚨 เรียก UpdateInFrontCache ใน Update ของ Player
        UpdateInFrontCache();
    }
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
    #region --- Inventory Logic ---
    public void AddItem(Item item)
    {
    
        ItemData newItemData = new ItemData(item);
        inventory.Add(newItemData);
        if (IsOwner) 
        {
            if (InventoryUI.Instance != null)
            {
                
                InventoryUI.Instance.UpdateUIOnItemCollect(newItemData, item.GetEquipment());
            }
        }
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
            Debug.LogError("❌ CharacterController is null in ApplyGravity!");
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
        else
        {
            
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
    public bool IsItemEquipped(string itemName)
    {
        foreach (GameObject weapon in WeaponVisuals)
        {
            if (weapon != null && weapon.activeSelf)
            {
                // 💡 แก้ไข: ใช้เฉพาะ .name.Contains() หรือเปรียบเทียบชื่อตรงๆ
                if (weapon.name.Contains(itemName))
                {
                    return true;
                }
            }
        }

        // ตรวจสอบใน equitpMentvis (ถือเป็น LeftHand หรือ Equipment)
        foreach (GameObject equipment in equitpMentvis)
        {
            if (equipment != null && equipment.activeSelf)
            {
                // 💡 แก้ไข: ใช้เฉพาะ .name.Contains()
                if (equipment.name.Contains(itemName))
                {
                    return true;
                }
            }
        }

        return false;
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
    [ServerRpc]
    private void ServerRequestAnimationTriggerServerRpc(FixedString32Bytes triggerName)
    {
        // ตรวจสอบ Server
        if (!IsServer) return;

        // สั่งให้ Client เล่น Trigger
        PlayAnimationTriggerClientRpc(triggerName);
    }
    [ClientRpc]
    private void PlayAnimationTriggerClientRpc(FixedString32Bytes triggerName)
    {
        if (animator != null)
        {
            animator.SetTrigger(triggerName.ToString());
        }
    }
    [ClientRpc]
    public void EquipItemVisualClientRpc(FixedString32Bytes itemName)
    {
        // 1. Logic การจัดการ Visuals
        HandleWeaponVisuals(itemName.ToString());
        
        // 2. Logic การแสดงผลดาเมจ Text (ถ้ามี)
    }

    // จัดการการสลับ Visuals 
    private void HandleWeaponVisuals(string targetItemName)
    {
        if (RightHand == null) return;

        bool foundAndEquipped = false;

        foreach (GameObject weapon in WeaponVisuals)
        {
            if (weapon == null) continue;
           

            bool isTargetWeapon = weapon.name.Contains(targetItemName) || weapon.CompareTag(targetItemName);

            if (isTargetWeapon)
            {
                weapon.SetActive(true);
                foundAndEquipped = true;
                Debug.Log($"Equipped: {weapon.name}");
            }
            else
            {
                weapon.SetActive(false);
            }
        }

        if (!foundAndEquipped)
        {
            Debug.LogWarning($"Visual for item '{targetItemName}' not found in WeaponVisuals list.");
        }
    }
    private void HandleEquitpment(string targetItemName)
    {
        if (LeftHand == null) return;
        bool foundAndEquipped = false;
        foreach (GameObject equitpment in equitpMentvis)
        {
            if (equitpment == null) continue;
            bool isTargetequitp = equitpment.name.Contains(targetItemName) || equitpment.CompareTag(targetItemName);

            if (isTargetequitp)
            {
                equitpment.SetActive(true);
                foundAndEquipped = true;
                Debug.Log($"Equipped: {equitpment.name}");
            }
            else
            {
                equitpment.SetActive(false);
            }

        }
        if (!foundAndEquipped)
        {
            Debug.LogWarning($"Visual for item '{targetItemName}' not found in WeaponVisuals list.");
        }
    }

}