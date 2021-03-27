using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleInstanceBrush: BaseCustomInstance
{

    
    protected override Vector3 givePos(float x, float z){
        var random = new System.Random();
        float angle = (float)(random.NextDouble() * 2.0 * Mathf.PI);
        float r = (float)(random.NextDouble() * radius);

        float xi = radius * (float)(Mathf.Sin(angle));
        float zi = radius * (float)(Mathf.Cos(angle));
        Vector3 p = new Vector3(x+xi,0.0f,z+zi);
        p.y = terrain.get(p.x,p.z);
        return p;
    }
}
