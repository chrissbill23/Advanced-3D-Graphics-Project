using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MontaneTrees : VegetationConstraint
{
    // Start is called before the first frame update
    protected override void setup()
    {
        terrain_of_growth = new Layers[2];
        terrain_of_growth[0] = Layers.Grass;
        terrain_of_growth[1] = Layers.Mud;
        min_altitude = 0.225f;
        max_altitude = 0.425f;
    }
    protected override void updateAll(){
        // setup();
    }
}
