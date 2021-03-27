using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public abstract class VegetationConstraint : MonoBehaviour
{
    public bool enable = true;
    public int[] detailsIndexes = new int[]{0};
    public GameObject[] objects;
    public float min_altitude = 0.0f;
    public float max_altitude = 1.0f;
    public float min_steepness = 0.0f;
    public float max_steepness = 45.0f;

    public Layers[] terrain_of_growth;
    public Layers[] layers = new Layers[0];
    public bool randomWeights = false;
    public float[] layers_weights = new float[]{0};
    protected Transform tfm;
    protected int grassOffset = 4;
    // Start is called before the first frame update
    void Start()
    {
        tfm = transform;
        setup();
    }

    // Update is called once per frame
    void Update()
    {
        if(randomWeights && layers.Length > 0){
            layers_weights = CustomTerrain.randomWeights(layers.Length);
        }
        updateAll();
    }
    public bool place(float x, float z,ref CustomTerrain terrain, bool det = false){
        float y = terrain.get(x,z);
        bool check = terrain.getSteepness(x , z) <= max_steepness && y >= min_altitude*terrain.max_height && y <= max_altitude*terrain.max_height;
        if(check && det){
            Vector2 prop = terrain.textureSize();//Added
            Layers l = terrain.getOccupance((int)z,(int)x);
            Layers l2 = terrain.getOccupance((int)Mathf.Min(z+grassOffset,prop.y-1),(int)Mathf.Min(x+grassOffset,prop.x-1));
            Layers l3 = terrain.getOccupance((int)Mathf.Max(z-grassOffset,0),(int)Mathf.Max(x-grassOffset,0));
            return Array.Exists(terrain_of_growth, el => el == l && el ==l2 && el==l3);
        }
        return check;
    }
    public int getDet(int x, int y, ref Vector2 detail_sz, ref CustomTerrain cterrain){
        Vector2 prop = cterrain.getPropDetAlpha();//Added
        Layers l = cterrain.getOccupance(y/(int)prop.y,x/(int)prop.x);
        Layers l2 = cterrain.getOccupance((int)Mathf.Min(y+grassOffset,detail_sz.y-1)/(int)prop.y,(int)Mathf.Min(x+grassOffset,detail_sz.x-1)/(int)prop.x);
        Layers l3 = cterrain.getOccupance((int)Mathf.Max(y-grassOffset,0)/(int)prop.y,(int)Mathf.Max(x-grassOffset,0)/(int)prop.x);
        if(place(y,x,ref cterrain) && Array.Exists(terrain_of_growth, el => el == l && el ==l2 && el==l3)){
            return 1;
        }
        return 0;
    }
    protected abstract void setup();
    protected abstract void updateAll();
    public int[] indexes(){
        return detailsIndexes;
    }
}
