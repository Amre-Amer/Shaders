using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysicsTrails {
    ShaderPhysics global;
    GameObject trailsGo;
    public Texture2D textureTrails;
    public ShaderPhysicsTrails(ShaderPhysics global0) {
        global = global0;
    }
    public void UpdateTrails()
    {
        if (textureTrails == null)
        {
            trailsGo = GameObject.CreatePrimitive(PrimitiveType.Quad);
            trailsGo.name = "trailsGo";
            trailsGo.transform.position = new Vector3(global.dataManager.resMeshX / 2, .1f, -global.dataManager.resMeshY / 2);
            trailsGo.transform.Rotate(90, 0, 0);
            //
            trailsGo.transform.localScale = new Vector3(global.dataManager.resMeshX, global.dataManager.resMeshY, 0);
            textureTrails = new Texture2D(global.dataManager.resMeshX, global.dataManager.resMeshY);
            textureTrails.filterMode = FilterMode.Point;
            global.miscManager.MakeTextureGrid(textureTrails);
            Material mat = trailsGo.GetComponent<Renderer>().material;
            mat.mainTexture = textureTrails;
        }
        Vector3 pos = trailsGo.transform.position;
        float y = .1f;
        //if (global.ynTrails == false)
        //{
        //    if (pos.y != -y)
        //    {
        //        pos.y = -y;
        //        trailsGo.transform.position = pos;
        //    }
        //}
        //else
        //{
        //    if (pos.y != y)
        //    {
        //        pos.y = y;
        //        trailsGo.transform.position = pos;
        //    }
        //}
        for (int n = 0; n < global.dataManager.objects.Length; n++)
        {
            UpdateTrail(n);
        }
    }

    void UpdateTrail(int n)
    {
        Vector3 pos = global.dataManager.objects[n].go.transform.position;
        Color color = new Color(1, 0, 0);
        int size = 1;
        for (int nx = 0; nx < size; nx++)
        {
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

}
