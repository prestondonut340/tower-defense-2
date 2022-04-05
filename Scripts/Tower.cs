﻿using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public TowerScriptableObject towerScriptableObject;
    private Enemy target;
    private Describable describable;
    private Transform _transform;
    public bool canShoot = false;
    private int timer;
    private int enemyLayer = 1 << 11;

    public static event Action<Tower> OnSelect;

    private void Awake()
    {
        describable = GetComponent<Describable>();
        _transform = transform;
    }

    private void FixedUpdate()
    {
        timer++;
        if (timer >= towerScriptableObject.attackDelay && target != null && canShoot)
        {
            timer = 0;
            Shoot();
        }
    }

    private void Update()
    {
        if (target != null && !target.Equals(null))
        {
            if (Vector3.Distance(target.Transform.position, _transform.position) > towerScriptableObject.range)
                target = null;
            else
                return;
        }

        var colliders = Physics.OverlapSphere(_transform.position, towerScriptableObject.range, enemyLayer);

        Enemy newTarget = null;
        int furthestIndex = 0;
        foreach (var collider in colliders)
        {
            if (!collider.TryGetComponent(out Enemy enemy)) continue;

            int pathPositionIndex = enemy.PathPositionIndex;
            if (pathPositionIndex > furthestIndex)
            {
                furthestIndex = pathPositionIndex;
                newTarget = enemy;
            }
        }
        target = newTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy) && target == null)
        {
            target = enemy;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out Enemy enemy) && target != null && target.Equals(enemy))
        {
            target = null;

            var colliders = Physics.OverlapSphere(transform.position, towerScriptableObject.range);

            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent(out enemy))
                {
                    target = enemy;
                }
            }
        }
    }

    private void Shoot()
    {
        var projectile = Instantiate(towerScriptableObject.bullet).GetComponent<Projectile>();
        projectile.Transform.position = transform.position;
        var directionEnemy = target.Transform.position - transform.position;

        projectile.direction = directionEnemy.normalized;
        projectile.speed = towerScriptableObject.bulletSpeed;
        projectile.damage = towerScriptableObject.damage;
        Destroy(projectile.gameObject, towerScriptableObject.lifeTime);
    }

    public void Upgrade(int upgradeIndex)
    {
        SetScriptableObject(towerScriptableObject.upgrades[upgradeIndex]);
    }

    public void SetScriptableObject(TowerScriptableObject towerScriptableObject){
        this.towerScriptableObject = towerScriptableObject;
        //GetComponent<SphereCollider>().radius = towerScriptableObject.range;
        var child = transform.GetChild(0);
        child.GetComponent<SphereCollider>().radius = towerScriptableObject.colliderSize;
        child.localPosition = new Vector3(0, towerScriptableObject.colliderSize / 2, 0);
    }

    private void OnMouseEnter()
    {
        var towerData = new TowerData
        {
            bulletSpeed = towerScriptableObject.bulletSpeed,
            damage = towerScriptableObject.damage,
            lifeTime = towerScriptableObject.lifeTime,
            attackDelay = towerScriptableObject.attackDelay,
            description = towerScriptableObject.description
        };

        describable.Inspect(towerData);
    }

    private void OnMouseExit()
    {
        describable.Uninspect();
    }

    private void OnMouseDown()
    {
        OnSelect?.Invoke(this);
    }
}
