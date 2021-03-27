using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Brush : MonoBehaviour {
    public bool randomPoints = true; //Added
    public bool applyOnArea = true; //Added

    protected CustomTerrain terrain;
    private bool active = false;

    public int radius {
        get {
            return terrain.brush_radius;
        }
    }

    void Start() {
        terrain = GetComponent<CustomTerrain>();
    }

    public void deactivate() {
        if (active)
            terrain.setBrush(null);
        active = false;
    }
    public void activate() {
        Brush active_brush = terrain.getBrush();
        if (active_brush)
            active_brush.deactivate();
        terrain.setBrush(this);
        active = true;
    }
    public void toggle() {
        if (isActive())
            deactivate();
        else
            activate();
    }
    public bool isActive() {
        return active;
    }

    public virtual void callDraw(float x, float z) {
        draw(x, z);
    }
    public abstract void draw(float x, float z);
    public abstract void draw(int x, int z);

    protected Vector2[] ramdPoints(float x, float z){
        var random = new System.Random();
        int max_points = applyOnArea ? (int)radius*radius : random.Next((int)radius*radius);
        Vector2[] p = new Vector2[max_points];
        while(max_points>0){
            float angle = (float)(random.NextDouble() * 360);
            int rad = max_points % 2 == 0 ? radius: (int)Mathf.Sqrt(radius*radius + radius*radius);
            float r = (float)(random.NextDouble() * rad);
            float xi = r * (float)(Mathf.Sin(angle));
            float zi = r * (float)(Mathf.Cos(angle));
            p[max_points-1] = new Vector2(x+xi,z+zi);
            max_points-=1;
        }
        return p;
    }
}
