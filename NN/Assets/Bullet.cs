using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    public float speed;
    private Tank owner;
    private Tank enemyTank;

    private float timer = 0;
    public float timeAlive = 4;

    private bool isActivated = false;

    public Vector3 dir;


    public void SetOwner(Tank tank)
    {
        owner = tank;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Tank" && other.gameObject != owner.gameObject && other.gameObject == enemyTank.gameObject)
        {
            owner.IncrementFitness();
            PopulationManager.Instance.RelocateTank(other.gameObject);
            Kill();
        }
    }

    private void Kill()
    {
        timer = 0;
        isActivated = false;
        owner.ResetCanShoot();
        gameObject.SetActive(false);
    }

    public void Shoot(Vector3 dir, GameObject tank, Transform shootPos)
    {
        isActivated = true;
        enemyTank = tank.GetComponent<Tank>();
        transform.position = shootPos.position;
        this.dir = dir;
        dir.y = 0;
    }

    public void UpdateMovement(float dt)
    {
        if (isActivated)
        {
            Vector3 pos = transform.position;
            pos += dir * 0.5f * speed * dt;
            transform.position = pos;
            timer += dt * 0.5f;
            if (timer > timeAlive)
            {
                timer = 0;
                owner.DecreaseFitness();
                Kill();
            }
        }
    }
}
