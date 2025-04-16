using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class ReacherRobot : Agent
{
    public GameObject pendulumA;
    public GameObject pendulumB;
    public GameObject pendulumC;
    public GameObject pendulumD;

    Rigidbody m_RbA;
    Rigidbody m_RbB;
    Rigidbody m_RbC;
    Rigidbody m_RbD;

    public GameObject gripper;
    public GameObject droppoint;

    public List<GameObject> goalCubes;
    public GameObject attachedCube = null;
    
    public float releaseInterval = 3f;
    private int currentCubeIndex = 0;
    private float releaseTimer = 0f;
    public float timer;

    public float m_GoalSpeed = 0.5f;
    float goalX;
    float goalY;
    float goalZ;

    public bool hasGreenGoal = false;
    public bool IsGreenGoalAtDroppoint = false;

    public bool hasRedGoal = false;
    public bool IsRedGoalAtDroppoint = false;

    public ReacherReward rewardScript;

    public override void Initialize()
    {
        //Set rigidbodies
        m_RbA = pendulumA.GetComponent<Rigidbody>();
        m_RbB = pendulumB.GetComponent<Rigidbody>();
        m_RbC = pendulumC.GetComponent<Rigidbody>();
        m_RbD = pendulumD.GetComponent<Rigidbody>();

        //Reset State Variables
        hasGreenGoal = false;
        hasRedGoal = false;
        timer = 0f;
        attachedCube = null;
        currentCubeIndex = 0;
        releaseTimer = 0f;

        SetResetParameters();
        ShuffleGoalCubes();
    }

    //Set goal cube spawn position and reset their physics
    public void SetResetParameters()
    {
        goalX = 3f;
        goalY = 2.5f;
        goalZ = 6f;

        currentCubeIndex = 0;
        releaseTimer = 0f;
        timer = 0f;

        int index = 0;
        foreach (GameObject cube in goalCubes)
        {
            Rigidbody rb = cube.GetComponent<Rigidbody>();
            if (rb != null)
            {
            rb.isKinematic = false;
            }

            Vector3 offset = new Vector3(0f, 0f, index * 0.3f);
            cube.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position + offset;
            cube.transform.SetParent(null);

            Collider cubeCollider = cube.GetComponent<Collider>();
            if(cubeCollider != null)
            {
                cubeCollider.enabled = true;
            }
        }
        index ++;
    }

    //Randomly shuffle the cube list
    void ShuffleGoalCubes()
    {
        for (int i = 0; i < goalCubes.Count; i++)
        {
            int randomIndex = Random.Range(i, goalCubes.Count);
            GameObject temp = goalCubes[i];
            goalCubes[i] = goalCubes[randomIndex];
            goalCubes[randomIndex] = temp;
        }
    }


    //Reset robot and cube state
    public override void OnEpisodeBegin()
    {
        pendulumA.transform.position = new Vector3(-0.6186612f, -0.217f, -0.2392216f) + transform.position;
        pendulumA.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbA.velocity = Vector3.zero;
        m_RbA.angularVelocity = Vector3.zero;

        pendulumB.transform.position = new Vector3(-0.6186612f, -0.217f, -0.2392216f) + transform.position;
        pendulumB.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbB.velocity = Vector3.zero;
        m_RbB.angularVelocity = Vector3.zero;

        pendulumC.transform.position = new Vector3(-0.6186612f, -0.217f, -0.2392216f) + transform.position;
        pendulumC.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbC.velocity = Vector3.zero;
        m_RbC.angularVelocity = Vector3.zero;

        pendulumD.transform.position = new Vector3(-0.6186612f, -0.217f, -0.2392216f) + transform.position;
        pendulumD.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        m_RbD.velocity = Vector3.zero;
        m_RbD.angularVelocity = Vector3.zero;

        //Reset flags
        hasGreenGoal = false;
        hasRedGoal = false;
        IsGreenGoalAtDroppoint = false;
        IsRedGoalAtDroppoint = false;

        timer = 0f;
        currentCubeIndex = 0;
        releaseTimer = 0f;

        //Detach and reset cube position if any
        if (attachedCube != null)
        {
            attachedCube.transform.SetParent(null);
            attachedCube.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position;
            Rigidbody rb = attachedCube.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
            }

            if (!goalCubes.Contains(attachedCube))
            {
                goalCubes.Add(attachedCube);
            }
        
            attachedCube = null;
        }

        //Detach and reset stuck cube under gripper if any
        foreach (Transform child in gripper.transform)
        {
            GameObject cube = child.gameObject;

            if ((cube.CompareTag("GreenGoal") || cube.CompareTag("RedGoal")) && !goalCubes.Contains(cube))
            {
            cube.transform.SetParent(null);
            cube.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position;
            Rigidbody rb = cube.GetComponent<Rigidbody>();
            if(rb != null)
            {
                rb.isKinematic = false;
                rb.velocity = Vector3.zero;
            }
            Collider cubeCollider = cube.GetComponent<Collider>();
            if(cubeCollider != null)
            {
                cubeCollider.enabled = true;
            }
            if (!goalCubes.Contains(cube))
            {
                goalCubes.Add(cube);
            }
            }
        }

        ShuffleGoalCubes();
        //SetResetParameters();
        rewardScript.ResetRewardState(); //Reset reward state
        ResetCubePositions(); //Reset cube layout
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(pendulumA.transform.localPosition);
        sensor.AddObservation(pendulumA.transform.rotation);
        sensor.AddObservation(m_RbA.angularVelocity);
        sensor.AddObservation(m_RbA.velocity);

        sensor.AddObservation(pendulumB.transform.localPosition);
        sensor.AddObservation(pendulumB.transform.rotation);
        sensor.AddObservation(m_RbB.angularVelocity);
        sensor.AddObservation(m_RbB.velocity);

        sensor.AddObservation(pendulumC.transform.localPosition);
        sensor.AddObservation(pendulumC.transform.rotation);
        sensor.AddObservation(m_RbC.angularVelocity);
        sensor.AddObservation(m_RbC.velocity);

        sensor.AddObservation(pendulumD.transform.localPosition);
        sensor.AddObservation(pendulumD.transform.rotation);
        sensor.AddObservation(m_RbD.angularVelocity);
        sensor.AddObservation(m_RbD.velocity);

        sensor.AddObservation(gripper.transform.localPosition);

        sensor.AddObservation(m_GoalSpeed);
        sensor.AddObservation(hasGreenGoal ? 1f : 0f);
        sensor.AddObservation(hasRedGoal ? 1f : 0f);

        foreach (GameObject cube in goalCubes)
        {
            sensor.AddObservation(cube.transform.localPosition);
        }
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        var continuousActions = actionBuffers.ContinuousActions;

        //End episode and disable torque if flag is on
        if (rewardScript.disabletorque)
        {
            m_RbA.AddTorque(Vector3.zero);
            m_RbB.AddTorque(Vector3.zero);
            m_RbC.AddTorque(Vector3.zero);
            m_RbD.AddTorque(Vector3.zero);
            
            EndEpisode();
        }
        else //Set torques for robot arm components
        {
            var torque = Mathf.Clamp(continuousActions[0], -1f, 1f) * 10f;
            m_RbA.AddTorque(new Vector3(0f, 0f, torque));
        
            torque = Mathf.Clamp(continuousActions[1], -1f, 1f) * 10f;
            m_RbB.AddTorque(new Vector3(0f, torque, 0f));
        
            torque = Mathf.Clamp(continuousActions[1], -1f, 1f) * 10f;
            m_RbC.AddTorque(new Vector3(0f, torque, 0f));
        
            torque = Mathf.Clamp(continuousActions[1], -1f, 1f) * 10f;
            m_RbD.AddTorque(new Vector3(0f, torque, 0f));
        }

        //Reset unattached cubes every 30s
        timer += Time.deltaTime;
        if (timer >= 30f)
        {
            ResetCubePositions();
        }

        ProcessCubeRelease();
        UpdateGoalPosition();
    }

    //Timed release of cubes
    void ProcessCubeRelease()
    {
        if (currentCubeIndex >= goalCubes.Count)
            return;
        
        while (currentCubeIndex < goalCubes.Count && attachedCube == goalCubes[currentCubeIndex])
        {
            currentCubeIndex++;
            releaseTimer = releaseInterval;
        }
      
        releaseTimer += Time.deltaTime;
        if (releaseTimer >= releaseInterval)
        {
            SendCube(goalCubes[currentCubeIndex]);
            currentCubeIndex++;
            releaseTimer = 0f;
        }
    }

    //Send Cubes
    void SendCube(GameObject cube)
    {
        if (cube == attachedCube) 
            return;

        Rigidbody rb = cube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = new Vector3(0f, 0f, -m_GoalSpeed);
        }
    }

    //Reset all unattached cube positions if exceeds 30s
    void ResetCubePositions()
    {
        int index = 0;
        foreach (GameObject cube in goalCubes)
        {
            if (attachedCube != cube)
            {
                Vector3 offset = new Vector3(0f, 0f, index * 0.3f);
                cube.transform.position = new Vector3(goalX, goalY, goalZ) + transform.position + offset;

                Rigidbody rb = cube.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                }
            }
            else
            {
                UpdateGoalPosition(); //In case if attached
            }

            Collider cubeCollider = cube.GetComponent<Collider>();
            if(cubeCollider != null)
            {
                cubeCollider.enabled = true;
            }
        }
        index ++;

        currentCubeIndex = 0;
        releaseTimer = 0f;
        timer = 0f;
    }
    
    //Triggered by ReacherReward script, once the flag is rised
    public void AttachGreenGoal(GameObject cube)
    {
        if (attachedCube != null)
        {
            return;
        }

        hasGreenGoal = true;
        attachedCube = cube;
        Rigidbody rb = attachedCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; //Set the attached cube to kinematic
        }

        attachedCube.transform.SetParent(gripper.transform, true);
        //attachedCube.transform.position = gripper.transform.position;

        IsGreenGoalAtDroppoint = false;
        goalCubes.Remove(attachedCube);

        UpdateGoalPosition();
    }

    //Triggered by ReacherReward script, once the flag is rised
    public void AttachRedGoal(GameObject cube)
    {
        if (attachedCube != null)
        {
            return;
        }
        hasRedGoal = true;
        attachedCube = cube;
        Rigidbody rb = attachedCube.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true; //Set the attached cube to kinematic
        }

        attachedCube.transform.SetParent(gripper.transform, true);
        //attachedCube.transform.position = gripper.transform.position;

        IsRedGoalAtDroppoint = false;
        goalCubes.Remove(attachedCube);

        UpdateGoalPosition();
    }

    //Once the gripper holds a cube, check if it has reached the drop point
    void UpdateGoalPosition()
    {       
        if(hasGreenGoal)
        {
            if (!IsRedGoalAtDroppoint && !IsGreenGoalAtDroppoint)
            {
                if (Vector3.Distance(attachedCube.transform.position, droppoint.transform.position) < 2f)
                {
                    Rigidbody rb = attachedCube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.isKinematic = false; //Set the attached cube to not kinematic if close enough
                    }

                    //attachedCube.transform.SetParent(null);
                    attachedCube.transform.SetParent(droppoint.transform, true);
                    attachedCube.transform.localPosition = Vector3.zero;

                    if (attachedCube.transform.parent == droppoint.transform)
                    {
                        IsGreenGoalAtDroppoint = true;
                        //attachedCube = null;
                    }
                }
            }
        }

        if(hasRedGoal)
        {
            if (!IsRedGoalAtDroppoint && !IsGreenGoalAtDroppoint)
            {
                if (Vector3.Distance(attachedCube.transform.position, droppoint.transform.position) < 2f)
                {
                    Rigidbody rb = attachedCube.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.isKinematic = false; //Set the attached cube to not kinematic if close enough
                    }

                    //attachedCube.transform.SetParent(null);
                    attachedCube.transform.SetParent(droppoint.transform, true);
                    attachedCube.transform.localPosition = Vector3.zero;

                    if (attachedCube.transform.parent == droppoint.transform)
                    {
                        IsRedGoalAtDroppoint = true;
                        //attachedCube = null;
                    }
                }
            }
        }
    }

}
