using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovetoPlayer : Enemy
{
    public float searchRadius = 5f;
    protected override EnemyType GetEnemyType()
    {
        return EnemyType.EnemyMovetoPlayer;
    }
    private void Update()
    {
        // 🚨 สำคัญ: ตรวจสอบเฉพาะบน Server/Host เท่านั้นที่ควรประมวลผล AI และการโจมตี
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
            // ระยะโจมตี
            Attack(_targetPlayer);
            currentState = State.Attack;
        }
        else if (GetDistanClosestPlayer() < searchRadius)
        {
            // ระยะไล่ล่า
            SetAnimationState(false);
            Move(directionToTarget.normalized);
            currentState = State.Chase;
        }
        else
        {
            SetAnimationState(false);
            currentState = State.Idel;
        }
    }
}
