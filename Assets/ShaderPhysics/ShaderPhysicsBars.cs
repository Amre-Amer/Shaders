using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysicsBars {
    public float barScale = 20;
    public GameObject parentBarsGo;
    public Vector3 posBar;
    public GameObject barThreshold;
    public GameObject barTop;
    public List<GameObject> bars;
    public GameObject go;
    ShaderPhysics global;
    int index;
    public ShaderPhysicsBars(ShaderPhysics global0) {
        global = global0;
    }
    public void UpdateBars()
    {
        if (bars == null)
        {
            posBar = new Vector3(0, global.dataManager.meshHeight, global.dataManager.resMeshY);
            global.dataManager.numBars = global.dataManager.resMeshX;
            parentBarsGo = new GameObject("bars");
            bars = new List<GameObject>();
            for (int n = 0; n < global.dataManager.numBars; n++)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bars.Add(go);
                index = bars.Count - 1;
                go.transform.parent = parentBarsGo.transform;
                go.transform.position = posBar + new Vector3(index, 0, 0);
                go.transform.localScale = new Vector3(.75f, 0, .75f);
            }
        }
        for (int n = global.dataManager.numBars - 1; n > 0; n--)
        {
            float yPrev = bars[n - 1].transform.localScale.y;
            Color colorPrev = bars[n - 1].GetComponent<Renderer>().material.color;
            UpdateBar(n, yPrev, colorPrev);
        }
        Color color = Color.white;
        float sumNew = 0;
        for (int n = 0; n < global.dataManager.numObjects; n++)
        {
            if (global.dataManager.objects[n].ynSensor == true)
            {
                color = Color.red;
            }
            if (global.dataManager.objects[n].ynSensorLast != global.dataManager.objects[n].ynSensor)
            {
                color = Color.cyan;
            }
            sumNew += global.dataManager.objects[n].diffColor;
        }
        float diffColorNew = sumNew / global.dataManager.numObjects;
        // new bar value 
        float yNew = diffColorNew * barScale;
        UpdateBar(0, yNew, color);
        UpdateBarThreshold();
        UpdateBarTop();
    }
    public void UpdateBarTop()
    {
        if (barTop == null)
        {
            barTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barTop.name = "barTop";
            barTop.transform.parent = parentBarsGo.transform;
            barTop.GetComponent<Renderer>().material.color = Color.blue;
            float y = 1 * barScale;
            barTop.transform.position = posBar + new Vector3(global.dataManager.numBars / 2, y, 0);
            barTop.transform.localScale = new Vector3(global.dataManager.numBars, .25f, 1);
        }
    }
    public void UpdateBarThreshold()
    {
        if (barThreshold == null)
        {
            barThreshold = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barThreshold.name = "barThreshold";
            barThreshold.transform.parent = parentBarsGo.transform;
            barThreshold.GetComponent<Renderer>().material.color = Color.green;
        }
        float y = global.dataManager.colorThreshold * barScale;
        barThreshold.transform.position = posBar + new Vector3(global.dataManager.numBars / 2, y, 0);
        barThreshold.transform.localScale = new Vector3(global.dataManager.numBars, .25f, 1);
    }
    public void UpdateBar(int n, float y, Color color)
    {
        Vector3 sca = bars[n].transform.localScale;
        bars[n].transform.localScale = new Vector3(sca.x, y, sca.z);
        Vector3 pos = bars[n].transform.position;
        bars[n].transform.position = new Vector3(pos.x, global.dataManager.meshHeight + y / 2, pos.z);
        bars[n].GetComponent<Renderer>().material.color = color;
    }

}
