using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySword : MonoBehaviour
{
    public Enemy enemy;

    private Router _router;

    private void Start()
    {
        _router = FindFirstObjectByType<Router>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && enemy.attack)
        {
            float damage = Random.Range(enemy.damage[0], enemy.damage[1]);
            _router.player.combat.TakeDamage((int)damage, enemy);
        }
    }
}
