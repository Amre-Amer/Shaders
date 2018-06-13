using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysics : MonoBehaviour {
    GameObject meshGo;
    Camera[] cams;
    int numObjects = 10;
    int numSphereObjects = 3;
    float speed = .2f;
    float colorThreshold = .75f;
    float range = 5;
    Color colorTarget = Color.white;
    int resCam = 1;
    int resMeshX;
    int resMeshY;
    GameObject[] objectGos;
    Texture2D texture;
    RenderTexture renderTexture;
    public Texture2D textureTrack;
    float angRange = 30;
    float grayScaleThreshold = .5f;
    float meshHeight = 5;
    float objectScale = 5f;
    public GameObject sphere;
    public Color aveColor;
    string meshShaderName = "Custom/ShaderPhysicsTransparent";
    string meshTextureName = "grid_opaque";

	// Use this for initialization
	void Start () {
        CreateMesh();
        CreateObjects();
	}
	
	// Update is called once per frame
	void Update () {
        UpdateMaterials();
        UpdateObjects();
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
        for (int n = 0; n < numObjects; n++)
        {
            objectGos[n] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objectGos[n].transform.position = GetObjectPos();
            if (n < numSphereObjects) {
                objectGos[n].transform.position = sphere.transform.position;
            }
            float s = objectScale;
            objectGos[n].transform.localScale = new Vector3(s, s, s);
            float ang = Random.Range(0, 360f);
            objectGos[n].transform.Rotate(0, ang, 0);
            //float ang2 = Random.Range(0, 360f);
            //objectGos[n].transform.Rotate(ang, 0, 0);
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
            }
            aveColor = GetAveColor(n);
            Color nearColor = GetNearestColor(n);
            float diffColor = GetColorDiff(aveColor);
            Color colorGo = aveColor;
            colorGo = new Color(diffColor, diffColor, diffColor);
            //if (aveColor.grayscale > colorThreshold)
            if (diffColor < colorThreshold)
            {
                //if (n == 0)
                //{
                //    Debug.Log("diffColor:" + diffColor + "\n");
                //}
                colorGo = Color.red;
                float yaw = Random.Range(180 - angRange, 180 + angRange);
                objectGos[n].transform.Rotate(0, yaw, 0);
                if (n < numSphereObjects)
                {
                    float pitch = Random.Range(-angRange, angRange);
                    objectGos[n].transform.Rotate(pitch, 0, 0);
                }
            }
            objectGos[n].transform.position += objectGos[n].transform.forward * speed;
            objectGos[n].GetComponent<Renderer>().material.color = colorGo;
        }
    }

    float GetColorDiff(Color color) {
        Vector3 colorV3 = new Vector3(color.r, color.g, color.b);
        Vector3 targetColorV3 = new Vector3(colorTarget.r, colorTarget.g, colorTarget.b);
        return Vector3.Distance(colorV3, targetColorV3);
    } 

    Color GetAveColor(int n)
    {
        Color sumColor = Color.black;
        RenderTexture.active = cams[n].targetTexture;
        texture = new Texture2D(resCam, resCam);
        texture.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        for (int nx = 0; nx < texture.width; nx++)
        {
            for (int ny = 0; ny < texture.height; ny++)
            {
                Color color = texture.GetPixel(nx, ny);
                sumColor += color;
            }
        }
        Destroy(texture);  // avoids memory leak 
        return sumColor / (texture.width * texture.height);
    }

    Color GetNearestColor(int n)
    {
        Color nearColor = Color.black;
        float distColorMin = -1;
        RenderTexture.active = cams[n].targetTexture;
        texture = new Texture2D(resCam, resCam);
        texture.ReadPixels(new Rect(0, 0, resCam, resCam), 0, 0);
        //
        for (int nx = 0; nx < texture.width; nx++)
        {
            for (int ny = 0; ny < texture.height; ny++)
            {
                Color color = texture.GetPixel(nx, ny);
                float distColor = GetColorDiff(color);
                if (distColorMin == -1 || distColor < distColorMin)
                {
                    distColorMin = distColor;
                    nearColor = color;
                }
            }
        }
        Destroy(texture);  // avoids memory leak 
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
        meshGo = new GameObject("meshGo");
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
