using UnityEngine;
using Unity.Netcode; 

[RequireComponent(typeof(Rigidbody))]
public class Enemy : Character
{
    protected enum State { Idel, Chase, Attack, Death }
    [Header("Attack Settings")]
    [SerializeField]
    protected float TimeToAttack = 1f;
    [SerializeField]
    protected float AttackRange = 1.5f; // ระยะโจมตี
    
    protected State currentState = State.Idel;
    protected float timer = 0f;
    protected Rigidbody rb;
    
    protected Player _targetPlayer;

    [Header("Reward")]
    [SerializeField]
    private NetworkObject[] rewardItemPrefabs = new NetworkObject[3];
    [Header("Drop Rate Weights (Total should be 100)")]
    [Tooltip("Item 1 (Common)")]
    [SerializeField]
    private int dropWeight1 = 50;
    [Tooltip("Item 2 (Uncommon)")]
    [SerializeField]
    private int dropWeight2 = 30;
    [Tooltip("Item 3 (Rare)")]
    [SerializeField]
    private int dropWeight3 = 20;
    [SerializeField]
    private int rewardCount = 1;



    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;
        _targetPlayer = GetClosestPlayer();
        if (_targetPlayer == null)
        {
            SetAnimationState(false);
            return;
        }
        Vector3 directionToTarget = _targetPlayer.transform.position - transform.position;
        Turn(directionToTarget);
        timer -= Time.fixedDeltaTime;

        if (GetDistanClosestPlayer() < AttackRange)
        {
            Attack(_targetPlayer);
            currentState = State.Attack;
        }
        else
        {
            SetAnimationState(false);

            currentState = State.Chase;
        }
    }
    public override void TakeDamage(int amount)
    {
        if (!IsServer)
        {
            return;
        }

        int actualDamage = Mathf.Clamp(amount - Deffent, 1, amount);
        health -= actualDamage;


        ShowDamageClientRpc(actualDamage, transform.position);

        if (health <= 0)
        {
            
            DropReward();
            InvokeOnDestroy();
            GetComponent<NetworkObject>().Despawn();
            
        }
    }

    public override void SetUP()
    {
        base.SetUP();
        rb = GetComponent<Rigidbody>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
        }
    }
    protected virtual void DropReward()
    {
        if (!IsServer) return;

        // ตรวจสอบความถูกต้องของ Array
        if (rewardItemPrefabs.Length < 3 || rewardItemPrefabs[0] == null)
        {
            return;
        }

        Vector3 dropPosition = transform.position + Vector3.up * 0.2f; // Drop เหนือพื้นเล็กน้อย

        for (int i = 0; i < rewardCount; i++)
        {
            NetworkObject selectedPrefab = GetRandomWeightedReward();

            if (selectedPrefab != null)
            {
                NetworkObject droppedItem = Instantiate(selectedPrefab, dropPosition, Quaternion.identity);
                droppedItem.Spawn(true);
                Debug.Log($"[SERVER REWARD] Dropped item: {selectedPrefab.name}");
            }
        }
    }
    // logic for experience reward
    protected virtual void ExperienceReward()
    {

    }
    private NetworkObject GetRandomWeightedReward()
    {
        int totalWeight = dropWeight1 + dropWeight2 + dropWeight3;
        if (totalWeight <= 0)
        {
            Debug.LogError("[REWARD ERROR] Total drop weight is zero or negative. Check inspector values.");
            return rewardItemPrefabs[0];
        }
        int randomNumber = UnityEngine.Random.Range(0, totalWeight);

        if (randomNumber < dropWeight1)
        {
            return rewardItemPrefabs[0];
        }
        else if (randomNumber < dropWeight1 + dropWeight2)
        {
            return rewardItemPrefabs[1];
        }
        else
        {
            return rewardItemPrefabs[2];
        }
    }

    protected virtual void Turn(Vector3 direction)
    {
        // ลบแกน Y ออกจากการหมุนเพื่อไม่ให้ศัตรูเงย/ก้มตามพื้น
        direction.y = 0; 
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 10f); // หมุนแบบนุ่มนวล
        }
    }
    
    protected virtual void Move(Vector3 direction)
    {
        rb.linearVelocity = new Vector3(direction.x * movementSpeed, rb.linearVelocity.y, direction.z * movementSpeed);
        animator.SetFloat("Speed", rb.linearVelocity.magnitude);
    }
    
    protected virtual void Attack(Player _player) 
    {
        if (timer <= 0)
        {
            _player.TakeDamage(Damage); 
            SetAnimationState(true);
            Debug.Log($"{Name} attacks {_player.Name} for {Damage} damage.");
            timer = TimeToAttack;
        }
    }

    protected void SetAnimationState(bool isAttacking)
    {
        // ใช้ SetBool เพียงครั้งเดียวเพื่อป้องกันการเรียกซ้ำ
        if (animator.GetBool("Attack") != isAttacking)
        {
            animator.SetBool("Attack", isAttacking);
        }
    }
}