using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRoom : MonoBehaviour {
    public bool ynSound = false;
    public Material material;
    [Range(0f, 10f)]
    public float size = .2f;
    [Range(0f, 1f)]
    public float thickness = .2f;
    [Range(0f, 3f)]
    public float speed = .1f;
    public GameObject centerGo;
    AudioSource audioSource;
    float min = -1;
    float max = -1;
    int cntFrames;
    float amplitude;
    Camera cam;
    RenderTexture renderTexture;
    //
	void Start () {
        if (centerGo == null)
        {
            centerGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        }
        //        centerGo.GetComponent<Renderer>().material.mainTexture = Resources.Load<Texture2D>("face_happy");
        GameObject face = GameObject.CreatePrimitive(PrimitiveType.Cube);
        face.transform.parent = centerGo.transform;
        face.transform.position = centerGo.transform.position + new Vector3(0, 0, .5f);
        face.transform.localScale = new Vector3(.25f, .25f, 1);
        //
        centerGo.transform.Rotate(0, 45, 0);
	}
	void Update () {
        UpdateCam();
        UpdateAmplitude();
        //UpdateCenter();
        UpdateMaterial();
        cntFrames++;
	}
    void UpdateCam() {
        if (cam == null)
        {
            cam = centerGo.AddComponent<Camera>();
            renderTexture = new RenderTexture(10, 10, 16, RenderTextureFormat.ARGB32);
            cam.targetTexture = renderTexture;
        }
        Color aveColor = GetAveColor();
        Vector3 aveColorV3 = new Vector3(aveColor.r, aveColor.g, aveColor.b);
        Color colorTarget = Color.yellow;
        Vector3 targetColorV3 = new Vector3(colorTarget.r, colorTarget.g, colorTarget.b);
        float distColor = Vector3.Distance(aveColorV3, targetColorV3);
        if (distColor < .75f) {
            aveColor = Color.red;
            float yawNew = Random.Range(160f, 200f);
            //yawNew = 180;
            centerGo.transform.Rotate(0, yawNew, 0);
        }
        centerGo.transform.position += centerGo.transform.forward * speed;
        centerGo.GetComponent<Renderer>().material.color = aveColor;
    }
    Color GetAveColor() {
        Color sumColor = Color.black;
        //        Texture2D texture = cam.GetComponent<Renderer>().material.mainTexture as Texture2D;
        RenderTexture.active = cam.targetTexture;
        Texture2D texture = new Texture2D(10, 10);
        texture.ReadPixels(new Rect(0, 0, 10, 10), 0, 0);
        for (int nx = 0; nx < texture.width; nx++) {
            for (int ny = 0; ny < texture.height; ny++)
            {
                Color color = texture.GetPixel(nx, ny);
                sumColor += color;
            }
        }
        return sumColor / (texture.width * texture.height);        
    }
    void UpdateMaterial() {
        Vector3 pos = centerGo.transform.position;
        Vector4 pos4 = new Vector4(pos.x, pos.y, pos.z, 0);
        Shader.SetGlobalVector("_Center", pos4);
        Shader.SetGlobalFloat("_Size", size);
        Shader.SetGlobalFloat("_Thickness", thickness);
        Shader.SetGlobalFloat("_Amplitude", amplitude);
        int yn = 0;
        if (ynSound == true) yn = 1;
        Shader.SetGlobalInt("_YnLines", yn);
        Shader.SetGlobalInt("_CntFrames", cntFrames);
        Shader.SetGlobalFloat("_Yaw", centerGo.transform.eulerAngles.y);
    }
    void UpdateAmplitude()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
            audioSource.loop = true;
            while (!(Microphone.GetPosition(null) > 0)) { }
            audioSource.Play();
        }
        float[] sample = new float[1];
        audioSource.GetOutputData(sample, 0);
        float value = sample[0];
        if (min == -1 || value < min) min = value;
        if (max == -1 || value > max) max = value;
        float range = max - min;
        if (range > 0)
        {
            amplitude = (value - min) / range;
            float s = 1f + amplitude * 10;
            centerGo.transform.localScale = new Vector3(s, s, s);
        }
    }
    void UpdateCenter() {
        centerGo.transform.position += centerGo.transform.forward * speed;
    }
}
