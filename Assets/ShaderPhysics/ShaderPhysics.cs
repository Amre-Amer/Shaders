using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPhysics : MonoBehaviour {
    public string layerHidden = "HiddenFromCam";
    public int numObjects = 3;
    public bool ynReset;
    [Range(0f, 1f)]
    public float colorThreshold = .1f; //.85f;  // .95
    [Range(0f, 1f)]
    public float speed = .2f;
    [Range(0f, 1f)]
    public float smooth = .95f;
    public bool ynTargets = false;
    public bool ynTrails = true;
    public bool ynStep;
    public Texture2D textureTrack;
    public int cntErrors;
    public bool ynSound = true;
    public Color colorTarget = Color.blue;
    float delay = 1;
    float startTime;
    float rangeShader = 5;
    public int resCam = 10; //10;
    float grayScaleThreshold = .5f;
    float meshHeight = 5;
    int numBars; // = resMeshX
    float barScale = 20;
    GameObject meshGo;
    int resMeshX; // = image width
    int resMeshY; // = image height
    //
    [HideInInspector]
    public float objectScale = 5f;
    public List<ShaderPhysicsObject> objects;
    string meshShaderName = "Custom/ShaderPhysicsTransparent";
    string meshTextureName = "grid_opaque";
    int nTracked = 0;
    public List<ShaderPhysicsBar> bars;
    GameObject barThreshold;
    GameObject barTop;
    AudioSource audioSource;
    Texture2D textureTrails;
    GameObject trailsGo;
    public GameObject parentObjectsGo;
    public GameObject parentBarsGo;
    Text textTimer;
    int cntFrames;
    public Vector3 posBar;
    public GameObject parentTargetsGo;

    // Use this for initialization
	void Start () {
        CreateMesh();
        posBar = new Vector3(0, meshHeight, resMeshY);
        CreateObjects();
	}
	
	// Update is called once per frame
	void Update () {
        float delay0 = delay;
        float delayExtraSensor = 4;
        //if (ynSensor0 == true) delay0 *= delayExtraSensor;
        if (ynStep == true && Time.realtimeSinceStartup - startTime < delay0) {
            return;            
        }
        startTime = Time.realtimeSinceStartup;
        UpdateMaterials();
        UpdateObjects();
        UpdateCam();
        UpdateBars();
        UpdateTrails();
        UpdateTimer();
        UpdateReset();
        //UpdateFar();
        UpdateYnSensorLast();
        cntFrames++;
	}

    void UpdateYnSensorLast() {
        for (int n = 0; n < numObjects; n++) {
            objects[n].ynSensorLast = objects[n].ynSensor;
        }
    }

    void UpdateReset() {
        if (ynReset == true) {
            for (int n = 0; n < numObjects; n++) {
                objects[n].go.transform.position = GetObjectPos();
            }
            cntErrors = 0;
            ynReset = false;
        }
    }
    void UpdateTimer() {
        if (textTimer == null) {
            textTimer = CreateText(Vector3.zero, "test", Color.blue);
            textTimer.transform.position = posBar + new Vector3(resMeshX / 5, resMeshX / 5, -1);
        }
        float prog = 100 * GetProgress();
        textTimer.text = prog.ToString("F0") + " %";
    }
    float GetProgress()
    {
        int cnt = 0;
        int cntAll = 0;
        for (int nx = 0; nx < textureTrails.width; nx++) {
            for (int ny = 0; ny < textureTrails.height; ny++)
            {
                if (textureTrack.GetPixel(nx, ny) == Color.black)
                {
                    cntAll++;
                    Color col = textureTrails.GetPixel(nx, ny);
                    if (col != Color.black) {
                        cnt++;
                    }
                }
            }
        }
        return cnt / (float)cntAll;
    }
    void UpdateTrails() {
        if (textureTrails == null) {
            trailsGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            trailsGo.name = "trailsGo";
            trailsGo.transform.position = new Vector3(resMeshX / 2, .1f, -resMeshY / 2);
            trailsGo.transform.Rotate(90, 0, 0);
                //
            trailsGo.transform.localScale = new Vector3(resMeshX, resMeshY, 0);
            textureTrails = new Texture2D(resMeshX, resMeshY);
            textureTrails.filterMode = FilterMode.Point;
            MakeTextureGrid(textureTrails);
            Material mat = trailsGo.GetComponent<Renderer>().material;
            mat.mainTexture = textureTrails;
        }
        Vector3 pos = trailsGo.transform.position;
        float y = .1f;
        if (ynTrails == false)
        {
            if (pos.y != -y)
            {
                pos.y = -y;
                trailsGo.transform.position = pos;
            }
        }
        else
        {
            if (pos.y != y)
            {
                pos.y = y;
                trailsGo.transform.position = pos;
            }
        }
        for (int n = 0; n < numObjects; n++)
        {
            UpdateTrail(n);
        }
    }

    void UpdateTrail(int n)
    {
        Vector3 pos = objects[n].go.transform.position;
        Color color = new Color(1, 0, 0);
        int size = 1;
        for (int nx = 0; nx < size; nx++) {
            for (int ny = 0; ny < size; ny++)
            {
                int x = (int)pos.x + nx;
                int y = (int)pos.z + ny;
                Color colorUnder = textureTrails.GetPixel(x, y);
                textureTrails.SetPixel(x, y, (color + colorUnder) / 2);
            }
        }
        textureTrails.Apply();
    }

    Text CreateText(Vector3 pos, string txt, Color color)
    {
        GameObject go = new GameObject("text");
        go.name = txt;
        go.transform.SetParent(GameObject.Find("Canvas").transform);
        go.transform.eulerAngles = Vector3.zero;
        go.transform.position = pos;
        go.transform.localScale = new Vector3(.4f, .4f, .4f);
        RectTransform rect = go.GetComponent<RectTransform>();
        Font font = (Font)Resources.GetBuiltinResource(typeof(Font), "Arial.ttf");
        Text text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = 20;
        text.name = "." + txt + ".";
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = txt;
        return text;
    }

    void MakeMaterialTransparent(Material mat) {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }
    void MakeTextureGrid(Texture2D tex)
    {
        for (int nx = 0; nx < tex.width; nx++)
        {
            for (int ny = 0; ny < tex.height; ny++)
            {
                Color color = new Color(0, 0, 0);
                tex.SetPixel(nx, ny, color);
            }
        }
        tex.Apply();
    }
    void MakeTextureClear(Texture2D tex) {
        Color[] colors = new Color[tex.width * tex.height];
        for (int n = 0; n < colors.Length; n++)
        {
            colors[n] = new Color(0, 0, 0, 0);
        }
        tex.SetPixels(colors);
        tex.Apply();
    }

    void UpdateFar() {
        for (int n = 0; n < numObjects; n++) {
            int nx = (int)objects[n].go.transform.position.x;
            int ny = (int)objects[n].go.transform.position.z;
            if (textureTrack.GetPixel(nx, ny) != Color.black) {
                cntErrors++;
                objects[n].go.transform.position = GetObjectPos();
                if (ynSound == true)
                {
                    PlaySound();
                }
            }
        }
    }
    void PlaySound() {
        if (audioSource == null) {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = Resources.Load<AudioClip>("Sound");
            audioSource.Play();
        } else {
            if (audioSource.isPlaying == false) {
                audioSource.Play();
            }
        }
    }
    void UpdateBars() {
        if (bars == null) {
            numBars = resMeshX;
            parentBarsGo = new GameObject("bars");
            bars = new List<ShaderPhysicsBar>();
            for (int n = 0; n < numBars; n++)
            {
                bars[n] = new ShaderPhysicsBar(this);
            }
        }
        for (int n = numBars - 1; n > 0; n--) {
            float yPrev = bars[n - 1].go.transform.localScale.y;
            Color colorPrev = bars[n - 1].go.GetComponent<Renderer>().material.color;
            UpdateBar(n, yPrev, colorPrev);
        }
        Color color = Color.white;
        float sumNew = 0;
        for (int n = 0; n < numObjects; n++)
        {
            if (objects[n].ynSensor == true)
            {
                color = Color.red;
            }
            if (objects[n].ynSensorLast != objects[n].ynSensor) {
                color = Color.cyan;
            }
            sumNew += objects[n].diffColor;
        }
        float diffColorNew = sumNew / numObjects;
        // new bar value 
        float yNew = diffColorNew * barScale;
        UpdateBar(0, yNew, color);
        UpdateBarThreshold();
        UpdateBarTop();
    }

    void UpdateBarTop()
    {
        if (barTop == null)
        {
            barTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barTop.name = "barTop";
            barTop.transform.parent = parentBarsGo.transform;
            barTop.GetComponent<Renderer>().material.color = Color.blue;
            float y = 1 * barScale;
            barTop.transform.position = posBar + new Vector3(numBars / 2, y, 0);
            barTop.transform.localScale = new Vector3(numBars, .25f, 1);
        }
    }
    void UpdateBarThreshold() {
        if (barThreshold == null) {
            barThreshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barThreshold.name = "barThreshold";
            barThreshold.transform.parent = parentBarsGo.transform;
            barThreshold.GetComponent<Renderer>().material.color = Color.green;
        }
        float y = colorThreshold * barScale;
        barThreshold.transform.position = posBar + new Vector3(numBars / 2, y, 0);
        barThreshold.transform.localScale = new Vector3(numBars, .25f, 1);
    }
    void UpdateBar(int n, float y, Color color) {
        Vector3 sca = bars[n].go.transform.localScale;
        bars[n].go.transform.localScale = new Vector3(sca.x, y, sca.z);
        Vector3 pos = bars[n].go.transform.position;
        bars[n].go.transform.position = new Vector3(pos.x, meshHeight + y / 2, pos.z);
        bars[n].go.GetComponent<Renderer>().material.color = color;
    }

    void UpdateCam() {
        Camera cam = Camera.main;
        GameObject go = objects[nTracked].go;
        cam.transform.position = go.transform.position + Vector3.up * 10 + Vector3.forward * -3;
        cam.transform.LookAt(go.transform.position);
    }

    void UpdateMaterials()
    {
        Vector4[] pos4s = new Vector4[numObjects];
        for (int n = 0; n < numObjects; n++) {
            Vector3 pos = objects[n].go.transform.position;
            pos4s[n] = new Vector4(pos.x, pos.y, pos.z, 0);
        }
        Shader.SetGlobalVectorArray("_Centers", pos4s);
        Shader.SetGlobalInt("_NumCenters", numObjects);
        Shader.SetGlobalFloat("_Range", rangeShader);
        Vector4 col4 = new Vector4(colorTarget.r, colorTarget.g, colorTarget.b, colorTarget.a);
        Shader.SetGlobalVector("_ColorTarget", col4);
    }
    public void CreateObjects() {
        objects = new List<ShaderPhysicsObject>();
        for (int n = 0; n < numObjects; n++)
        {
            objects[n] = new ShaderPhysicsObject(n, this);
        }
    }
    public Vector3 GetObjectPos() {
        Vector3 pos = Vector3.zero;
        bool ynGood = true;
        int cnt = 0;
        int numTries = 150;
        for (int n = 0; n < numTries; n++) // 10 tries
        {
            cnt++;
            int r = 4;
            int rx = Random.Range(r, resMeshX - r);
            int ry = Random.Range(r, resMeshY - r);
            ynGood = true;
            for (int nx = -r; nx <= r; nx++) {
                for (int ny = -r; ny <= r; ny++)
                {
                    Color color = textureTrack.GetPixel(rx + nx,ry + ny);
                    if (color.grayscale > grayScaleThreshold)
                    {
                        ynGood = false;
                        break;
                    }
                }
                if (ynGood == false) {
                    break;
                }
            }
            if (ynGood == true) {
                float y = meshHeight / 2;
                pos = new Vector3(rx, y, ry);
                textureTrack.SetPixel(rx, ry, Color.white);
                textureTrack.Apply();
                break;
            }
        }
        if (ynGood == false) {
            Debug.Log("GetObjectPos fail! (" + numTries + ")\n");
        } else {
            Debug.Log("GetObjectPos:" + cnt + " (" + numTries + ")\n");
        }
        return pos;
    }

    void UpdateObjects()
    {
        for (int n = 0; n < numObjects; n++)
        {
            UpdateSensorValue(n);
            UpdateDisplay(n);
            UpdatePointer(n);
            UpdateTarget(n);
            NavigateObject(n);
        }
    }

    void UpdateTarget(int n) {
        if (ynTargets == true)
        {
            if (parentTargetsGo == null)
            {
                parentTargetsGo = new GameObject("parentTargetsGo");
            }
        }
        else
        {
            if (parentTargetsGo != null)
            {
                DestroyImmediate(parentTargetsGo);
            }
        }
        float targetClearance = 10;
        if (objects[n].ynSensor == true)
        {
            float yaw = objects[n].yawPointer;
            Vector3 vector = objects[n].go.transform.position - objects[n].displayTarget;
            objects[n].target = objects[n].go.transform.position + Vector3.Normalize(vector) * targetClearance;
        }
        else
        {
            objects[n].target = objects[n].go.transform.position + objects[n].go.transform.forward * targetClearance;
        }
    }

    void NavigateObject(int n) {
        objects[n].look = smooth * objects[n].look + (1 - smooth) * objects[n].target;
        //
        if (ynTargets == true)
        {
            objects[n].targetGo.transform.position = objects[n].target;
            objects[n].lookGo.transform.position = objects[n].look;
        }
        //
        objects[n].go.transform.LookAt(objects[n].look);
        objects[n].go.transform.position += objects[n].go.transform.forward * speed;
        Vector3 eul = objects[n].go.transform.eulerAngles;
        objects[n].go.transform.eulerAngles = new Vector3(0, eul.y, 0);
        Vector3 pos = objects[n].go.transform.position;
        objects[n].go.transform.position = new Vector3(pos.x, meshHeight / 2, pos.z);
    }

    void UpdateSensorValue(int n) {
        bool ynSensor = false;
        objects[n].aveColor = GetAveColorCam(n);
        Color colorGo = objects[n].aveColor;
        objects[n].diffColor = GetColorDiffTarget(objects[n].aveColor);
        if (objects[n].diffColor < colorThreshold)
        {
            colorGo = Color.red;
            ynSensor = true;
        }
        objects[n].go.GetComponent<Renderer>().material.color = colorGo;
        objects[n].ynSensor = ynSensor;
    }
    void UpdateDisplay(int n) {
        float heightDisplayAboveObject = 30;
        objects[n].displayGo.transform.position = objects[n].go.transform.position + Vector3.up * heightDisplayAboveObject;
        RenderTexture.active = objects[n].cam.targetTexture;
        objects[n].displayTexture.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        float sumX = 0;
        float sumY = 0;
        float cx = 0;
        float cy = 0;
        int cnt = 0;
        //
        for (int nx = 0; nx < objects[n].displayTexture.width; nx++) {
            for (int ny = 0; ny < objects[n].displayTexture.height; ny++) {
                Color col = objects[n].displayTexture.GetPixel(nx, ny);
                float diff = GetColorDiffTarget(col);
                float value = 1 - diff;
                col = colorTarget * value;
                objects[n].displayTexture.SetPixel(nx, ny, col);
                if (value > colorThreshold) {
                    sumX += nx * value;
                    sumY += ny * value;
                    cnt++;
                }
                if (value > colorThreshold) {
                    objects[n].displayTexture.SetPixel(nx, ny, Color.yellow);
                }
            }        
        }
        cx = sumX / cnt;
        cy = sumY / cnt;
        if (objects[n].ynSensor == true)
        {
            objects[n].yawPointer = objects[n].go.transform.eulerAngles.y + (cx / resCam - .5f) * objects[n].cam.fieldOfView;
            objects[n].displayTexture.SetPixel((int)cx, (int)cy, Color.red);
            Vector3 pos = new Vector3(cx, cy, objects[n].cam.nearClipPlane);
            objects[n].displayTarget = objects[n].cam.ScreenToWorldPoint(pos);
        }
        objects[n].displayTexture.Apply();
    }
    void UpdatePointer(int n) {
        float scalePointer = 0;
        if (objects[n].ynSensor == true)
        {
            scalePointer = 20;
        }
        objects[n].pointerGo.transform.position = objects[n].go.transform.position;
        objects[n].pointerGo.transform.localScale = new Vector3(.1f, .1f, scalePointer);
        objects[n].pointerGo.transform.eulerAngles = new Vector3(0, objects[n].yawPointer, 0);
    }
    float GetColorDiffTarget(Color color) {
        Vector3 colorV3 = new Vector3(color.r, color.g, color.b);
        Vector3 targetColorV3 = new Vector3(colorTarget.r, colorTarget.g, colorTarget.b);
        return Vector3.Distance(Vector3.Normalize(colorV3), Vector3.Normalize(targetColorV3));
    } 

    Color GetAveColorTexture(Texture2D tex) {
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
    Color GetAveColorCam(int n)
    {
        RenderTexture.active = objects[n].cam.targetTexture;
        Texture2D tex = new Texture2D(resCam, resCam);
        tex.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        Color aveColor = GetAveColorTexture(tex);
        Destroy(tex);  // avoids memory leak 
        return aveColor;
    }

    public void CreateMesh()
    {
        resMeshX = textureTrack.width;
        resMeshY = textureTrack.height;

        Vector3[] points = new Vector3[resMeshX * resMeshY];
        for (int nx = 0; nx < resMeshX; nx++)
        {
            for (int ny = 0; ny < resMeshY; ny++)
            {
                int nPoints = nx * resMeshY + ny;
                float y = 0;
                Color color = textureTrack.GetPixel(nx, ny);
                if (color.grayscale > grayScaleThreshold) {
                    y = meshHeight;
                }
                points[nPoints] = new Vector3(nx, y, ny);
            }
        }
        Mesh mesh = CreateMeshFromPoints(points, resMeshX - 1, resMeshY - 1);
        meshGo = new GameObject("meshGo:" + points.Length + " points");
        MeshFilter meshFilter = meshGo.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = meshGo.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find(meshShaderName));
        meshRenderer.sharedMaterial.mainTexture = Resources.Load<Texture2D>(meshTextureName);
    }
    public Mesh CreateMeshFromPoints(Vector3[] points, int numX, int numY)
    {
        int numFacesPerQuad = 2;
        int numVerticesPerQuad = 4;
        int numQuads = numX * numY;
        int numTrianglesPerQuad = numFacesPerQuad * 3;
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[numVerticesPerQuad * numQuads];
        Vector2[] uvs = new Vector2[numVerticesPerQuad * numQuads];
        int[] triangles = new int[numTrianglesPerQuad * numQuads];
        int q = 0;
        for (int nx = 0; nx < numX; nx++)
        {
            for (int ny = 0; ny < numY; ny++)
            {
                int n0 = getDR2N(nx, ny, numY);
                int n1 = getDR2N(nx, ny + 1, numY);
                int n2 = getDR2N(nx + 1, ny + 1, numY);
                int n3 = getDR2N(nx + 1, ny, numY);
                Vector3 pnt0 = points[n0];
                Vector3 pnt1 = points[n1];
                Vector3 pnt2 = points[n2];
                Vector3 pnt3 = points[n3];
                //
                vertices[q * numVerticesPerQuad + 0] = pnt0;
                vertices[q * numVerticesPerQuad + 1] = pnt1;
                vertices[q * numVerticesPerQuad + 2] = pnt2;
                vertices[q * numVerticesPerQuad + 3] = pnt3;
                //
                float s = .05f;  // make bottom different texture uv from top
                if (pnt0.y != 0) {
                    s = .0125f;
                }
                uvs[q * numVerticesPerQuad + 0] = new Vector2(0, 0);
                uvs[q * numVerticesPerQuad + 1] = new Vector2(0, s);
                uvs[q * numVerticesPerQuad + 2] = new Vector2(s, s);
                uvs[q * numVerticesPerQuad + 3] = new Vector2(s, 0);
                //
                triangles[q * numTrianglesPerQuad + 0] = q * numVerticesPerQuad + 0;
                triangles[q * numTrianglesPerQuad + 1] = q * numVerticesPerQuad + 1;
                triangles[q * numTrianglesPerQuad + 2] = q * numVerticesPerQuad + 2;
                triangles[q * numTrianglesPerQuad + 3] = q * numVerticesPerQuad + 0;
                triangles[q * numTrianglesPerQuad + 4] = q * numVerticesPerQuad + 2;
                triangles[q * numTrianglesPerQuad + 5] = q * numVerticesPerQuad + 3;
                q++;
            }
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }
    int getDR2N(int dx, int dy, int numY)
    {
        return dx * (numY + 1) + dy;
    }
}
