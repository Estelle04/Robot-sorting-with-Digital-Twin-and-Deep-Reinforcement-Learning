using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReacherReward : MonoBehaviour
{
    public GameObject agent;
    public GameObject gripper;
    public GameObject rotateBase;
    public GameObject targetGoal; //Green Cube
    public GameObject targetGoal1;
    public GameObject nontargetGoal; //Red Cube
    public GameObject nontargetGoal1;
    public GameObject droppoint;

    private float gripperContactTimer = 0f;
    private float previousDistanceToGreen;
    private float previousDistanceToDroppoint;

    private Quaternion initialRotation;
    private bool greenGoalAttachReward = false;
    private bool dropPointRewardGiven = false;
    private bool redGoalAttachReward = false;
    private bool dropPointRedRewardGiven = false;
    private bool targetGoal_flag;
    private bool targetGoal1_flag;
    private bool nontargetGoal_flag;
    private bool nontargetGoal1_flag;

    private bool alreadyAttached = false;
    public bool disabletorque;
    private bool backrewardgiven = false;

    private float lastZAngle;
    private float originalrotation;

    private bool goalanglecounting = false;
    private float Timer2 = 0f;


    void Start()
    {
        previousDistanceToGreen = Vector3.Distance(targetGoal.transform.position, gripper.transform.position);
        previousDistanceToDroppoint = Vector3.Distance(gripper.transform.position, droppoint.transform.position);
        lastZAngle = rotateBase.transform.localEulerAngles.y;
        originalrotation = rotateBase.transform.localEulerAngles.y;
        initialRotation = rotateBase.transform.rotation;
    }

    //When gripper touches the cube
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == gripper)
        {
            if (gameObject.CompareTag("GreenGoal") && !alreadyAttached)
            {
                agent.GetComponent<ReacherRobot>().AddReward(0.5f);
            }
            else if (gameObject.CompareTag("RedGoal") && !alreadyAttached)
            {
                agent.GetComponent<ReacherRobot>().AddReward(-0.5f);
            }
        }
    }

    //Penalty for gripper hitting the ground
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            agent.GetComponent<ReacherRobot>().AddReward(-0.05f);
        }
    }


    //When gripper stays in contact with the cube for more than 1s, attach goal
    void OnTriggerStay(Collider other)
    {
        if (other.gameObject == gripper)
        {
            gripperContactTimer += Time.deltaTime;

                if (gameObject.CompareTag("GreenGoal") && !alreadyAttached)
                {
                    agent.GetComponent<ReacherRobot>().AttachGreenGoal(gameObject);
                    alreadyAttached = true;
                    if (gameObject == targetGoal){
                        targetGoal_flag = true;
                    }
                    if (gameObject == targetGoal1){
                        targetGoal1_flag = true;
                    }                    
                }
                else if (gameObject.CompareTag("RedGoal") && !alreadyAttached)
                {
                    agent.GetComponent<ReacherRobot>().AttachRedGoal(gameObject);
                    alreadyAttached = true;
                    if (gameObject == nontargetGoal){
                        nontargetGoal_flag = true;
                    }
                    if (gameObject == nontargetGoal1){
                        nontargetGoal1_flag = true;
                    }     
                }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == gripper)
        {
            gripperContactTimer = 0f; //Reset Timer once exit
        }
    }

    void Update()
    {
        //One-time reward when green goal is attached
        if (agent.GetComponent<ReacherRobot>().hasGreenGoal && !greenGoalAttachReward)
        {
            agent.GetComponent<ReacherRobot>().AddReward(5f);
            greenGoalAttachReward = true;
        }

        //One-time reward when green goal is dropped
        if (agent.GetComponent<ReacherRobot>().IsGreenGoalAtDroppoint && !dropPointRewardGiven)
        {
            agent.GetComponent<ReacherRobot>().AddReward(5f);
            dropPointRewardGiven = true;
        }

        //One-time penalty if red goal is attached
        if (agent.GetComponent<ReacherRobot>().hasRedGoal && !redGoalAttachReward)
        {
            agent.GetComponent<ReacherRobot>().AddReward(-5f);
            redGoalAttachReward = true;
        }

        //One-time penalty if red goal is delivered
        if (agent.GetComponent<ReacherRobot>().IsRedGoalAtDroppoint && !dropPointRedRewardGiven)
        {
            agent.GetComponent<ReacherRobot>().AddReward(-5f);
            dropPointRedRewardGiven = true;
        }

        //Shaped reward: reduce distance to droppoint while carrying green goal
        if (agent.GetComponent<ReacherRobot>().hasGreenGoal)
        {
            if (!agent.GetComponent<ReacherRobot>().IsGreenGoalAtDroppoint && targetGoal_flag)
            {
                float currentDistanceToDroppoint = Vector3.Distance(targetGoal.transform.position, droppoint.transform.position);
                float DropdistanceDelta = previousDistanceToDroppoint - currentDistanceToDroppoint;
                float DropshapingReward = DropdistanceDelta * 0.01f;
                agent.GetComponent<ReacherRobot>().AddReward(DropshapingReward);
                previousDistanceToDroppoint = currentDistanceToDroppoint;
            }
            if (!agent.GetComponent<ReacherRobot>().IsGreenGoalAtDroppoint && targetGoal1_flag)
            {
                float currentDistanceToDroppoint = Vector3.Distance(targetGoal1.transform.position, droppoint.transform.position);
                float DropdistanceDelta = previousDistanceToDroppoint - currentDistanceToDroppoint;
                float DropshapingReward = DropdistanceDelta * 0.01f;
                agent.GetComponent<ReacherRobot>().AddReward(DropshapingReward);
                previousDistanceToDroppoint = currentDistanceToDroppoint;
            }
            lastZAngle = rotateBase.transform.localEulerAngles.y;//Update base rotation starting point once the cube is attached for spin checking
        }
        else
        {
            lastZAngle = originalrotation;//If not carrying goal, take lastZAngle as original orientation
        }

        //Shaped reward: reduce distance to droppoint while carrying red goal (still penalized)
        if (agent.GetComponent<ReacherRobot>().hasRedGoal)
        {
            if (!agent.GetComponent<ReacherRobot>().IsRedGoalAtDroppoint && nontargetGoal_flag)
            {
                float currentDistanceToDroppoint = Vector3.Distance(nontargetGoal.transform.position, droppoint.transform.position);
                float DropdistanceDelta = previousDistanceToDroppoint - currentDistanceToDroppoint;
                float DropshapingReward = DropdistanceDelta * 0.01f;
                agent.GetComponent<ReacherRobot>().AddReward(DropshapingReward);
                previousDistanceToDroppoint = currentDistanceToDroppoint;
            }
            if (!agent.GetComponent<ReacherRobot>().IsRedGoalAtDroppoint && nontargetGoal1_flag)
            {
                float currentDistanceToDroppoint = Vector3.Distance(nontargetGoal1.transform.position, droppoint.transform.position);
                float DropdistanceDelta = previousDistanceToDroppoint - currentDistanceToDroppoint;
                float DropshapingReward = DropdistanceDelta * 0.01f;
                agent.GetComponent<ReacherRobot>().AddReward(DropshapingReward);
                previousDistanceToDroppoint = currentDistanceToDroppoint;
            }
            lastZAngle = rotateBase.transform.localEulerAngles.y;//Update base rotation starting point once the cube is attached for spin checking
        }
        else
        {
            lastZAngle = originalrotation;//If not carrying goal, take lastZAngle as original orientation
        }

        //Reward once the robot rotates back to a position that faces the front
        if (agent.GetComponent<ReacherRobot>().IsGreenGoalAtDroppoint || agent.GetComponent<ReacherRobot>().IsRedGoalAtDroppoint)
        {
            Quaternion currentRotation = rotateBase.transform.rotation;
            float angleDifference = Quaternion.Angle(initialRotation, currentRotation);
            if(Mathf.Abs(angleDifference) < 20f && !backrewardgiven)
            {
                disabletorque = true;
                backrewardgiven = true;
                agent.GetComponent<ReacherRobot>().AddReward(1f);
            }
        }

        //Penalty if full spin is detected (355 degrees is used here)
        float currentZAngle = rotateBase.transform.localEulerAngles.y;
        float deltaZ = Mathf.DeltaAngle(lastZAngle, currentZAngle);
        if (Mathf.Abs(deltaZ) >= 345f)
        {
            float fullSpinPenalty = 0.1f;
            agent.GetComponent<ReacherRobot>().AddReward(-fullSpinPenalty);
            goalanglecounting = false;
        }

        //Penalty once the robot flip over, stays stuck at side while rotating in a wrong direction
        if (!agent.GetComponent<ReacherRobot>().IsGreenGoalAtDroppoint || !agent.GetComponent<ReacherRobot>().IsRedGoalAtDroppoint)
        {
            if (agent.GetComponent<ReacherRobot>().pendulumD.transform.localPosition.x < -2f && (currentZAngle > 300f || currentZAngle < 70f))
            {
                Timer2 += Time.deltaTime;
                agent.GetComponent<ReacherRobot>().AddReward(-0.005f * Timer2);
            }
            else
            {
                Timer2 = 0f;
            }
        }

        //Tiny time penalty
        if (!disabletorque)
        {
            agent.GetComponent<ReacherRobot>().AddReward(-0.0001f);
        }
    }

    //Reset all flags
    public void ResetRewardState()
    {
        greenGoalAttachReward = false;
        dropPointRewardGiven = false;
        dropPointRedRewardGiven = false;
        backrewardgiven = false;
        disabletorque = false;
        goalanglecounting = false;
        alreadyAttached = false;
        targetGoal_flag = false;
        targetGoal1_flag = false;
        nontargetGoal_flag = false;
        nontargetGoal1_flag = false;
        gripperContactTimer = 0f;
    }
}
