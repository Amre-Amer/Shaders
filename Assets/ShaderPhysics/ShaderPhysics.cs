using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPhysics : MonoBehaviour {
    public bool ynTrackSphere;
    public bool ynUseNear;
    public bool ynTrails = true;
    public bool ynUseDelta = true;
    public bool ynStep;
    float delay = 1;
    float startTime;
    public Texture2D textureTrack;
    public GameObject cageGo;
    public GameObject display;
    public Color aveColor0;
    public Color nearColor0;
    public float diffAveColor0;
    public float diffNearColor0;
    public float diffColor0;
    public float colorThreshold0;
    public bool ynSensor0;
    int numObjects = 10;
    int numSphereObjects = 3;
    float speed = .2f;
    float colorThreshold = .1f; //.85f;  // .95
    float range = 5;
    int resCam = 10; //10;
    float angRange = 30;
    float grayScaleThreshold = .5f;
    float meshHeight = 5;
    float objectScale = 5f;
    int numBars; // = resMeshX
    float barScale = 100;
    float distFar = 200;
    GameObject meshGo;
    Camera[] cams;
    Color colorTarget = Color.white;
    int resMeshX; // = image width
    int resMeshY; // = image height
    GameObject[] objectGos;
    //Texture2D texture;
    RenderTexture renderTexture;
    string meshShaderName = "Custom/ShaderPhysicsTransparent";
    string meshTextureName = "grid_opaque";
    bool[] ynSensorsLast;
    int nTracked;
    GameObject[] bars;
    GameObject barThreshold;
    GameObject barTop;
    AudioSource audioSource;
    Texture2D textureTrails;
    GameObject trailsGo;
    GameObject parentObjectsGo;
    GameObject parentBarsGo;
    float deltaLast;
    Text textTimer;

    // Use this for initialization
	void Start () {
        UpdateTracked();
        CreateMesh();
        CreateObjects();
	}
	
	// Update is called once per frame
	void Update () {
        float delay0 = delay;
        if (ynSensor0 == true) delay0 *= 4;
        if (ynStep == true && Time.realtimeSinceStartup - startTime < delay0) {
            return;            
        }
        startTime = Time.realtimeSinceStartup;
        UpdateTracked();
        UpdateMaterials();
        UpdateObjects();
        UpdateCam();
        UpdateBars();
        //UpdateFar();
        UpdateTrails();
        UpdateTimer();
	}

    void UpdateTimer() {
        if (textTimer == null) {
            textTimer = CreateText(Vector3.zero, "test", Color.red);
            textTimer.transform.position = new Vector3(resMeshX / 5, resMeshX / 5, resMeshY);
        }
        float prog = 100 * GetProgress();
        textTimer.text = prog.ToString("F2") + " %";
    }
    float GetProgress()
    {
        int cnt = 0;
        for (int nx = 0; nx < textureTrails.width; nx++) {
            for (int ny = 0; ny < textureTrails.height; ny++)
            {
                Color col = textureTrails.GetPixel(nx, ny);
                if (col != Color.black) {
                    if (textureTrack.GetPixel(nx, ny) == Color.black)
                    {
                        cnt++;
                    }
                }
            }
        }
        return cnt / (float)(textureTrails.width * textureTrails.height);
    }
    void UpdateTrails() {
        if (ynTrails == false) return;
        if (textureTrails == null) {
            trailsGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            trailsGo.name = "trailsGo";
            trailsGo.transform.position = new Vector3(resMeshX / 2, .1f, resMeshY / 2);
            trailsGo.transform.Rotate(90, 0, 0);
                //
            trailsGo.transform.localScale = new Vector3(resMeshX, resMeshY, 0);
            textureTrails = new Texture2D(resMeshX, resMeshY);
            textureTrails.filterMode = FilterMode.Point;
            //MakeTextureClear(textureTrails);
            MakeTextureGrid(textureTrails);
            //textureTrails = Resources.Load<Texture2D>("grid_opaque");
            Material mat = trailsGo.GetComponent<Renderer>().material;
            mat.mainTexture = textureTrails;
            //MakeMaterialTransparent(mat);
            //mat.SetTextureScale("_MainTex", new Vector2(5, 5));
        }
        if (textureTrails != null)
        {
            for (int n = numSphereObjects; n < numObjects; n++)
            {
                UpdateTrail(n);
            }
        }
    }

    void UpdateTrail(int n)
    {
        Vector3 pos = objectGos[n].transform.position;
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
                //if (nx % 10 == 0)
                //{
                //    color += new Color(0, 1, 0);
                //    color /= 2;
                //}
                //if (ny % 10 == 0)
                //{
                //    color += new Color(0, 0, 1);
                //    color /= 2;
                //}
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
    void UpdateTracked() {
        nTracked = numSphereObjects;
        if (ynTrackSphere == true) {
            nTracked--;
        }
    }

    void UpdateFar() {
        for (int n = 0; n < numObjects; n++) {
            int nx = (int)objectGos[n].transform.position.x;
            int ny = (int)objectGos[n].transform.position.z;
            if (textureTrack.GetPixel(nx, ny) != Color.black) {
                objectGos[n].transform.position = GetObjectPos();
                PlaySound();
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
            bars = new GameObject[numBars];
            for (int n = 0; n < numBars; n++)
            {
                bars[n] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bars[n].transform.parent = parentBarsGo.transform;
                bars[n].transform.position = new Vector3(n, 0, -meshHeight);
                bars[n].transform.localScale = new Vector3(.75f, 0, .75f);
            }
        }
        for (int n = numBars - 1; n > 0; n--) {
            float yPrev = bars[n - 1].transform.localScale.y;
            Color colorPrev = bars[n - 1].GetComponent<Renderer>().material.color;
            UpdateBar(n, yPrev, colorPrev);
        }
        float yNew = diffColor0 * barScale;
        Color color = Color.white;
        if (ynSensor0 == true)
        {
            color = Color.red;
        }
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
            barTop.transform.position = new Vector3(numBars / 2, -meshHeight + y, -meshHeight);
            barTop.transform.localScale = new Vector3(numBars, .25f, 1);
        }
    }
    void UpdateBarThreshold() {
        if (barThreshold == null) {
            barThreshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barThreshold.name = "barThreshold";
            barThreshold.transform.parent = parentBarsGo.transform;
            barThreshold.GetComponent<Renderer>().material.color = Color.green;
            float y = colorThreshold * barScale;
            barThreshold.transform.position = new Vector3(numBars / 2, -meshHeight + y, -meshHeight);
            barThreshold.transform.localScale = new Vector3(numBars, .25f, 1);
        }
    }
    void UpdateBar(int n, float y, Color color) {
        Vector3 sca = bars[n].transform.localScale;
        bars[n].transform.localScale = new Vector3(sca.x, y, sca.z);
        Vector3 pos = bars[n].transform.position;
        bars[n].transform.position = new Vector3(pos.x, -meshHeight + y / 2, pos.z);
        bars[n].GetComponent<Renderer>().material.color = color;
    }

    void UpdateCam() {
        Camera cam = Camera.main;
        GameObject go = objectGos[nTracked];
        cam.transform.position = go.transform.position + Vector3.up * 10 + Vector3.forward * -3;
        cam.transform.LookAt(go.transform.position);
    }

    void UpdateMaterials()
    {
        Vector4[] pos4s = new Vector4[numObjects];
        for (int n = 0; n < numObjects; n++) {
            Vector3 pos = objectGos[n].transform.position;
            pos4s[n] = new Vector4(pos.x, pos.y, pos.z, 0);
        }
        Shader.SetGlobalVectorArray("_Centers", pos4s);
        Shader.SetGlobalInt("_NumCenters", numObjects);
        Shader.SetGlobalFloat("_Range", range);
    }
    public void CreateObjects() {
        objectGos = new GameObject[numObjects];
        parentObjectsGo = new GameObject("objects:" + numObjects);
        for (int n = 0; n < numObjects; n++)
        {
            objectGos[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objectGos[n].transform.parent = parentObjectsGo.transform;
            objectGos[n].transform.position = GetObjectPos();
            if (n < numSphereObjects) {
                objectGos[n].transform.position = cageGo.transform.position;
            }
            if (n == nTracked)
            {
                objectGos[n].name = "tracked";
            }
            float s = objectScale;
            objectGos[n].transform.localScale = new Vector3(s, s, s);
            float yaw = Random.Range(0, 360f);
            objectGos[n].transform.Rotate(0, yaw, 0);
            if (n < numSphereObjects)
            {
                float pitch = Random.Range(0, 360f);
                objectGos[n].transform.Rotate(pitch, 0, 0);
            }
        }
    }
    Vector3 GetObjectPos() {
        Vector3 pos = Vector3.zero;
        for (int n = 0; n < 10; n++)
        {
            int nx = Random.Range(0, resMeshX);
            int ny = Random.Range(0, resMeshY);
            Color color = textureTrack.GetPixel(nx, ny);
            if (color.grayscale < grayScaleThreshold) {
                float y = meshHeight / 2;
                pos = new Vector3(nx, y, ny);
                break;
            }
        }
        return pos;
    }

    void UpdateObjects()
    {
        if (ynSensorsLast == null) {
            ynSensorsLast = new bool[numObjects];
        }
        if (cams == null) {
            cams = new Camera[numObjects];
        }
        for (int n = 0; n < numObjects; n++)
        {
            if (cams[n] == null)
            {
                cams[n] = objectGos[n].AddComponent<Camera>();
                renderTexture = new RenderTexture(resCam, resCam, 16, RenderTextureFormat.ARGB32);
                cams[n].targetTexture = renderTexture;
                cams[n].targetTexture.filterMode = FilterMode.Point;
                //
                if (n == nTracked)
                {
                    if (display != null)
                    {
                        display.GetComponent<Renderer>().material.mainTexture = cams[n].targetTexture;
                    }
                }
            }
            bool ynSensor = GetSensorValue(n);
            NavigateObject(n, ynSensor);
        }
    }

    void NavigateObject(int n, bool ynSensor) {
        float s = 1;
        //if (ynSensor == true && ynSensorsLast[n] == false)
        if (ynSensor == true)
        {
            float yaw = Random.Range(180 - angRange, 180 + angRange);
            objectGos[n].transform.Rotate(0, yaw, 0);
            if (n < numSphereObjects)
            {
                float pitch = Random.Range(-angRange, angRange);
                objectGos[n].transform.Rotate(pitch, 0, 0);
            }
            s = 3;
        }
        ynSensorsLast[n] = ynSensor;
        objectGos[n].transform.position += objectGos[n].transform.forward * speed * s;
    }

    bool GetSensorValue(int n) {
        bool ynSensor = false;
        Color aveColor = GetAveColorCam(n);
        Color nearColor = GetNearestColor(n);
        float diffAveColor = GetColorDiff(aveColor);
        float diffNearColor = GetColorDiff(nearColor);
        Color colorGo = aveColor;
        float diffColor = diffAveColor;
        if (ynUseNear == true) {
            diffColor = diffNearColor;
        }
        if (ynUseDelta == true) {
            diffColor = Mathf.Abs(aveColor.grayscale - deltaLast); 
            deltaLast = aveColor.grayscale;
        }
        //        if (diffColor < colorThreshold)
        //Debug.Log("aveColor:" + aveColor.grayscale + " diffColor:" + diffColor + "\n");
        if (diffColor > colorThreshold)
        {
            colorGo = Color.red;
            ynSensor = true;
        }
        if (n == nTracked)
        {
            aveColor0 = aveColor;
            diffAveColor0 = diffAveColor;
            diffNearColor0 = diffNearColor;
            nearColor0 = nearColor;
            diffColor0 = diffColor;
            colorThreshold0 = colorThreshold;
            ynSensor0 = ynSensor;
            colorGo = Color.white;
        }
        objectGos[n].GetComponent<Renderer>().material.color = colorGo;
        return ynSensor;
    }
    float GetColorDiff(Color color) {
        Vector3 colorV3 = new Vector3(color.r, color.g, color.b);
        Vector3 targetColorV3 = new Vector3(colorTarget.r, colorTarget.g, colorTarget.b);
        return Vector3.Distance(colorV3, targetColorV3);
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
        RenderTexture.active = cams[n].targetTexture;
        Texture2D tex = new Texture2D(resCam, resCam);
        tex.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        Color aveColor = GetAveColorTexture(tex);
        Destroy(tex);  // avoids memory leak 
        return aveColor;
    }

    Color GetNearestColor(int n)
    {
        Color nearColor = Color.black;
        float distColorMin = -1;
        RenderTexture.active = cams[n].targetTexture;
        Texture2D tex = new Texture2D(resCam, resCam);
        tex.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        for (int nx = 0; nx < tex.width; nx++)
        {
            for (int ny = 0; ny < tex.height; ny++)
            {
                Color color = tex.GetPixel(nx, ny);
                float distColor = GetColorDiff(color);
                if (distColorMin == -1 || distColor < distColorMin)
                {
                    distColorMin = distColor;
                    nearColor = color;
                }
            }
        }
        Destroy(tex);  // avoids memory leak 
        return nearColor;
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
