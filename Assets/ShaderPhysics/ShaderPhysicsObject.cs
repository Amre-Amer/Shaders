using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysicsObject : MonoBehaviour {
    public GameObject go;
    public GameObject displayGo;
    public GameObject pointerGo;
    public GameObject targetGo;
    public GameObject lookGo;
    public Camera cam;
    public Vector3 target;
    public Vector3 look;
    public Vector3 displayTarget;
    public Color aveColor;
    public bool ynSensor;
    public bool ynSensorLast;
    public float diffColor;
    public float yawPointer;
    public Texture2D displayTexture;
    public int index;
    ShaderPhysics global;
    public ShaderPhysicsObject(int index0, ShaderPhysics global0) {
        index = index0;
        global = global0;
        UpdateParents();
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.parent = global.dataManager.parentObjectsGo.transform;
        go.transform.position = global.miscManager.GetRandomObjectPos();
        float s = global.dataManager.objectScale;
        go.transform.localScale = new Vector3(s, s, s);
        float yaw = Random.Range(0, 360f);
        go.transform.Rotate(0, yaw, 0);
        //
        cam = go.AddComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(global.dataManager.resCam, global.dataManager.resCam, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = renderTexture;
        cam.targetTexture.filterMode = FilterMode.Point;
        cam.nearClipPlane = .1f;
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer(global.dataManager.layerHidden));
        //
        displayTexture = new Texture2D(global.dataManager.resCam, global.dataManager.resCam);
        displayGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayGo.transform.parent = global.dataManager.parentTargetsGo.transform;
        displayGo.transform.localScale = new Vector3(10, 10, 10);
        displayGo.transform.eulerAngles = new Vector3(0, 0, 0);
        displayGo.GetComponent<Renderer>().material.mainTexture = displayTexture;
        //
        pointerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointerGo.transform.parent = global.dataManager.parentObjectsGo.transform;
        //
        targetGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetGo.layer = LayerMask.NameToLayer(global.dataManager.layerHidden);
        targetGo.transform.parent = global.dataManager.parentTargetsGo.transform;
        targetGo.transform.localScale = new Vector3(1, 1, 1);
        //
        lookGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lookGo.layer = LayerMask.NameToLayer(global.dataManager.layerHidden);
        lookGo.transform.parent = global.dataManager.parentTargetsGo.transform;
        lookGo.transform.localScale = new Vector3(1, 1, 1);
        lookGo.GetComponent<Renderer>().material.color = Color.cyan;
        //
        go.name = "object";
    }
    void UpdateParents() {
        if (global.dataManager.parentTargetsGo == null)
        {
            global.dataManager.parentTargetsGo = new GameObject("parentTargetsGo");
        }
        if (global.dataManager.parentObjectsGo == null)
        {
            global.dataManager.parentObjectsGo = new GameObject("objects");
        }
    }
    public void UpdateSensorValue()
    {
        ynSensor = false;
        aveColor = GetAveColorCam(cam);
        Color colorGo = aveColor;
        diffColor = global.miscManager.GetColorDiffTarget(aveColor);
        if (diffColor < global.dataManager.colorThreshold)
        {
            colorGo = Color.red;
            ynSensor = true;
        }
        go.GetComponent<Renderer>().material.color = colorGo;
    }

    public Color GetAveColorCam(Camera cam)
    {
        RenderTexture.active = cam.targetTexture;
        Texture2D tex = new Texture2D(global.dataManager.resCam, global.dataManager.resCam);
        tex.ReadPixels(new Rect(0, 0, global.dataManager.resCam, global.dataManager.resCam), 0, 0);
        //
        Color aveColor = global.miscManager.GetAveColorTexture(tex);
        Destroy(tex);  // avoids memory leak 
        return aveColor;
    }

    public void UpdateDisplay()
    {
        float heightDisplayAboveObject = 30;
        displayGo.transform.position = go.transform.position + Vector3.up * heightDisplayAboveObject;
        RenderTexture.active = cam.targetTexture;
        displayTexture.ReadPixels(new Rect(0, 0, global.dataManager.resCam, global.dataManager.resCam), 0, 0);
        //
        float sumX = 0;
        float sumY = 0;
        float cx = 0;
        float cy = 0;
        int cnt = 0;
        //
        for (int nx = 0; nx < displayTexture.width; nx++)
        {
            for (int ny = 0; ny < displayTexture.height; ny++)
            {
                Color col = displayTexture.GetPixel(nx, ny);
                float diff = global.miscManager.GetColorDiffTarget(col);
                float value = 1 - diff;
                col = global.dataManager.colorTarget * value;
                displayTexture.SetPixel(nx, ny, col);
                if (value > global.dataManager.colorThreshold)
                {
                    sumX += nx * value;
                    sumY += ny * value;
                    cnt++;
                }
                if (value > global.dataManager.colorThreshold)
                {
                    displayTexture.SetPixel(nx, ny, Color.yellow);
                }
            }
        }
        cx = sumX / cnt;
        cy = sumY / cnt;
        if (ynSensor == true)
        {
            yawPointer = go.transform.eulerAngles.y + (cx / global.dataManager.resCam - .5f) * cam.fieldOfView;
            displayTexture.SetPixel((int)cx, (int)cy, Color.red);
            Vector3 pos = new Vector3(cx, cy, cam.nearClipPlane);
            displayTarget = cam.ScreenToWorldPoint(pos);
        }
        displayTexture.Apply();
    }

    public void UpdatePointer()
    {
        float scalePointer = 0;
        if (ynSensor == true)
        {
            scalePointer = 20;
        }
        pointerGo.transform.position = go.transform.position;
        pointerGo.transform.localScale = new Vector3(.1f, .1f, scalePointer);
        pointerGo.transform.eulerAngles = new Vector3(0, yawPointer, 0);
    }

    public void UpdateTarget()
    {
        if (global.dataManager.parentTargetsGo == null)
        {
            global.dataManager.parentTargetsGo = new GameObject("parentTargetsGo");
        }
        float clearance = 10;
        bool ynAvoid = global.AvoidNearestObject(index, clearance);
        if (ynSensor == true)
        {
            float yaw = yawPointer;
            Vector3 vector = go.transform.position - displayTarget;
            target = go.transform.position + Vector3.Normalize(vector) * clearance;
        }
        else
        {
            if (ynAvoid == false)
            {
                target = go.transform.position + go.transform.forward * clearance;
            }
        }
    }

    public void NavigateObject()
    {
        look = global.dataManager.smooth * look + (1 - global.dataManager.smooth) * target;
        //
        targetGo.transform.position = target;
        lookGo.transform.position = look;
        //
        go.transform.LookAt(look);
        go.transform.position += go.transform.forward * global.dataManager.speed;
        Vector3 eul = go.transform.eulerAngles;
        go.transform.eulerAngles = new Vector3(0, eul.y, 0);
        Vector3 pos = go.transform.position;
        go.transform.position = new Vector3(pos.x, global.dataManager.meshHeight / 2, pos.z);
    }

}
