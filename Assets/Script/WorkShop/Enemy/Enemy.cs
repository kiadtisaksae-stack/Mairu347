using UnityEngine;
using Unity.Netcode; // สำหรับ OnNetworkSpawn และ Netcode
// สมมติว่าคลาส Player และ Character มีอยู่จริง
// และ Character สืบทอดมาจาก Identity ที่มี GetClosestPlayer()

[RequireComponent(typeof(Rigidbody))]
public class Enemy : Character
{
    // ⚙️ ตัวแปรสถานะและตั้งค่า
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
    public override void SetUP()
    {
        base.SetUP();
        rb = GetComponent<Rigidbody>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
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