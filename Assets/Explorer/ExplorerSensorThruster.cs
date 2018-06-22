using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerSensorThruster {
    public float delta;
    public Camera cam;
    public GameObject display;
    public Color colorAve;
    public Color colorTarget = Color.blue;
    public Texture2D textureDisplay;
    public GameObject goParent;
    public GameObject camGo;
    public int camRes = 10;
    public float offset = .9f;
    public float diffColor;
    public float diffColorTarget = .5f;
    public float speed = .2f;
    public GameObject deltaGo;
    public string description;
    public string layerHidden = "HiddenFromCam";
    public float idleRange = .1f;
    public bool IsIdle;
    //
    public ExplorerSensorThruster(GameObject goParent0, string description0) {
        goParent = goParent0;
        description = description0;
        //
        camGo = new GameObject("camGo " + description);
        camGo.transform.parent = goParent.transform;
        cam = AddCameraToGo(camGo);
        //
        display = AddDisplayToGo(camGo, "display " + description);
        display.transform.localPosition = new Vector3(0, 0, -offset);
        //
        ConnectCamToDisplay(cam, display);
        //
        CreateTextureDisplay();
    }
    public void Update() {
        LoadRenderTextureToTexture(cam.targetTexture, textureDisplay);
        colorAve = GetAveColorTexture(textureDisplay);
        diffColor = GetColorDiffTarget(colorAve);
        delta = (diffColor - diffColorTarget) * speed;
        UpdateDeltaGo();
    }
    public void UpdateDeltaGo() {
        if (deltaGo == null) {
            deltaGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deltaGo.name = "delta " + description;
            deltaGo.transform.parent = camGo.transform;
            deltaGo.transform.eulerAngles = camGo.transform.eulerAngles;
            deltaGo.layer = LayerMask.NameToLayer(layerHidden);
        }
        float sca = (1 + diffColor);
//        Debug.Log(description + " diffColor:" + diffColor + "\n");
        deltaGo.transform.position = camGo.transform.position + camGo.transform.forward * sca/2;
        deltaGo.transform.localScale = new Vector3(.1f, .1f, sca);
        Color col = Color.white;
        IsIdle = false;
        if (Mathf.Abs(diffColor - diffColorTarget) < idleRange) {
            col = Color.red;
            IsIdle = true;
        }
        deltaGo.GetComponent<Renderer>().material.color = col;
    }
    public void CreateTextureDisplay()
    {
        textureDisplay = new Texture2D(camRes, camRes);
    }
    public Camera AddCameraToGo(GameObject go)
    {
        cam = go.AddComponent<Camera>();
        cam.targetTexture = new RenderTexture(camRes, camRes, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture.filterMode = FilterMode.Point;
        cam.nearClipPlane = .1f;
        cam.cullingMask &= ~(1 << LayerMask.NameToLayer(layerHidden));
        return cam;
    }
    public GameObject AddDisplayToGo(GameObject go, string txt)
    {
        GameObject quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.parent = go.transform;
        quad.name = txt;
        return quad;
    }
    public void ConnectCamToDisplay(Camera cam, GameObject display)
    {
        Renderer rend = display.GetComponent<Renderer>();
        rend.material.mainTexture = cam.targetTexture;
    }
    public void LoadRenderTextureToTexture(RenderTexture rTex, Texture2D tex)
    {
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, camRes, camRes), 0, 0);
        tex.Apply();
    }
    public Color GetAveColorTexture(Texture2D tex)
    {
        Color sumColor = Color.black;
        for (int nx = 0; nx < tex.width; nx++)
        {
            for (int ny = 0; ny < tex.height; ny++)
            {
                Color color = tex.GetPixel(nx, ny);
                sumColor += color;
            }
        }
        return sumColor / (tex.width * tex.height);
    }
    public float GetColorDiffTarget(Color color)
    {
        Vector3 colorV3 = new Vector3(color.r, color.g, color.b);
        Vector3 targetColorV3 = new Vector3(colorTarget.r, colorTarget.g, colorTarget.b);
        return Vector3.Distance(Vector3.Normalize(colorV3), Vector3.Normalize(targetColorV3));
    }
}
