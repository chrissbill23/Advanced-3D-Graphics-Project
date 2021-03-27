using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class SmoothBrush : TerrainBrush
{

    private int intensity = 1;


    public override void draw(int x, int z)
    {

        /* for (int zi = -radius + 1; zi <= radius; zi++)
         {
             for (int xi = -radius + 1; xi <= radius; xi++)
             {
                 if (zi == radius && xi == radius)
                     continue;
                 Vector3 p_now = new Vector3((float)(x + xi), terrain.get(x + xi, z + zi), z + zi);
                 Vector3 p_prec = new Vector3(p_now.x, terrain.get(p_now.x, p_now.z - 1), p_now.z - 1);
                 Vector3 p_prec2 = new Vector3(p_now.x - 1, terrain.get(p_now.x - 1, p_now.z), p_now.z);
                 Vector3 p_next = new Vector3(p_now.x, terrain.get(p_now.x, p_now.z + 1), p_now.z + 1);
                 Vector3 p_next2 = new Vector3(p_now.x + 1, terrain.get(p_now.x + 1, p_now.z), p_now.z);

                 Vector3 q_prec = p_now * 0.5f + p_next * 0.5f;
                 Vector3 q_next = p_prec * 0.125f + p_now * 0.75f + p_next * 0.125f;

                 Vector3 q_prec2 = p_now * 0.5f + p_next2 * 0.5f;
                 Vector3 q_next2 = p_prec2 * 0.125f + p_now * 0.75f + p_next2 * 0.125f;

                 terrain.set(q_prec.x, q_prec.z, q_prec.y);
                 terrain.set(q_next.x, q_next.z, q_next.y);
                 terrain.set(q_prec2.x, q_prec2.z, q_prec2.y);
                 terrain.set(q_next2.x, q_next2.z, q_next2.y);
             }
         }*/
        for (int zi = -radius; zi <= radius; zi++)
         {
             for (int xi = -radius; xi <= radius; xi++)
             {
                 if (terrain.get(x + xi, z + zi) <= 0.0f || zi - intensity < -radius || zi + intensity > radius || xi - intensity < -radius || xi + intensity > radius)
                     continue;
                 float tot_value = 0.0f;
                 int n = 0;
                 for(int i = zi - intensity; i<= zi + intensity; ++i)
                 {
                     for (int j= xi - intensity; j<= xi + intensity; ++j)
                     {
                         float v = terrain.get(x + j, z + i);
                         if (v > 0.0f)
                         {
                             tot_value += v;
                             n += 1;
                         }
                     }
                 }
                 if(n > 0)
                     tot_value /= n;
                 terrain.set((float)(x + xi), (float)(z + zi), tot_value);
             }
         }

    }
}
