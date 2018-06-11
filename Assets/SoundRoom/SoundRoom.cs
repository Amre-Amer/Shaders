using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRoom : MonoBehaviour {
    public Material material;
    [Range(0f, 10f)]
    public float size = .2f;
    [Range(0f, 1f)]
    public float thickness = .2f;
    float[] amplitudes;
    int numCenters = 1;
    GameObject centerGo;
    AudioSource audioSource;
    float min = -1;
    float max = -1;
    int cntFrames;
    float amplitude;
    // Use this for initialization
	void Start () {
        centerGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        amplitudes = new float[numCenters];
	}
	
	// Update is called once per frame
	void Update () {
        UpdateAmplitude();
        //UpdateCenter();
        UpdateMaterial();
        cntFrames++;
	}
    void UpdateMaterial() {
        Vector3 pos = centerGo.transform.position;
        Vector4 pos4 = new Vector4(pos.x, pos.y, pos.z, 0);
        material.SetVector("_Center", pos4);
        material.SetFloat("_Size", size);
        material.SetFloat("_Thickness", thickness);
//        amplitude = Random.Range(0f, 1f);
        material.SetFloat("_Amplitude", amplitude);
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
        if (min == -1 || value < min)
        {
            min = value;
        }
        if (max == -1 || value > max)
        {
            max = value;
        }
        //float amplitude = 0;
        float range = max - min;
        if (range > 0)
        {
            amplitude = (value - min) / range;
        }
    }
    void UpdateCenter() {
        float rad = 3;
        float x = rad * Mathf.Cos(cntFrames * Mathf.Deg2Rad);
        float y = 0;
        float z = rad * Mathf.Sin(cntFrames * Mathf.Deg2Rad);
        centerGo.transform.position = new Vector3(x, y, z);
        float s = .25f + amplitude * 3;
        centerGo.transform.localScale = new Vector3(s, s, s);
    }
}
