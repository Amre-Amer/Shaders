using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoMM : MonoBehaviour {
    [Range(.1f, 10f)]
    public float scale = 1;
    int numNodes = 10;
    GameObject[] videoGos;
    VideoClip[] videoClips;
    VideoPlayer[] videoPlayers;
    AudioSource[] audioSources;
    Vector3[] looksPos;
    Vector3[] targetsPos;
    Vector3[] origsPos;
    Vector3[] targetsScale;
    Vector3[] origsScale;
    Vector3[] looksScale;
    float smooth = .95f;
    float smoothScale = .25f;
    float speed = 1f;
    float tolerance = 1f;
    int nPicked;
    GameObject displayGo;
    VideoPlayer videoPlayer;
    Vector3 lookScaleDisplay;
    Vector3 targetScaleDisplay;
    Vector3 lookPosDisplay;
    Vector3 targetPosDisplay;
    float delay = 1;
    float startTime;
    public bool ynStep = true;

	// Use this for initialization
	void Start () {
        displayGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
        displayGo.name = "display";
        videoPlayer = displayGo.AddComponent<VideoPlayer>();
        displayGo.transform.localScale = new Vector3(0, 0, 0);
        //
        videoGos = new GameObject[numNodes];
        videoClips = new VideoClip[numNodes];
        videoPlayers = new VideoPlayer[numNodes];
        targetsPos = new Vector3[numNodes];
        origsPos = new Vector3[numNodes];
        looksPos = new Vector3[numNodes];
        targetsScale = new Vector3[numNodes];
        origsScale = new Vector3[numNodes];
        looksScale = new Vector3[numNodes];
        audioSources = new AudioSource[numNodes];
        for (int n = 0; n < numNodes; n++)
        {
            videoGos[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            videoGos[n].name = "video " + n;
            float ang = 360f / numNodes * n;
            float rad = 5 * scale;
            float x = rad * Mathf.Cos(ang * Mathf.Deg2Rad);
            float y = rad * Mathf.Sin(ang * Mathf.Deg2Rad);
            float z = n * 2 * scale;
            Vector3 pos = new Vector3(x, y, z);
            videoGos[n].transform.position = Vector3.zero;
            Vector3 sca = new Vector3(2 * scale, 2 * scale, 1 * scale);
            videoGos[n].transform.localScale = sca;
            videoPlayers[n] = videoGos[n].AddComponent<VideoPlayer>();
            videoPlayers[n].clip = videoClips[n];
            videoPlayers[n].isLooping = true;
            //
            audioSources[n] = videoGos[n].AddComponent<AudioSource>();
            videoPlayers[n].audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayers[n].EnableAudioTrack(0, true);
            videoPlayers[n].SetTargetAudioSource(0, audioSources[n]);
            if (n == 0) {
                videoPlayers[n].url = Application.streamingAssetsPath + "/" + "SampleVideo_640x360_5mb.mp4";
            } else {
                if (n == 1) {
                    videoPlayers[n].url = Application.streamingAssetsPath + "/" + "small.mp4";
                } else {
                    videoPlayers[n].url = Application.streamingAssetsPath + "/" + "CountDown" + n + ".mov";
                }
            }
            videoPlayers[n].Prepare();
            audioSources[n].mute = true;
            targetsPos[n] = pos;
            origsPos[n] = pos;
            targetsScale[n] = sca;
            origsScale[n] = sca;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (ynStep == false || Time.realtimeSinceStartup - startTime > delay)
        {
            startTime = Time.realtimeSinceStartup;
            UpdateNodes();
        }
        UpdatePress();
	}

    void PickVideo(int n)
    {
        RestoreTargets();
        //Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 3;
        Vector3 posTarget = GetDisplayTargetPosition();
        Vector3 posSource = videoPlayers[n].transform.position;
        targetsPos[n] = posTarget;
        targetsScale[n] = new Vector3(.01f, .01f, .01f);
        //
        displayGo.transform.position = posSource;
        displayGo.transform.eulerAngles = Camera.main.transform.eulerAngles;
        //
        displayGo.transform.localScale = new Vector3(.01f, .01f, .01f);
        lookScaleDisplay = displayGo.transform.localScale;
        targetScaleDisplay = GetDisplayTargetScale();
        //
        lookPosDisplay = posSource;
        targetPosDisplay = posTarget;
        //
        videoPlayer.url = videoPlayers[n].url;
        videoPlayer.Play();
    }

    Vector3 GetDisplayTargetScale() {
        Vector3 posLL = Camera.main.ScreenToWorldPoint(new Vector3(0, 0, Camera.main.nearClipPlane));
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = new Vector3(.1f, .1f, .1f);
        go.transform.position = posLL;
        Vector3 posLR = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Camera.main.nearClipPlane));
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = new Vector3(.1f, .1f, .1f);
        go.transform.position = posLR;
        Vector3 posUL = Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height, Camera.main.nearClipPlane));
        go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = new Vector3(.1f, .1f, .1f);
        go.transform.position = posUL;
        float sx = Vector3.Distance(posLL, posLR);
        float sy = Vector3.Distance(posLL, posUL);
        return new Vector3(sx, sy, 1);
    }

    Vector3 GetDisplayTargetPosition() {
        Vector3 posCenter = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane));
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.localScale = new Vector3(.1f, .1f, .1f);
        go.transform.position = posCenter;
        return posCenter;
    }

    void UpdateNodes() {
        for (int n = 0; n < numNodes; n++) {
            UpdateNode(n);            
        }
        lookScaleDisplay = smoothScale * targetScaleDisplay + (1 - smoothScale) * lookScaleDisplay;
        displayGo.transform.localScale = lookScaleDisplay;
        //displayGo.transform.position = videoPlayers[nPicked].transform.position;
        //
        lookPosDisplay = smooth * targetPosDisplay + (1 - smooth) * lookPosDisplay;
        displayGo.transform.position = lookPosDisplay;
    }

    void UpdateNode(int n) {
        float dist = Vector3.Distance(videoGos[n].transform.position, targetsPos[n]);
        if (dist > tolerance)
        {
            looksPos[n] = smooth * targetsPos[n] + (1 - smooth) * looksPos[n];
            videoGos[n].transform.LookAt(looksPos[n]);
            //
            looksScale[n] = smooth * targetsScale[n] + (1 - smooth) * looksScale[n];
            videoGos[n].transform.position += videoGos[n].transform.forward * speed;
            //
        }
        videoGos[n].transform.localScale = looksScale[n];
        //
    }

    void PickVideoRandom() {
        int n = Random.Range(0, numNodes);
        PickVideo(n);
        Debug.Log(n + "\n");
    }

    void RestoreTargets() {
        for (int n = 0; n < numNodes; n++) {
            targetsPos[n] = origsPos[n];
            targetsScale[n] = origsScale[n];
            audioSources[n].mute = true;
        }
        videoPlayer.Stop();
        targetScaleDisplay = new Vector3(0, 0, 0);
    }

    void UpdatePress() {
        if (Input.GetMouseButtonDown(0) == true) {
            nPicked = -1;
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 100.0f))
            {
                string nam = hit.transform.name;
                if (nam == "display") {
                    RestoreTargets();                    
                } else {
                    Debug.Log("You selected the " + nam + "\n"); // ensure you picked right object
                    string txt = nam.Substring(nam.Length - 1, 1);
                    int n = int.Parse(txt);
                    nPicked = n;
                    PickVideo(n);
                }
            }
            if (nPicked == -1) {
                RestoreTargets();
            }
        }
    }
}
