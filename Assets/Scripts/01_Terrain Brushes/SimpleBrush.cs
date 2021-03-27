﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleBrush : TerrainBrush {

    public float height = 5;

    public override void draw(int x, int z) {
        if(!randomPoints){
            for (int zi = -radius; zi <= radius; zi++) {
                for (int xi = -radius; xi <= radius; xi++) {
                    terrain.set(x + xi, z + zi, height);
                }
            }
        } else
        {
            Vector2[] points = ramdPoints(x,z);
            for (int i = 0; i < points.Length; ++i )
            {
                terrain.set(points[i].x, points[i].y, height);
                //print(points[i]);
            }
        }
    }
}
