using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootHillsTrees : VegetationConstraint
{
    // Start is called before the first frame update
    protected override void setup()
    {
        terrain_of_growth = new Layers[3];
        terrain_of_growth[0] = Layers.Mud;
        terrain_of_growth[1] = Layers.Grass;
        terrain_of_growth[2] = Layers.Sand;
        max_altitude = 0.225f;
    }
    protected override void updateAll(){
        //setup();
    }
}
