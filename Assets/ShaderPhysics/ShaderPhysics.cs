using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPhysics : MonoBehaviour {
    //public float frequencyYnReset = 15; 
    //public string layerHidden = "HiddenFromCam";
    //public int numObjects = 3;
//    [Range(0f, 1f)]
    //public float colorThreshold = .68f; //.85f;  // .95
//    [Range(0f, 1f)]
    //public float speed = .2f;
//    [Range(0f, 1f)]
//    public float smooth = .95f;
    //public Texture2D textureTrack;
    //public int cntErrors;
    //public Color colorTarget = Color.blue;
    //public float rangeShader = 5;
    //public int resCam = 10; //10;
    //public float grayScaleThreshold = .5f;
    //public float meshHeight = 5;
    //public int numBars; // = resMeshX
    //public int resMeshX; // = image width
    //public int resMeshY; // = image height
    //
    //public List<ShaderPhysicsObject> objects;
    //public bool ynReset = false;
    //public float objectScale = 5f;
    //public string meshShaderName = "Custom/ShaderPhysicsTransparent";
    //public string meshTextureName = "grid_opaque";
    //public int nTracked = 0;
    //public Text textPercentComplete;
    //public int cntFrames;
    ////
    //public GameObject meshGo;
    //public GameObject parentTargetsGo;
    //public GameObject parentObjectsGo;
    //
    public ShaderPhysicsBars barsManager;
    public ShaderPhysicsMesh meshManager;
    public ShaderPhysicsMisc miscManager;
    public ShaderPhysicsTrails trailsManager;
    public ShaderPhysicsData dataManager;

    // Use this for initialization
	void Start () {
        dataManager = new ShaderPhysicsData();
        miscManager = new ShaderPhysicsMisc(this);
        miscManager.PlaySound(this.gameObject);
        barsManager = new ShaderPhysicsBars(this);
        trailsManager = new ShaderPhysicsTrails(this);

        //UpdateTextureTrack();
        //ShaderPhysicsObject obj = new ShaderPhysicsObject(this);
        //Debug.Log(obj.go.name + "\n");

        InvokeRepeating("Scramble", dataManager.frequencyYnReset, dataManager.frequencyYnReset);
	}
	
	// Update is called once per frame
	void Update () {
        UpdateTextureTrack();
        UpdateMesh();
        UpdateObjects();
        UpdateShader();
        barsManager.UpdateBars();
        trailsManager.UpdateTrails();
        miscManager.UpdateFar();
        UpdatePercentComplete();
        UpdateYnSensorLast();
        UpdateYnReset();
        dataManager.cntFrames++;
	}

    void UpdateObjects()
    {
        if (dataManager.objects == null)
        {
            dataManager.objects = new ShaderPhysicsObject[dataManager.numObjects];
            for (int n = 0; n < dataManager.numObjects; n++)
            {
                dataManager.objects[n] = new ShaderPhysicsObject(n, this);
            }
            dataManager.parentObjectsGo.name = "objects:" + dataManager.numObjects;
        }
        for (int n = 0; n < dataManager.objects.Length; n++)
        {
            dataManager.objects[n].UpdateSensorValue();
            dataManager.objects[n].UpdateDisplay();
            dataManager.objects[n].UpdatePointer();
            dataManager.objects[n].UpdateTarget();
            dataManager.objects[n].NavigateObject();
        }
    }

    void UpdateShader()
    {
        Vector4[] pos4s = new Vector4[dataManager.objects.Length];
        for (int n = 0; n < dataManager.objects.Length; n++)
        {
            Vector3 pos = dataManager.objects[n].go.transform.position;
            pos4s[n] = new Vector4(pos.x, pos.y, pos.z, 0);
        }

        Shader.SetGlobalVectorArray("_Centers", pos4s);
        Shader.SetGlobalInt("_NumCenters", dataManager.objects.Length);
        Shader.SetGlobalFloat("_Range", dataManager.rangeShader);
        Vector4 col4 = new Vector4(dataManager.colorTarget.r, dataManager.colorTarget.g, dataManager.colorTarget.b, dataManager.colorTarget.a);
        Shader.SetGlobalVector("_ColorTarget", col4);
    }

    void UpdateTextureTrack() {
        if (dataManager.textureTrack == null)
        {
            dataManager.textureTrack = Resources.Load<Texture2D>("track_circle");
        }
    }

    void Scramble() {
        dataManager.ynReset = true;
    }

    void UpdateYnSensorLast() {
        for (int n = 0; n < dataManager.objects.Length; n++) {
            dataManager.objects[n].ynSensorLast = dataManager.objects[n].ynSensor;
        }
    }

    void UpdatePercentComplete() {
        if (dataManager.textPercentComplete == null) {
            dataManager.textPercentComplete = miscManager.CreateText(Vector3.zero, "%", Color.blue);
            dataManager.textPercentComplete.transform.position = barsManager.posBar + new Vector3(dataManager.resMeshX / 5, dataManager.resMeshX / 5, -1);
        }
        float prog = 100 * GetProgress();
        dataManager.textPercentComplete.text = prog.ToString("F0") + " %";
    }
    float GetProgress()
    {
        int cnt = 0;
        int cntAll = 0;
        for (int nx = 0; nx < trailsManager.textureTrails.width; nx++) {
            for (int ny = 0; ny < trailsManager.textureTrails.height; ny++)
            {
                if (dataManager.textureTrack.GetPixel(nx, ny) == Color.black)
                {
                    cntAll++;
                    Color col = trailsManager.textureTrails.GetPixel(nx, ny);
                    if (col != Color.black) {
                        cnt++;
                    }
                }
            }
        }
        return cnt / (float)cntAll;
    }

    void UpdateYnReset()
    {
        if (dataManager.ynReset == true)
        {
            for (int n = 0; n < dataManager.objects.Length; n++)
            {
                dataManager.objects[n].go.transform.position = miscManager.GetRandomObjectPos();
            }
            dataManager.cntErrors = 0;
            dataManager.ynReset = false;
        }
    }

    public bool AvoidNearestObject(int nCheck, float clearance) {
        bool yn = false;
        for (int n = 0; n < dataManager.objects.Length; n++) {
            if (n != nCheck) {
                float dist = Vector3.Distance(dataManager.objects[n].go.transform.position, dataManager.objects[nCheck].go.transform.position);            
                if (dist < clearance) {
                    Vector3 pos = dataManager.objects[n].go.transform.position;
                    Vector3 vector = dataManager.objects[nCheck].go.transform.position - pos;
                    dataManager.objects[nCheck].target = dataManager.objects[nCheck].go.transform.position + Vector3.Normalize(vector) * clearance;
                    yn = true;
                    break;
                }
            }
        }
        return yn;
    }

    public void UpdateMesh()
    {
        if (meshManager == null)
        {
            dataManager.resMeshX = dataManager.textureTrack.width;
            dataManager.resMeshY = dataManager.textureTrack.height;

            meshManager = new ShaderPhysicsMesh(this);
            dataManager.meshGo = meshManager.CreateMeshGoFromTexture(dataManager.textureTrack);
        }
    }
}
