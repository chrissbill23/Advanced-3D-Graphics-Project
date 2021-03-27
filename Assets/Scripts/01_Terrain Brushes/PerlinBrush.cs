using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class PerlinBrush : TerrainBrush
{
    public float p = 0.5f;
    public float q = 2.0f;
    public int octaves = 7;

    private Vector2 randGrad()
    {
        var random = new System.Random();
        return new Vector3((float)random.NextDouble() * 2.0f - 1.0f, (float)random.NextDouble() * 2 - 1).normalized;
    }
    private float fade(float t)
    {
        return t * t * t * (t * (t * 6.0f - 15.0f) + 10.0f);
    }
    private List<Vector3> cubicInterpolation(ref List<Vector3> P,  ref Vector3 mu) {
        //Paul Breeuwsma coefficients
        List<List<Vector3>> Q = new List<List<Vector3>>();
        List<Vector3> Q2 = new List<Vector3> ();
        float mux = mu.x * mu.x;
        float muy = mu.y * mu.y;
        float muz = mu.z * mu.z;
        var rand = new System.Random();
        float freq = q;
        float tot_amp = 0.0f;
        float amp = p;
        for (int j=0; j<octaves; j++)
        {
            tot_amp += amp;
            Q.Add(new List<Vector3>());
            for (int i = 0; i < P.Count; i++)
            {
                Vector3 p_now = P[i];
                //Vector3 p_next = new Vector3(p_now.x + (float)rand.NextDouble(),0.0f, p_now.z + (float)rand.NextDouble());

                float x = p_now.x / (2.0f * radius) * freq;
                float z = p_now.z / (2.0f * radius) * freq;
                p_now.y = amp*Mathf.PerlinNoise(x, z);//Math.Max(0.0f,noiseValue(ref p_now));
                                                  //p_next.y = Math.Max(0.0f, noiseValue(ref p_next));
                print(p_now.y);
                Q[j].Add(p_now);
                //Q.Add(p_next);
            }
            freq *= q;
            amp *= p;
        }
        for (int i = 0; i < P.Count; i++)
        {
            Vector3 pnew = new Vector3(P[i].x,0.0f, P[i].z);
            for (int j = 0; j < octaves; j++)
            {
                pnew.y =pnew.y + Q[j][i].y;
            }
            pnew.y = pnew.y / tot_amp;
            Q2.Add(pnew);
            print(Q2[i].y);
        }

            return Q2;
    
    }
    private float noiseValue(ref Vector3 P)
    {
        Vector2 p = new Vector2(P.x, P.z);
        Vector2 p0 = new Vector2((int)p.x, (int)p.y);
        Vector2 p1 = p0 + new Vector2(1.0f, 0.0f);
        Vector2 p2 = p0 + new Vector2(0.0f, 1.0f);
        Vector2 p3 = p0 + new Vector2(1.0f, 1.0f);

        Vector2 g0 = randGrad();
        Vector2 g1 = randGrad();
        Vector2 g2 = randGrad();
        Vector2 g3 = randGrad();

        float t0 = p.x - p0.x;
        float fade_t0 = fade(t0);

        float t1 = p.y - p0.y;
        float fade_t1 = fade(t1);

        float p0p1 = (1.0f - fade_t0) * Vector2.Dot(g0, (p - p0)) + fade_t0 * Vector2.Dot(g1, (p - p1));
        float p2p3 = (1.0f - fade_t0) * Vector2.Dot(g2, (p - p2)) + fade_t0 * Vector2.Dot(g3, (p - p3));


        return (1.0f - fade_t1) * p0p1 + fade_t1 * p2p3;

    }
    private float perlinValue(float y) {
        return 0.0f;
    }
    public override void draw(int x, int z)
    {
        List<List<Vector3>> Q = new List<List<Vector3>>();
        List<Vector3> Q2 = new List<Vector3>();
        float freq = q;
        float tot_amp = 0.0f;
        float amp = p;
        for (int j = 0; j < octaves; j++)
        {
            tot_amp += amp;
            Q.Add(new List<Vector3>());

            for (int zi = -radius; zi <= radius; zi++)
            {
                for (int xi = -radius; xi <= radius; xi++)
                {
                    Vector3 p = new Vector3(x * 1.0f + xi,0.0f, z * 1.0f + zi);
                    float x_n = p.x / (2.0f * radius) * freq;
                    float z_n = p.z / (2.0f * radius) * freq;
                    p.y = amp * Mathf.PerlinNoise(x_n, z_n);//Math.Max(0.0f,noiseValue(ref p_now));
                                                            //p_next.y = Math.Max(0.0f, noiseValue(ref p_next));
                    Q[j].Add(p);
                }
            }
            freq *= q;
            amp *= p;
        }
        for (int i = 0; i < Q[0].Count; i++)
        {
            Vector3 p = Q[0][i];
            for (int j = 1; j < octaves; j++)
            {
                p.y = p.y + Q[j][i].y;
            }
            p.y = terrain.get(p.x, p.z) + (p.y / tot_amp);
            if(p.y > terrain.max_height)
                p.y = terrain.max_height;
            terrain.set((float)p.x, (float)p.z, p.y);
        }


    }
}
