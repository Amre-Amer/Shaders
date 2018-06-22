using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explorer : MonoBehaviour {
    GameObject meshGo;
    Texture2D tex;
    ExplorerMesh meshManager;
    ExplorerRig[] rigs;
    int numRigs = 1;
    float rangeShader = 5;
    public Color colorTarget = Color.blue;

	// Use this for initialization
	void Start () {
        meshManager = new ExplorerMesh();
        tex = Resources.Load<Texture2D>("track_circle");
        meshGo = meshManager.CreateMeshGoFromTexture(tex);
        //
        rigs = new ExplorerRig[numRigs];
        rigs[0] = new ExplorerRig(this);
//        float y = meshManager.radius + meshManager.meshHeight * 5;
        rigs[0].goMain.transform.position = new Vector3(20, 10, 2);
	}
	
	// Update is called once per frame
	void Update () {
        UpdateRigs();
        UpdateShader();
	}

    void UpdateShader()
    {
        Vector4[] pos4s = new Vector4[numRigs];
        for (int n = 0; n < numRigs; n++)
        {
            Vector3 pos = rigs[n].goMain.transform.position;
            pos4s[n] = new Vector4(pos.x, pos.y, pos.z, 0);
        }

        Shader.SetGlobalVectorArray("_Centers", pos4s);
        Shader.SetGlobalInt("_NumCenters", numRigs);
        Shader.SetGlobalFloat("_Range", rangeShader);
        Vector4 col4 = new Vector4(colorTarget.r, colorTarget.g, colorTarget.b, colorTarget.a);
        Shader.SetGlobalVector("_ColorTarget", col4);
    }

    void UpdateRigs() {
        for (int n = 0; n < numRigs; n++) {
            rigs[n].Update();
        }
    }
}
