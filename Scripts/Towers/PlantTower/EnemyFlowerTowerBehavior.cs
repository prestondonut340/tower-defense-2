using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlowerTowerBehavior : TowerBehaviorBase
{
    private Transform headTransform;

    public override Vector3 BulletOrigin => headTransform.position;
    new private void Awake()
    {
        base.Awake();
        headTransform = _transform.GetChild(_transform.childCount -1).GetChild(0);
        StartCoroutine(FollowTargetCoroutine());
    }
    public override Enemy FindFurthestTarget()
    {
        Enemy newTarget = null;
        int furthestIndex = 0;
        var towerColliders = Physics.OverlapSphere(_transform.position, range, Layers.Tower);

        foreach(var towerCollider in towerColliders)
        {
            if (!towerCollider.TryGetComponent(out TowerBehaviorBase behavior)) continue;
            Enemy enemy;

            if(behavior.Equals(this))
                enemy = base.FindFurthestTarget();
            else
                enemy = behavior.FindFurthestTarget();

            if(enemy== null) continue;
            
            int pathPositionIndex = enemy.PathPositionIndex;
            if (pathPositionIndex > furthestIndex && behavior.IsTargetVisible(enemy))
            {
                furthestIndex = pathPositionIndex;
                newTarget = enemy;
            }
            
        }
        return newTarget;
    }

    private IEnumerator FollowTargetCoroutine()
    {
        while(true)
        {
            if(target == null || target.Equals(null)) goto endofloop;
            float time = Time.deltaTime * 20;
            var expectedPosition = target.positions[target.PathPositionIndex+8];
            expectedPosition += new Vector3(0, target.Height + 2, 0);
            // headTransform.position = Vector3.Lerp(headTransform.position, expectedPosition, time);
            var newPosition = Vector3.Lerp(headTransform.position, expectedPosition, time);
            var dirToNewPosition = newPosition - headTransform.position;
            if(dirToNewPosition.magnitude > time)
            {
                newPosition = headTransform.position + dirToNewPosition * time;
            }
            headTransform.position = newPosition;

            
            endofloop:
            yield return null;
        }
    }
}
