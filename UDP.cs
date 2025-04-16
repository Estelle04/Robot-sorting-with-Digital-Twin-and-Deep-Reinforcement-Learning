using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class JointAngleSender : MonoBehaviour
{
    [Header("Joint Transforms")]
    public Transform baseJoint;       
    public Transform shoulderJoint;   
    public Transform elbowJoint;      
    public Transform wristJoint;      
    public Transform LArm;
    public Transform RArm;

    [Header("UDP Settings")]
    public string nodeRedIP = "127.0.0.1"; //Define IP address to send UDP to
    public int nodeRedPort = 1234; //UDP Port to send messages to

    private UdpClient udpClient;

    [Header("Update Settings")]
    public float updateInterval = 2f; //Time Interval in sending updates
    private float timer = 0f; //Interal Timer


    [Header("Baseline Angles (Degrees)")]
    public float baseAngleBaseline = 0f;      
    public float shoulderAngleBaseline = 0f; 
    public float elbowAngleBaseline = 0f;
    public float wristAngleBaseline = 0f; 


    void Start()
    {
        udpClient = new UdpClient();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= updateInterval)
        {
            timer = 0f;

            //Base Joint Angle (on y-axis)
            float baseAngleCurrent = baseJoint.localEulerAngles.y;

            //Raw Joint Angle of shoulder, elbow, and wrist (on Z-axis)
            float shoulderAngleCurrent = shoulderJoint.localEulerAngles.z;
            float elbowAngleCurrent = elbowJoint.localEulerAngles.z;
            float wristAngleCurrent = wristJoint.localEulerAngles.z;

            //Adjust the angles with respect to the offsets between the physical robot and the simulation
            float baseAngleDelta = baseAngleCurrent;
            float shoulderAngleDelta = shoulderAngleBaseline + shoulderAngleCurrent - 6.38f;
            float elbowAngleDelta = elbowAngleBaseline + elbowAngleCurrent - 113f;
            float wristAngleDelta = wristAngleBaseline + wristAngleCurrent + 28.66f;

            //Normalize
            if (baseAngleCurrent > 180f) baseAngleCurrent -= 360f;
            if (shoulderAngleDelta > 180f) shoulderAngleDelta -= 360f;
            if (elbowAngleDelta > 180f) elbowAngleDelta -= 360f;
            if (wristAngleDelta > 180f) wristAngleDelta -= 360f;

            //Horizontal distance between grippers
            float diff = Mathf.Abs(LArm.localPosition.x - RArm.localPosition.x);
            diff = Mathf.Clamp(diff, 0f, 0.0006f); //Clamp
            float normalizedDiff = Mathf.InverseLerp(0f, 0.0006f, diff); //Normalize
            float gripperAngle = normalizedDiff * (0.01f - (-0.01f)) + (-0.01f);//Mapping

            //Integrate into a string
            string angleString = string.Format("{0:F2},{1:F2},{2:F2},{3:F2},{4:F2}",
                baseAngleDelta, shoulderAngleDelta, elbowAngleDelta, wristAngleDelta, gripperAngle);

            //Send through UDP
            SendAnglesOverUDP(angleString);
        }
    }


    private void SendAnglesOverUDP(string message) //Encoding
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        udpClient.Send(data, data.Length, nodeRedIP, nodeRedPort);
    }

    void OnApplicationQuit() //Close the UDP client when application exits
    {
        udpClient.Close();
    }
}
