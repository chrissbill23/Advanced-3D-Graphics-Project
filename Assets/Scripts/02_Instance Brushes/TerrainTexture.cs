using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum options{
    option_1,
    option_2
}
public class TerrainTexture : TerrainBrush
{
    [Header("Select one of the following options to proceed")]
    public options option = options.option_1;
    [Header("Option 1: Insert the weight and the type of each texture in the espector")]
    public float[] curentWeights = new float[1]{0.0f};
     public Layers[] layersType = new Layers[0];
    [Header("Option 2: Automatic fill by earth level")]
    public Zone zones = Zone.FootHill;

    public override void draw(int x, int z) {
        Vector2 textureSize = terrain.textureSize();
        int amap_width = (int)textureSize.x;
        int amap_height = (int)textureSize.y;
        int tot_layers = (int) terrain.getTextures().Length/(amap_height*amap_width);
        TerrainData data = terrain.getData();
        float[] weights = new float[0];
        if(option == options.option_1){
            if(tot_layers == 0 || layersType.Length != curentWeights.Length){
                terrain.debug.text = "No Layers added or type of layers not specified";
                return;
            }
            weights = curentWeights;
        }
        VegetationConstraint v  = null;
        if(option == options.option_2){
            if(zones != Zone.Random){
                v = terrain.getVegetationFromZone(zones);
                weights = v.layers_weights;
                layersType = v.layers;
                curentWeights = weights;
            } else{
                foreach(int zo in System.Enum.GetValues(typeof(Zone))){
                    if(Zone.Random != (Zone)z){
                        v = terrain.getVegetationFromZone((Zone)zo);
                        weights = v.layers_weights;
                        layersType = v.layers;
                        curentWeights = weights;
                        fill(weights,x,z, amap_width , amap_height, tot_layers, v);
                    }
                }
            }
        }
        if(weights.Length > tot_layers){
            terrain.debug.text = "Added more weights than layers.";
            return;
        }
        
        fill(weights,x,z, amap_width , amap_height, tot_layers,v);
    }
    protected void fill(float[] weights,int x, int z,int amap_width ,int amap_height,int tot_layers, VegetationConstraint v = null){
        float[,,]alphamaps = terrain.getTextures();
        if(!randomPoints){
            for (int h = z-radius; h <= z+radius; h++) {
                for (int w = x-radius; w <= x+radius; w++) {
                    if(h<0 || w<0 || h>=amap_height || w>=amap_width)
                        continue;
                    if(v != null && !v.place(w,h,ref terrain))
                        continue;
                    float maxCoef = 0;
                    int maxIdx = 0;
                    for(int r = weights.Length; r<tot_layers; r++){
                        alphamaps[h, w,  r] = 0.0f;
                    }
                    for(int l = 0; l<weights.Length; l++){
                        alphamaps[h, w,l] = weights[l];
                        if(weights[l] > maxCoef){
                            maxCoef = weights[l];
                            maxIdx = l;
                        }
                    }
                    terrain.setOccupance(h,w, layersType[maxIdx]);
                }
            }
        } else{
            Vector2[] points =  ramdPoints(x, z);
            foreach (Vector2 p in points)
            {
                int h = (int)p.y;
                int w = (int)p.x;
                if(h<0 || w<0 || h>=amap_height || w>=amap_width)
                        continue;
                if(v != null && !v.place(w,h,ref terrain))
                    continue;
                float maxCoef = 0;
                int maxIdx = 0;
                for(int r = weights.Length; r<tot_layers; r++){
                    alphamaps[h, w,  r] = 0.0f;
                }
                for(int l = 0; l<weights.Length; l++){
                    alphamaps[h, w,l] = weights[l];
                    if(weights[l] > maxCoef){
                        maxCoef = weights[l];
                        maxIdx = l;
                    }
                }
                terrain.setOccupance(h,w, layersType[maxIdx]);
            }
        }
        terrain.setTextures(alphamaps);
        terrain.saveTextures();
    }

}
