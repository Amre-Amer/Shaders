using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueWave : MonoBehaviour {
    GameObject camGo;
    GameObject meshGo;
    RogueWaveMesh meshManager;
    Texture2D tex;
    //[Range(0, 100f)]
    //public float meshHeight = 10;
//    float meshHeightLast;
    [Range(.01f, 10f)]
    public float speed = .5f;  // frames
    [Range(0, 100f)]
    public float frequency1 = 10;  // frames
    [Range(0, 100f)]
    public float frequency2 = 20;  // frames
    [Range(0, 100f)]
    public float frequency3 = 30;  // frames
    [Range(0, 100f)]
    public float amplitude = 10;
    int resX = 100;
    int resY = 100;
    int cntFrames;
    int cntFrames2;
    GameObject texGo;
    Camera cam;
	// Use this for initialization
	void Start () {
        meshManager = new RogueWaveMesh();
        tex = Resources.Load<Texture2D>("Icon");
        meshGo = meshManager.CreateMeshGoFromTexture(tex);
        texGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        texGo.GetComponent<Renderer>().material.mainTexture = tex;
        texGo.transform.localScale = new Vector3(resX, resY, 1);
        texGo.transform.Rotate(90, 0, 0);
        texGo.transform.position = new Vector3(-resX, 0, -resY);
        //
        camGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        camGo.name = "cam";
        camGo.transform.position = new Vector3(resX / 2f, resX / 4f, resY / 2f);
        cam = camGo.AddComponent<Camera>();
        RenderTexture rt = new RenderTexture(resX, resY, 16, RenderTextureFormat.ARGB32);
        cam.targetTexture = rt;
	}
	
	// Update is called once per frame
	void Update () {
        float x = resX/2f * Mathf.Cos(cntFrames * Mathf.Deg2Rad);
        float z = resX/2f * Mathf.Sin(cntFrames * Mathf.Deg2Rad);
        float y = cam.transform.position.y; 
        camGo.transform.position = new Vector3(x, y, z); 
        camGo.transform.LookAt(Vector3.zero);

        UpdateTexture();
        //if (meshHeight != meshHeightLast)
        //{
        meshManager.meshHeight = amplitude;
        meshManager.UpdateMeshGoWithTexture(meshGo, tex);
//            meshHeightLast = meshHeight;
        //}
        cntFrames--;
        cntFrames2++;
	}

    void UpdateTexture() {
        if (tex == null)
        {
            tex = new Texture2D(resX, resY);
        }
        RenderTexture.active = cam.targetTexture;
        tex.ReadPixels(new Rect(0, 0, resX, resY), 0, 0);
        for (int nx = 0; nx < resX; nx++) {
            for (int ny = 0; ny < resY; ny++) {
                Color colCam = tex.GetPixel(nx, ny);                 
                tex.SetPixel(nx, ny, colCam);
            }
        }
//        RenderTexture.active = null;
        tex.Apply();
        texGo.GetComponent<Renderer>().material.mainTexture = tex;
    }
    float GetDistance2D(float x1, float y1, float x2, float y2) {
        Vector2 posA = new Vector2(x1, y1);
        Vector2 posB = new Vector2(x2, y2);
        return Vector2.Distance(posA, posB);
    }
}
