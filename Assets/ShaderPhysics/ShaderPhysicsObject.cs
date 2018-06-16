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
    ShaderPhysics global;
    int index;
    public ShaderPhysicsObject(int index0, ShaderPhysics global0) {
        global = global0;
        index = index0;
        UpdateParents();
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.parent = global.parentObjectsGo.transform;
        go.transform.position = global.GetObjectPos();
        go.name = "object " + index;
        float s = global.objectScale;
        go.transform.localScale = new Vector3(s, s, s);
        float yaw = Random.Range(0, 360f);
        go.transform.Rotate(0, yaw, 0);
        //
        cam = go.AddComponent<Camera>();
        RenderTexture renderTexture = new RenderTexture(global.resCam, global.resCam, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = renderTexture;
        cam.targetTexture.filterMode = FilterMode.Point;
        cam.nearClipPlane = .1f;
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer(global.layerHidden));
        //
        displayTexture = new Texture2D(global.resCam, global.resCam);
        displayGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayGo.transform.localScale = new Vector3(10, 10, 10);
        displayGo.transform.eulerAngles = new Vector3(0, 0, 0);
        displayGo.GetComponent<Renderer>().material.mainTexture = displayTexture;
        //
        pointerGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pointerGo.transform.parent = global.parentObjectsGo.transform;
        //
        targetGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        targetGo.layer = LayerMask.NameToLayer(global.layerHidden);
        targetGo.transform.parent = global.parentTargetsGo.transform;
        targetGo.transform.localScale = new Vector3(1, 1, 1);
        //
        lookGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        lookGo.layer = LayerMask.NameToLayer(global.layerHidden);
        lookGo.transform.parent = global.parentTargetsGo.transform;
        lookGo.transform.localScale = new Vector3(1, 1, 1);
        lookGo.GetComponent<Renderer>().material.color = Color.cyan;
        //
        global.objects.Add(this);
    }
    void UpdateParents() {
        if (global.parentTargetsGo == null)
        {
            global.parentTargetsGo = new GameObject("parentTargetsGo");
        }
        if (global.parentObjectsGo == null)
        {
            global.parentObjectsGo = new GameObject("objects:" + global.numObjects);
        }
    }
}
