using UnityEngine;
using System.Collections;
using System;

public class Tank : TankBase
{
    public Bullet bullet;
    public GameObject bulletInstance;
    public bool canShoot = true;
    public Transform shootPos;
    public float timer;

    public float fitness = 0;

    private void Start()
    {
        if (bulletInstance == null)
        {
            bulletInstance = Instantiate(bullet.gameObject, transform.parent);
            bulletInstance.GetComponent<Bullet>().SetOwner(this);
            bulletInstance.SetActive(false);
        }
    }

    protected override void OnReset()
    {
        fitness = 1;
    }

    protected override void OnThink(float dt) 
	{
        // Direction to closest mine (normalized!)
        Vector3 diToTank = GetDirToTank(nearTank);

        // Current tank view direction (it's always normalized)
        Vector3 dir = this.transform.forward;

        // Sets current tank view direction and direction to the mine as inputs to the Neural Network
        inputs[0] = diToTank.x;
        inputs[1] = diToTank.z;
        inputs[2] = dir.x;
        inputs[3] = dir.z;

        // Think!!! 
        float[] output = brain.Synapsis(inputs);

        SetForces(output[0], output[1], dt);

        if (!canShoot)
        {
            timer += dt;
            if (timer >= 3)
            {
                canShoot = true;
                timer = 0;
            }
        }


        if (bulletInstance)
        {
            bulletInstance.GetComponent<Bullet>().UpdateMovement(dt);
        }

    }

    public void ResetCanShoot()
    {
        canShoot = true;
    }

    internal void DecreaseFitness()
    {
        fitness *= 0.5f;
        genome.fitness = fitness;
    }

    internal void IncrementFitness()
    {
        fitness *= 2;
        genome.fitness = fitness;
    }

    protected override void OnTankInSight(GameObject tank)
    {
        Shoot(tank);
    }

    private void Shoot(GameObject tank)
    {
        if (canShoot)
        {
            timer = 0;
            canShoot = false;
            bulletInstance.gameObject.SetActive(true);
            bulletInstance.GetComponent<Bullet>().Shoot(transform.forward, tank, shootPos);
        }
    }

    public void DestroyBullet()
    {
        Destroy(bulletInstance);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(shootPos.position, transform.forward * 10);
    }

}
