using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour {
	AudioSource aud;
	GameObject audGo;
	// Use this for initialization
	void Start () {
		aud = GetComponent<AudioSource>();
//		Parse();
	}
	
	// Update is called once per frame
	void Update () {
		GetSampleValue();
	}

	void GetSampleValue() {
		float[] sample = new float[1];
		aud.GetOutputData(sample, 0);		
		if (audGo == null) {
			audGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
		}
		float value = sample[0];
		float y = value * 100;
		audGo.transform.position = new Vector3(-1, y / 2, 0);
        audGo.transform.localScale = new Vector3(.5f, y, .5f);
	}

	void Parse() {
        float[] samples = new float[aud.clip.samples * aud.clip.channels];
        aud.clip.GetData(samples, 0);
		int numSamples = 1000;
		int span = samples.Length / numSamples;
		Debug.Log("samples:" + samples.Length + " span:" + span + " freq:" + aud.clip.frequency + " length:" + aud.clip.length + " sec\n");
		float min = -1;
		float max = -1;
		int iMin = -1;
		int iMax = -1;
		int cnt = 0;
		GameObject[] gos = new GameObject[numSamples + 1];
		for (int i = 0; i < samples.Length; i += span)
		{
			float value = samples[i];
			if (i == 0 || value < min) {
				min = value;
				iMin = i;
			}
			if (i == 0 || value > max)
            {
                max = value;
                iMax = i;
            }
			cnt++;
		}
		cnt = 0;
		float range = max - min;
		for (int i = 0; i < samples.Length; i += span)
		{
			float value = samples[i];
			float frac = (value - min) / range;
			float scale = 100;
            if (gos[cnt] == null)
            {
                gos[cnt] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            }
			float h = frac * scale;
			gos[cnt].transform.position = new Vector3(cnt, h / 2, 0);
            gos[cnt].transform.localScale = new Vector3(.5f, h, .5f);
			cnt++;
		}
		Debug.Log("min:" + min.ToString("F2") + " max:" + max.ToString("F2") + "\n");
	}
}
