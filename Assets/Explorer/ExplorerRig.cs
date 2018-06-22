using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerRig {
    Explorer global;
    public GameObject goMain;
    public ExplorerSensorThruster[] sensorThrusters;
    public int numSensorThrusters = 2;
    public Vector3 posLast;
    public float tolerance = .01f;
    float yawDelta = 5;
    float yDelta = .1f;
    //
    public ExplorerRig(Explorer global0) {
        global = global0;
        //
        goMain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        goMain.name = "rig";
        //
        float offset = .9f;
        //
        sensorThrusters = new ExplorerSensorThruster[numSensorThrusters];
        sensorThrusters[0] = new ExplorerSensorThruster(goMain, "forward");
        sensorThrusters[0].camGo.transform.Rotate(0, 0, 0);
        sensorThrusters[0].display.transform.localPosition = new Vector3(0, 0, -offset);
        //
        sensorThrusters[1] = new ExplorerSensorThruster(goMain, "down");
        sensorThrusters[1].camGo.transform.Rotate(90, 0, 0);
        sensorThrusters[1].display.transform.localPosition = new Vector3(0, 0, -offset);
    }
    public void Update()
    {
        Vector3 pos = goMain.transform.position;
        for (int n = 0; n < numSensorThrusters; n++) {
            //
            sensorThrusters[n].Update();
            pos += sensorThrusters[n].camGo.transform.forward * sensorThrusters[n].delta;
        }
        goMain.transform.position = pos;
        //
        float dist = Vector3.Distance(pos, posLast);
        if (dist < tolerance)
        {
//            Move();
//            Rotate();
        }
        posLast = goMain.transform.position;
    }
     
    public void Move() {
        goMain.transform.position += goMain.transform.up * yDelta * 3;    
        Debug.Log("move y:" + goMain.transform.position.y + "\n");
    }
    public void Rotate() {
        goMain.transform.Rotate(0, yawDelta, 0);
        Debug.Log("yaw:" + goMain.transform.eulerAngles.y + "\n");
    }
}
