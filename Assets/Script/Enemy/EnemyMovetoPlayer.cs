using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovetoPlayer : Enemy
{
    public float searchRadius = 5f;
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
            SetAnimationRun(true);
            Move(directionToTarget.normalized);
            currentState = State.Chase;
        }
        else
        {
            SetAnimationRun(false);
            SetAnimationState(false);
            currentState = State.Idel;
        }
    }
}
