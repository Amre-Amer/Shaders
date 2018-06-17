using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPhysicsData {
    public ShaderPhysicsObject[] objects;
    public float frequencyYnReset = 15;
    public string layerHidden = "HiddenFromCam";
    public int numObjects = 3;
    public float colorThreshold = .68f; //.85f;  // .95
    public float speed = .2f;
    public float smooth = .95f;
    public Texture2D textureTrack;
    public int cntErrors;
    public Color colorTarget = Color.blue;
    public float rangeShader = 5;
    public int resCam = 10; //10;
    public float grayScaleThreshold = .5f;
    public float meshHeight = 5;
    public int numBars; // = resMeshX
    public int resMeshX; // = image width
    public int resMeshY; // = image height
    public bool ynReset = false;
    public float objectScale = 5f;
    public string meshShaderName = "Custom/ShaderPhysicsTransparent";
    public string meshTextureName = "grid_opaque";
    public int nTracked = 0;
    public Text textPercentComplete;
    public int cntFrames;
    //
    public GameObject meshGo;
    public GameObject parentTargetsGo;
    public GameObject parentObjectsGo;

    public ShaderPhysicsData() {
        
    }
}
