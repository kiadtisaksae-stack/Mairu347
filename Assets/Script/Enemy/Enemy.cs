using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(NetworkRigidbody))]
public class Enemy : Character
{
    [Header("Enemy Type")]
    public EnemyType enemyType;
    protected enum State { Idel, Chase, Attack, Death }

    [Header("Attack Settings")]
    protected float TimeToAttack = 1f;
    protected float AttackRange = 1.5f;
    protected bool isRun = false;

    protected State currentState = State.Idel;
    protected float timer = 0f;
    protected Rigidbody rb;
    protected SphereCollider sphereCollider;
    protected Player _targetPlayer;

    [Header("Xp Drop")]
    public int xpValue = 50;
    public float xpShareRadius = 15f;

    public override void OnNetworkSpawn()
    {
        this._initialMaxHealth = enemyType.initialMaxHealth;
        this.Name = enemyType.enemyName;
        this.Defence = enemyType.defence;
        this.Damage = enemyType.damage;
        this.movementSpeed = enemyType.movementSpeed;
        this.xpValue = enemyType.experience;
        this.xpShareRadius = enemyType.xpShareRadius;
        this.AttackRange = enemyType.attackRange;
        this.TimeToAttack = enemyType.attackCooldown;
        base.OnNetworkSpawn();
    }

    public override void TakeDamage(int amount)
    {
        if (!IsServer) return;

        int actualDamage = Mathf.Clamp(amount - Defence, 1, amount);
        health -= actualDamage;

        if (health <= 0)
        {
            if (QuestManager.Instance != null && enemyType != null)
                QuestManager.Instance.OnEnemyKilled(enemyType);

            ShareXpInRadius();
            DropReward();
            SetAnimDie(true);
            InvokeOnDestroy();
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public override void SetUP()
    {
        base.SetUP();
        rb = GetComponent<Rigidbody>();
        sphereCollider = GetComponent<SphereCollider>();
        if (animator == null)
            Debug.LogError("Animator not found on " + gameObject.name);
    }

    // ─────────────────────────────────────────
    // Drop — อ่านจาก EnemyType.dropTable
    // ─────────────────────────────────────────
    protected virtual void DropReward()
    {
        if (!IsServer) return;
        if (enemyType == null || enemyType.dropTable == null || enemyType.dropTable.Count == 0) return;

        // คำนวณ total weight ครั้งเดียว
        int totalWeight = 0;
        foreach (var entry in enemyType.dropTable)
            totalWeight += entry.weight;

        if (totalWeight <= 0) return;

        Vector3 dropPos = transform.position + Vector3.up * 0.2f;

        for (int i = 0; i < enemyType.dropCount; i++)
        {
            GameObject prefab = GetRandomDrop(totalWeight);
            if (prefab == null) continue;

            NetworkObject netObj = prefab.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                // prefab มี NetworkObject → Spawn ผ่าน Network
                NetworkObject dropped = Instantiate(netObj, dropPos, Quaternion.identity);
                dropped.Spawn(true);
            }
            else
            {
                // prefab ธรรมดา → Instantiate ปกติ
                Instantiate(prefab, dropPos, Quaternion.identity);
            }
        }
    }

    private GameObject GetRandomDrop(int totalWeight)
    {
        int roll = Random.Range(0, totalWeight);
        int cumulative = 0;

        foreach (var entry in enemyType.dropTable)
        {
            cumulative += entry.weight;
            if (roll < cumulative)
                return entry.prefab;
        }

        return null;
    }

    // ─────────────────────────────────────────
    // XP
    // ─────────────────────────────────────────
    private void ShareXpInRadius()
    {
        PlayerLevel[] allPlayers = FindObjectsByType<PlayerLevel>(FindObjectsSortMode.None);
        if (allPlayers.Length == 0) return;

        int averageXp = xpValue / allPlayers.Length;
        foreach (PlayerLevel player in allPlayers)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= xpShareRadius)
                player.AddExperience(averageXp);
        }
    }

    // ─────────────────────────────────────────
    // Movement / Combat helpers
    // ─────────────────────────────────────────
    protected virtual void Turn(Vector3 direction)
    {
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * 10f);
        }
    }

    protected virtual void Move(Vector3 direction)
    {
        rb.linearVelocity = new Vector3(direction.x * movementSpeed, rb.linearVelocity.y, direction.z * movementSpeed);
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
        if (animator.GetBool("Attack") != isAttacking)
            animator.SetBool("Attack", isAttacking);
    }

    protected void SetAnimationRun(bool isRun)
    {
        bool HasParameter(Animator anim, string paramName)
        {
            foreach (AnimatorControllerParameter param in anim.parameters)
                if (param.name == paramName) return true;
            return false;
        }
        if (!HasParameter(animator, "Run")) return;
        if (animator.GetBool("Run") != isRun)
            animator.SetBool("Run", isRun);
    }

    protected void SetAnimDie(bool die)
    {
        if (animator.GetBool("Die") != die)
            animator.SetBool("Die", die);
    }
}