using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CapsuleAutoController : MonoBehaviour {

    public float max_speed = 0.5f;
    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;

    void Start() {
        terrain = Terrain.activeTerrain;
        cterrain = terrain.GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;
    }

    void Update() {
        Vector3 scale = terrain.terrainData.heightmapScale;
        Transform tfm = transform;
        Vector3 v = tfm.rotation * Vector3.forward * max_speed;
        Vector3 loc = tfm.position + v;
        if (loc.x < 0)
            loc.x += width;
        else if (loc.x > width)
            loc.x -= width;
        if (loc.z < 0)
            loc.z += height;
        else if (loc.z > height)
            loc.z -= height;
        loc.y = cterrain.getInterp(loc.x/scale.x, loc.z/scale.z);
        tfm.position = loc;
    }
}
