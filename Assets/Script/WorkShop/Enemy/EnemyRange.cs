using UnityEngine;

public class EnemyRange : Enemy
{
    public float attackRange = 5f; // Range within which the enemy can attack

    private void Update()
    {
        if (!IsServer) return;
        _targetPlayer = GetClosestPlayer();
        if (_targetPlayer == null)
        {
            SetAnimationState(false);
            return;
        }
        Vector3 directionToTarget = _targetPlayer.transform.position - transform.position;
        float distanceToTarget = directionToTarget.magnitude;

        Turn(directionToTarget);
        timer -= Time.fixedDeltaTime;

        if (GetDistanClosestPlayer() < attackRange)
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
}
