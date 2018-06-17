using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPhysicsMisc {
    AudioSource audioSource;
    ShaderPhysics global;
    public ShaderPhysicsMisc(ShaderPhysics globals0) {
        global = globals0;
    }
    public void MakeTextureGrid(Texture2D tex)
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
    public void MakeTextureClear(Texture2D tex)
    {
        Color[] colors = new Color[tex.width * tex.height];
        for (int n = 0; n < colors.Length; n++)
        {
            colors[n] = new Color(0, 0, 0, 0);
        }
        tex.SetPixels(colors);
        tex.Apply();
    }
    public void PlaySound(GameObject go)
    {
        if (audioSource == null)
        {
            audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = Resources.Load<AudioClip>("Sound");
            audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying == false)
            {
                audioSource.Play();
            }
        }
    }

    public void UpdateFar()
    {
        for (int n = 0; n < global.dataManager.objects.Length; n++)
        {
            int nx = (int)global.dataManager.objects[n].go.transform.position.x;
            int ny = (int)global.dataManager.objects[n].go.transform.position.z;
            if (global.dataManager.textureTrack.GetPixel(nx, ny) != Color.black)
            {
                global.dataManager.cntErrors++;
                global.dataManager.objects[n].go.transform.position = GetRandomObjectPos();
            }
        }
    }

    public void MakeMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.DisableKeyword("_ALPHABLEND_ON");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
    }

    public Vector3 GetRandomObjectPos()
    {
        Vector3 pos = Vector3.zero;
        //return pos;
        bool ynGood = true;
        int cnt = 0;
        int numTries = 50;
        for (int n = 0; n < numTries; n++) // 10 tries
        {
            cnt++;
            int r = 4;
            int rx = Random.Range(r, global.dataManager.resMeshX - r);
            int ry = Random.Range(r, global.dataManager.resMeshY - r);
            ynGood = true;
            for (int nx = -r; nx <= r; nx++)
            {
                for (int ny = -r; ny <= r; ny++)
                {
                    Color color = global.dataManager.textureTrack.GetPixel(rx + nx, ry + ny);
                    if (color.grayscale > global.dataManager.grayScaleThreshold)
                    {
                        ynGood = false;
                        break;
                    }
                }
                if (ynGood == false)
                {
                    break;
                }
            }
            if (ynGood == true)
            {
                float y = global.dataManager.meshHeight / 2;
                pos = new Vector3(rx, y, ry);
                break;
            }
        }
        if (ynGood == false)
        {
            Debug.Log("GetObjectPos fail! (" + numTries + ")\n");
        }
        else
        {
//            Debug.Log("GetObjectPos:" + cnt + " (" + numTries + ")\n");
        }
        return pos;
    }

    public void UpdateCam()
    {
        Camera cam = Camera.main;
        GameObject go = global.dataManager.objects[global.dataManager.nTracked].go;
        cam.transform.position = go.transform.position + Vector3.up * 10 + Vector3.forward * -3;
        cam.transform.LookAt(go.transform.position);
    }

    public Text CreateText(Vector3 pos, string txt, Color color)
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

    public float GetColorDiffTarget(Color color)
    {
        Vector3 colorV3 = new Vector3(color.r, color.g, color.b);
        Vector3 targetColorV3 = new Vector3(global.dataManager.colorTarget.r, global.dataManager.colorTarget.g, global.dataManager.colorTarget.b);
        return Vector3.Distance(Vector3.Normalize(colorV3), Vector3.Normalize(targetColorV3));
    }

    public Color GetAveColorTexture(Texture2D tex)
    {
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

}
