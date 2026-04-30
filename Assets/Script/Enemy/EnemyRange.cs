using UnityEngine;

public class EnemyRange : Enemy
{
    private float rangedAttackRange = 5f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        this.rangedAttackRange = enemyType.attackRange;
    }

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

        if (GetDistanClosestPlayer() < rangedAttackRange)
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
