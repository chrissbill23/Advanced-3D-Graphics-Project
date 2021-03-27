using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class GaussianBrush : TerrainBrush
{

    public float height = 10;
    public float std = 5;
    

    public override void draw(int x, int z)
    {
        
            for (int zi = -radius; zi <= radius; zi++)
            {
                for (int xi = -radius; xi <= radius; xi++)
                {
                    float exp = (float)Math.Exp(-(zi * zi *1.0f+ xi * xi*1.0f) / (2 * std * std));
                    float z1 =(float) (height / (2 * std * std * Math.PI ) * exp);
                    float y = terrain.get(x + xi, z + zi) + z1;
                    if(y > terrain.max_height){
                        y = terrain.max_height;
                    }
                    terrain.set(x+xi, z+zi , y) ;
                }
            } 
    }
}
