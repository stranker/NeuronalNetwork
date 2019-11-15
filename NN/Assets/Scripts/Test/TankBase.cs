using UnityEngine;
using System.Collections;

public class TankBase : MonoBehaviour
{
    public float Speed = 10.0f;
    public float RotSpeed = 20.0f;

    protected Genome genome;
	protected NeuralNetwork brain;
    protected GameObject nearMine;
    protected GameObject goodMine;
    protected GameObject badMine;
    protected float[] inputs;

    // Sets a brain to the tank
    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    // Used by the PopulationManager to set the closest mine
    public void SetNearestMine(GameObject mine)
    {
        nearMine = mine;
    }

    public void SetGoodNearestMine(GameObject mine)
    {
        goodMine = mine;
    }

    public void SetBadNearestMine(GameObject mine)
    {
        badMine = mine;
    }

    protected bool IsGoodMine(GameObject mine)
    {
        return goodMine == mine;
    }

    protected Vector3 GetDirToMine(GameObject mine)
    {
        return (mine.transform.position - this.transform.position).normalized;
    }
    
    protected bool IsCloseToMine(GameObject mine)
    {
        return (this.transform.position - nearMine.transform.position).sqrMagnitude <= 2.0f;
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

        if(IsCloseToMine(nearMine))
        {
            OnTakeMine(nearMine);
            // Move the mine to a random position in the screen
            PopulationManager.Instance.RelocateMine(nearMine);
        }
	}

    protected virtual void OnThink(float dt)
    {

    }

    protected virtual void OnTakeMine(GameObject mine)
    {
    }

    protected virtual void OnReset()
    {

    }
}
