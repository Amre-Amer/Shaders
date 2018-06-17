using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysicsMesh {
    public ShaderPhysics global;
    public Vector3[] points;
    public GameObject go;
    public Mesh mesh;
    //public int resMeshX;
    //public int resMeshY;
    public Texture2D tex;
    public ShaderPhysicsMesh(ShaderPhysics global0) {
        global = global0;
    }
    public GameObject CreateMeshGoFromTexture(Texture2D tex0){
        tex = tex0;
        global.dataManager.resMeshX = tex.width;
        global.dataManager.resMeshY = tex.height;
        points = CreatePointsFromTexture();
        mesh = CreateMeshFromPoints();
        GameObject meshGo = new GameObject("meshGo:" + points.Length + " points");
        MeshFilter meshFilter = meshGo.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        MeshRenderer meshRenderer = meshGo.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = new Material(Shader.Find(global.dataManager.meshShaderName));
        meshRenderer.sharedMaterial.mainTexture = Resources.Load<Texture2D>(global.dataManager.meshTextureName);
        return meshGo;
    }
    public Vector3[] CreatePointsFromTexture()
    {
        
        points = new Vector3[global.dataManager.resMeshX * global.dataManager.resMeshY];
        for (int nx = 0; nx < global.dataManager.resMeshX; nx++)
        {
            for (int ny = 0; ny < global.dataManager.resMeshY; ny++)
            {
                int nPoints = nx * global.dataManager.resMeshY + ny;
                float y = 0;
                Color color = tex.GetPixel(nx, ny);
                if (color.grayscale > global.dataManager.grayScaleThreshold)
                {
                    y = global.dataManager.meshHeight;
                }
                points[nPoints] = new Vector3(nx, y, ny);
            }
        }
        return points;
    }
    public Mesh CreateMeshFromPoints()
    {
        int numX = global.dataManager.resMeshX - 1;
        int numY = global.dataManager.resMeshY - 1;
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
