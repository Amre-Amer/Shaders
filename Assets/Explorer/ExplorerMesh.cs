using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplorerMesh {
    //public ShaderPhysics global;
    public Vector3[] points;
    public GameObject go;
    public Mesh mesh;
    //public int resMeshX;
    //public int resMeshY;
    public Texture2D tex;
    public int resMeshX = 10;
    public int resMeshY = 10;
    public string meshShaderName = "Custom/Explorer";
    public string meshTextureName = "grid_opaque";
    public float grayscaleThreshold = .5f;
    public float meshHeight = 10;
    public float radius = 10;
    public Vector3 center = Vector3.zero;
    public ExplorerMesh() {
//        global = global0;
    }
    public GameObject CreateMeshGoFromTexture(Texture2D tex0){
        tex = tex0;
        resMeshX = tex.width;
        resMeshY = tex.height;
        points = CreatePointsFromTexture();
        mesh = CreateMeshFromPoints();
        GameObject meshGo = new GameObject("meshGo:" + points.Length + " points");
        MeshFilter meshFilter = meshGo.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = meshGo.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find(meshShaderName));
        meshRenderer.sharedMaterial.mainTexture = Resources.Load<Texture2D>(meshTextureName);
        return meshGo;
    }

    public Vector3[] CreateSpherePointsFromTexture()
    {
        GameObject go = new GameObject("tmp");
        points = new Vector3[resMeshX * resMeshY];
        for (int nx = 0; nx < resMeshX; nx++)
        {
            for (int ny = 0; ny < resMeshY; ny++)
            {
                int nPoints = nx * resMeshY + ny;
                float yaw = nx / (float)resMeshX * 360;
                float pitch = ny / (float)resMeshY  * 180 - 90;
                go.transform.position = center;
                go.transform.eulerAngles = new Vector3(pitch, yaw, 0);
                go.transform.position += go.transform.forward * radius;
                float ht = 0;
                Color color = tex.GetPixel(nx, ny);
                if (color.grayscale > grayscaleThreshold)
                {
                    ht = meshHeight;
                }
                ht = meshHeight * color.grayscale;
                go.transform.position += go.transform.forward * ht;
                points[nPoints] = go.transform.position;
            }
        }
        return points;
    }

    public Vector3[] CreatePointsFromTexture()
    {
        
        points = new Vector3[resMeshX * resMeshY];
        for (int nx = 0; nx < resMeshX; nx++)
        {
            for (int ny = 0; ny < resMeshY; ny++)
            {
                int nPoints = nx * resMeshY + ny;
                float y = 0;
                Color color = tex.GetPixel(nx, ny);
                if (color.grayscale > grayscaleThreshold)
                {
                    y = meshHeight;
                }
                y = meshHeight * color.grayscale;
                points[nPoints] = new Vector3(nx, y, ny);
            }
        }
        return points;
    }
    public Mesh CreateMeshFromPoints()
    {
        int numX = resMeshX - 1;
        int numY = resMeshY - 1;
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
                if (pnt0.y != 0)
                {
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
