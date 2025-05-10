using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class RobotArm : MonoBehaviour
{
    public Transform end;
    public Transform target;
    private Vector3 targetStartPos;
    public List<Gripper> grippers;

    [Serializable] //Define movable objects and the corresponding drop target
    public class TargetObject
    {
        public Transform obj; //The object to be moved
        public Transform targetPos; //Where to be placed
    }
    public List<TargetObject> targetObjects; //List of objects and the target positions
    private bool isRunning;

    void Start()
    {
        targetStartPos = target.position;
    }

    void Update() //RGB for three separate objects selection
    {
        if (isRunning) //Skip if a grabbing sequence is running already
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Grab(targetObjects[0]);
        }
        else if (Input.GetKeyDown(KeyCode.G))
        {
            Grab(targetObjects[1]);
        }
        else if (Input.GetKeyDown(KeyCode.B))
        {
            Grab(targetObjects[2]);
        }
    }
    
    private void Grab(TargetObject targetObject) //Full pick-and-place sequence for a selected object
    {
        isRunning = true;
        Sequence sequence = DOTween.Sequence();

        //Move above the object
        Vector3 pos0 = new Vector3(targetObject.obj.position.x, targetObject.obj.position.y + 5f, targetObject.obj.position.z);
        sequence.Append(target.DOMove(pos0, 8f).SetEase(Ease.Linear));

        //Open grippers
        sequence.AppendCallback(delegate
        {
            foreach (var item in grippers)
            {
                item.Open();
            }
        });
        sequence.AppendInterval(1f);

        //Lower down to grab position
        sequence.Append(target.DOMove(targetObject.obj.position, 8f).SetEase(Ease.Linear));
        
        //Close Gripper
        sequence.AppendCallback(delegate
        {
            foreach (var item in grippers)
            {
                item.Close();
            }
        });
        sequence.AppendInterval(1f);

        //Attach the picked object to mimic picking up
        sequence.AppendCallback(delegate
        {
            targetObject.obj.GetComponent<Rigidbody>().isKinematic = true;
            targetObject.obj.parent = end;
        });

        //Lift up
        Vector3 pos1 = new Vector3(pos0.x, targetObject.targetPos.position.y, pos0.z);
        sequence.Append(target.DOMove(pos1, 8f).SetEase(Ease.Linear));
        
        //Move to drop
        sequence.Append(target.DOMove(targetObject.targetPos.position, 8f).SetEase(Ease.Linear));
        
        //Release
        sequence.AppendCallback(delegate
        {
            foreach (var item in grippers)
            {
                item.Open();
            }
            targetObject.obj.parent = null;
            targetObject.obj.GetComponent<Rigidbody>().isKinematic = false;
        });
        sequence.AppendInterval(1f);
        
        //Return to original position in two steps
        Vector3 pos2 = new Vector3(targetStartPos.x, pos1.y, targetStartPos.z);
        sequence.Append(target.DOMove(pos2, 8f).SetEase(Ease.Linear));
        sequence.Append(target.DOMove(targetStartPos, 8f).SetEase(Ease.Linear));
        
        //Close the gripper
        sequence.AppendCallback(delegate
        {
            foreach (var item in grippers)
            {
                item.Close();
            }
        });
        sequence.AppendInterval(1f);

        //Mark the sequence as a complete
        sequence.AppendCallback(delegate
        {
            isRunning = false;
        });
    }
}
