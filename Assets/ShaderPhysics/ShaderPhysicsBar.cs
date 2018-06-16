using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderPhysicsBar{
    public GameObject go;
    ShaderPhysics global;
    int index;
    public ShaderPhysicsBar(ShaderPhysics global0) {
        global = global0;
        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        global.bars.Add(this);
        index = global.bars.Count - 1;
        go.transform.parent = global.parentBarsGo.transform;
        go.transform.position = global.posBar + new Vector3(index, 0, 0);
        go.transform.localScale = new Vector3(.75f, 0, .75f);
    }
}
