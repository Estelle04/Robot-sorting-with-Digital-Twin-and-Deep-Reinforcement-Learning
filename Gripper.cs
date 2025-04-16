using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Gripper : MonoBehaviour
{
    public float openPosX;
    private Tweener moveTweener; //Active Tween, can be stopped when needed
    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O)) //Manual Open or Close
        {
            Open();
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            Close();
        }
    }
    public void Open() //Open
    {
        if (moveTweener != null)
        {
            moveTweener.Kill(); //Stop any previous movement
        }
        moveTweener = this.transform.DOLocalMoveX(openPosX, 1f).SetEase(Ease.Linear);
    }
    public void Close()//Close
    {
        if (moveTweener != null)
        {
            moveTweener.Kill(); //Stop any previous movement
        }
        moveTweener = this.transform.DOLocalMoveX(0f, 1f).SetEase(Ease.Linear);
    }
    void OnCollisionEnter(Collision collision) //Gripper stop moving when it touches the object
    {
        if (moveTweener != null)
        {
            moveTweener.Kill(); //Stop movement on collision
        }
    }

}
