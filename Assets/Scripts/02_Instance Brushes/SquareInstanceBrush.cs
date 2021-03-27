using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SquareInstanceBrush : BaseCustomInstance
{

    
    protected override Vector3 givePos(float x, float z){
        var random = new System.Random();
        float xi = ((float)random.NextDouble())*2.0f - 1;
        float zi = ((float)random.NextDouble())*2.0f - 1;
        xi = xi*radius;
        zi = zi*radius;
        Vector3 p = new Vector3(x+xi,0.0f,z+zi);
        p.y = terrain.get(p.x,p.z);
        return p;
    }
}
