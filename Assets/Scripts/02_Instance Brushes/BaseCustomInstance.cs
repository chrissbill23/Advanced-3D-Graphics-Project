using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




public class TextureInfos
{
     private class TextureConstraints{
         public float min_altitude; 
         public float max_altitude; 
         public float max_steepness;
         public TextureConstraints(float mina = 0.0f, float ma = 1.0f, float ms=45.0f){
             min_altitude = mina;
             max_altitude = ma;
             max_steepness = ms;
         }
     }
     private Dictionary<TreeTextures, TextureConstraints> treescontrainst =   new Dictionary<TreeTextures, TextureConstraints>();
     private Dictionary<Layers, TextureConstraints> layerscontrainst =   new Dictionary<Layers, TextureConstraints>();
     public float highest_altitude=8000.0f;
     public TextureInfos(){
         //Trees init
         treescontrainst.Add(TreeTextures.Broadleaf_Mobile,new TextureConstraints(0.0f,0.425f));
         treescontrainst.Add(TreeTextures.Broadleaf_Desktop,new TextureConstraints(0.0f,0.275f));
         treescontrainst.Add(TreeTextures.Palm_Desktop,new TextureConstraints(0.0f,0.275f));
         treescontrainst.Add(TreeTextures.Conifer_Desktop,new TextureConstraints(0.225f,0.6f));

         //Textures Init
     }
     public bool placeTree(ref TreeTextures t, ref CustomTerrain terrain, ref Vector3 p){
         //Check steepness
        TextureConstraints cons = treescontrainst[t];
        return terrain.getSteepness(p.x , p.z) <= cons.max_steepness && p.y >= cons.min_altitude*highest_altitude && p.y <= cons.max_altitude*highest_altitude;
     }
}

public abstract class BaseCustomInstance : InstanceBrush
{
 
    public Zone zone = Zone.FootHill;
    
    public int maxObjects = 10;
    public float min_distance = 0.0f;
    public float max_terrain_steepness = 45.0f;

    protected abstract Vector3 givePos(float x, float z);
    public override void draw(float x, float z)
    {
        var random = new System.Random();
        int tot = terrain.getObjectCount();
        List<TreeInstance> curr = new List<TreeInstance>();
        List<Vector3> currp = new List<Vector3>();
        for(int i = 0; i<tot; i++){
            TreeInstance o = terrain.getObject(i);
            Vector3 v = terrain.getObjectLoc(i);
            if(v.x <= x + radius && v.x >= x - radius && v.z <= z + radius && v.z >= z - radius) {
                curr.Add(o);
                currp.Add(v);
            }
        }

        int left = maxObjects - curr.Count;
        left = maxObjects;
        if(left > 0){
            Array values = Enum.GetValues(typeof(TreeTextures));
            for (int i = 0; i < left; i++) {
                float xi = ((float)random.NextDouble())*2.0f - 1;
                float zi = ((float)random.NextDouble())*2.0f - 1;
                xi = xi*radius;
                zi = zi*radius;
                Vector3 p = givePos(x,z);
                for(int j=0; j<currp.Count; j++){
                    Vector3 d = (p - currp[j]);
                    float D = d.sqrMagnitude;
                    if( D <= min_distance){
                        p = p + (d* (D+0.01f));
                    }
                }
                
                if( p.x <= x + radius && p.x >= x - radius && p.z <= z + radius && p.z >= z - radius) {
                    if (zone == Zone.Random){
                        Zone obs = ((Zone)values.GetValue(random.Next(values.Length)));
                        if(obs == Zone.Random){
                            obs = Zone.FootHill;
                        }
                        zone = obs;
                    }
                    VegetationConstraint v = terrain.getVegetationFromZone(zone);
                    int index = random.Next(0,v.objects.Length);
                    if(v.objects == null || v.objects.Length == 0)
                        return;
                    terrain.object_prefab = v.objects[index];
                    setPrefab(terrain.registerPrefab(terrain.object_prefab));
                    if(v.place(p.x,p.z,ref terrain,true)){
                        currp.Add(p);
                        spawnObject(p.x, p.z);
                    }
                }
                //print("---" + i + 'p'+GetType());
                //terrain.object_prefab;
                //getPrefable, terrain.getObject, terrain.countexistingObejects, check current object index
            }
        }
        
    }
}
