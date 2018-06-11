using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveTracker : MonoBehaviour {
	int numGos = 1;
	[Range(0f, .1f)]
    public float speed = .05f;
	[Range(0f, 1f)]
    public float thickness = .15f;
	[Range(0f, 1f)]
	public float size = .5f;
	[Space]
	[Header("Ave Distance Between")]
    [Range(0, 8)]
    public float rangeRed = 2;
    [Range(0, 8)]
    public float rangeGreen = 3;
    [Range(0, 8)]
    public float rangeBlue = 5;
	GameObject[] gos;
	GameObject goFloor;
	Material material;
	Vector3[] targets;
	Vector3[] looks;
	float smooth = .95f;
	float distNear = 2;
	GameObject[] targetGos;
	GameObject[] lookGos;
	int numFLoorClones = 100;
	float spacingFloors = .1f;
	int cntFrames;
	AudioSource audioSource;
	float min = -1;
	float max = -1;
	// Use this for initialization
	void Start () {
		CreateFloor();
		CreateGos();
		//CreateFloorClones();
		Debug.Log("gos:" + gos.Length + "\n");
	}
	
	// Update is called once per frame
	void Update () {
		UpdateAudioSource();
		UpdateTargets();
		UpdateAvoids();
		UpdateTargetGos();
		UpdateLookGos();
		UpdateGos();
		UpdateShader();
		cntFrames++;
	}

	void UpdateAudioSource() {
		bool ynMicrophone = true;
		if (audioSource == null)
        {
			audioSource = gameObject.AddComponent<AudioSource>();
			if (ynMicrophone == true) {
				StartMicrophone();          
			} else {
				LoadClip();
			}
        }
	}

	void StartMicrophone() {
        audioSource.clip = Microphone.Start("Built-in Microphone", true, 10, 44100);
		audioSource.loop = true;
		while(!(Microphone.GetPosition(null) > 0)) {}
        audioSource.Play();
	}

	void LoadClip() {
        audioSource.clip = Resources.Load<AudioClip>("Sound");
        audioSource.Play();
	}

	void CreateGos() {
		gos = new GameObject[numGos];
		for (int n = 0; n < numGos; n++) {
			gos[n] = GameObject.CreatePrimitive(PrimitiveType.Cube);
			gos[n].transform.position = GetRandomPos();
		}        		
	}
	void CreateFloor() {
		goFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
		goFloor.transform.eulerAngles = new Vector3(90, 0, 0);
		goFloor.transform.localScale = new Vector3(10, 10, 10);
		goFloor.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/WaveTracker"));
	}
	float GetAmplitude() {
		float[] sample = new float[1];
		audioSource.GetOutputData(sample, 0);
		float value = sample[0];
		if (min == -1 || value < min) {
			min = value;
		}
		if (max == -1 || value > max)
        {
            max = value;
        }
		float range = max - min;
		float amplitude = (value - min) / range;
		return amplitude;
	}
	void UpdateShader()
    {
		Vector4[] gos4 = new Vector4[numGos];
		float[] amplitudes = new float[numGos];
		material = goFloor.GetComponent<Renderer>().material;
		for (int n = 0; n < numGos; n++)
		{
			Vector3 p = gos[n].transform.position;
			gos4[n] = new Vector4(p.x, p.y, p.z, 0);
			if (n == 0) {
				amplitudes[n] = GetAmplitude();
			} else {
				amplitudes[n] = 0; //Mathf.Sin(cntFrames * Mathf.Deg2Rad) * .5f + .5f;
			}
		}
		material.SetVectorArray("_Centers", gos4);
		material.SetInt("_NumCenters", numGos);
		material.SetFloat("_Size", size);
		material.SetFloat("_RangeRed", rangeRed);
		material.SetFloat("_RangeGreen", rangeGreen);
		material.SetFloat("_RangeBlue", rangeBlue);
		//material.SetFloat("_Sound", sound);
		material.SetFloat("_Thickness", thickness);
		material.SetFloatArray("_Amplitudes", amplitudes);
    }
	void CreateFloorClones() {
		for (int n = 1; n < numFLoorClones / 2; n++)
        {
			float x = goFloor.transform.position.x;
			float yOffset = (float) n * spacingFloors;
			float y = goFloor.transform.position.y;
			float z = goFloor.transform.position.z;
            //
			GameObject go1 = Instantiate(goFloor);
			go1.transform.position = new Vector3(x, y + yOffset, z);
			//
            GameObject go2 = Instantiate(goFloor);
            go2.transform.position = new Vector3(x, y - yOffset, z);
        }
	}
	void UpdateTargetGos() {
		if (targetGos == null)
        {
			targetGos = new GameObject[numGos];
        }
		for (int n = 0; n < numGos; n++)
        {
			if (targetGos[n] == null) {
				targetGos[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				targetGos[n].transform.localScale = new Vector3(.25f, .25f, .25f);
				targetGos[n].GetComponent<Renderer>().material.color = Color.blue;
			}
			targetGos[n].transform.position = targets[n];
        }
	}
	void UpdateLookGos() {
		if (lookGos == null)
        {
			lookGos = new GameObject[numGos];
        }
		for (int n = 0; n < numGos; n++)
        {
            if (lookGos[n] == null)
            {
                lookGos[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                lookGos[n].transform.localScale = new Vector3(.25f, .25f, .25f);
				lookGos[n].GetComponent<Renderer>().material.color = Color.cyan;
            }
			lookGos[n].transform.position = looks[n];
        }
	}
	void UpdateGos() {
		for (int n = 0; n < numGos; n++)
		{
			looks[n] = smooth * looks[n] + (1 - smooth) * targets[n];
			GameObject go = gos[n];
			go.transform.LookAt(looks[n]);
			go.transform.eulerAngles = new Vector3(0, go.transform.eulerAngles.y, 0);
			go.transform.position += go.transform.forward * speed;
			go.transform.position = new Vector3(go.transform.position.x, 0, go.transform.position.z);
		}
	}
	void UpdateTargets() {
		if (targets == null) {
			targets = new Vector3[numGos];
			looks = new Vector3[numGos];
		}
		for (int n = 0; n < numGos; n++) {
			float dist = Vector3.Distance(gos[n].transform.position, targets[n]);
			if (dist < distNear) {
				targets[n] = GetRandomPos();
			}
		}
	}
	void UpdateAvoids() {
		for (int n = 0; n < numGos; n++)
		{
			CheckAvoid(n);
		}
	}
	void CheckAvoid(int n) {
		int nNearest = GetNearest(n);
		if (nNearest >= 0)
		{
			float dist = Vector3.Distance(gos[n].transform.position, gos[nNearest].transform.position);
			if (dist < distNear)
			{
				Vector3 vector = gos[n].transform.position - gos[nNearest].transform.position;
				targets[n] = gos[n].transform.position + vector * distNear * 1;
			}
		}
	}
	int GetNearest(int nCheck) {
		int nNearest = -1;
		float distNearest = -1;
		for (int n = 0; n < numGos; n++) {
			if (n != nCheck) {
				float dist = Vector3.Distance(gos[nCheck].transform.position, gos[n].transform.position);
				if (nNearest == -1 || dist < distNearest) {
					distNearest = dist;
					nNearest = n;
				}
			}
		}
		return nNearest;
	}
	Vector3 GetRandomPos() {
		float sizeX = goFloor.transform.localScale.x / 2;
        float sizeY = goFloor.transform.localScale.y / 2;
        float sizeZ = goFloor.transform.localScale.z / 2;
		float x = goFloor.transform.position.x + Random.Range(-sizeX, sizeX);
        float y = .5f;
        float z = goFloor.transform.position.z + Random.Range(-sizeZ, sizeZ);
		return new Vector3(x, y, z);
	}
}
