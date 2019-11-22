using UnityEngine;
using System.Collections;

public class TankBase : MonoBehaviour
{
    public float Speed = 10.0f;
    public float RotSpeed = 20.0f;

    protected Genome genome;
	protected NeuralNetwork brain;
    protected GameObject nearTank;
    protected float[] inputs;
    public LayerMask tankLayer;

    // Sets a brain to the tank
    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    public void SetNearestTank(GameObject tank)
    {
        nearTank = tank;
    }

    protected Vector3 GetDirToTank(GameObject tank)
    {
        return (tank.transform.position - this.transform.position).normalized;
    }

    protected bool IsCloseToTank(GameObject tank)
    {
        return (this.transform.position - tank.transform.position).sqrMagnitude <= 10.0f;
    }

    protected bool IsOnSight(GameObject tank)
    {
        return Physics.Raycast(transform.position + new Vector3(0,0.5f,0), (tank.transform.position - transform.position).normalized, 10f, tankLayer);
    }

    protected void SetForces(float leftForce, float rightForce, float dt)
    {
        // Tank position
        Vector3 pos = this.transform.position;

        // Use the outputs as the force of both tank tracks
        float rotFactor = Mathf.Clamp((rightForce - leftForce), -1.0f, 1.0f);

        // Rotate the tank as the rotation factor
        this.transform.rotation *= Quaternion.AngleAxis(rotFactor * RotSpeed * dt, Vector3.up);

        // Move the tank in current forward direction
        pos += this.transform.forward * Mathf.Abs(rightForce + leftForce) * 0.5f * Speed * dt;

        // Sets current position
        this.transform.position = pos;

    }

	// Update is called once per frame
	public void Think(float dt) 
	{
        OnThink(dt);

        if(IsCloseToTank(nearTank))
            if (IsOnSight(nearTank))
                OnTankInSight(nearTank);
	}

    protected virtual void OnThink(float dt)
    {

    }

    protected virtual void OnTankInSight(GameObject tank)
    {
    }

    protected virtual void OnReset()
    {

    }
}
