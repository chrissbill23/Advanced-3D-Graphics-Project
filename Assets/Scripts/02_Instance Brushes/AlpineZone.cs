using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AlpineZone : VegetationConstraint
{
    // Start is called before the first frame update
    protected override void setup()
    {

        var tmp =  Enum.GetValues(typeof(Layers));
        terrain_of_growth = new Layers[tmp.Length];
        foreach(int i in tmp){
            terrain_of_growth[i] = (Layers)i;
        }
        min_altitude = 0.6f;
        max_altitude = 1.0f;
    }
    protected override void updateAll(){
        //setup();
    }
}
