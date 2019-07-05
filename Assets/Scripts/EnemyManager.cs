﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyManager : MonoBehaviour {

    private List<Enemy> enemies = new List<Enemy>();
    private int aliveCount;
    static private EnemyManager instance;

    private List<System.Action> enemyDyingListeners = new List<System.Action>();

    public static EnemyManager Instance { get => instance; }

    public EnemyManager() { if (instance == null) instance = this; }
    IEnumerator RemoveDead()
    {
        Enemy it;
        while (true)
        {
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                if ((it = enemies[i]) == null)
                {
                    enemies.Remove(it);
                }
            }
            yield return new WaitForSeconds(10);
        }
    }

    private void Awake()
    {
        if (instance != null && this != instance) 
        {
            Debug.LogError($"{this.GetType().Name} cannot exist double or more in one scene. GameObject '{name}' has been Deleted because it has second {this.GetType().Name}.");
            //Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        StartCoroutine( RemoveDead() );
    }

    public Enemy Summon(Enemy target,Vector3 position,Quaternion quaternion)
    {
        var r = Instantiate(target, position, quaternion);
        r.manager = this;
        return r;
    }

    public void AddNewEnemy(Enemy enemy)
    {
        if(enemy == null) { return; }

        aliveCount++;
        enemies.Add(enemy);
    }

    public void AddEnemyDyingListener(System.Action listener)
    {
        enemyDyingListeners.Add(listener);
    }

    public void EnemyDying()
    {
        foreach(var f in enemyDyingListeners)
        {
            f();
        }
    }
    
}
