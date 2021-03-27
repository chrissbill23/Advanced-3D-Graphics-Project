using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TreeTextures
     {
         Broadleaf_Mobile,
         Broadleaf_Desktop,
         Palm_Desktop,
         Conifer_Desktop,
         Random
     }
public enum Layers{
    Mud,
    Grass,
    Rocky,
    Cracked,
    Dry,
    Water,
    Snow,
    Cliff,
    Sand,
    Automatic
}
public enum AnimalFeed{
    Carnivorous,
    Herbivore,
    Omnivore
}
public enum AnimalType{
    Grounded,
    FlyingBird,
    Acquatic

}
public class ObjectsInTerrain : MonoBehaviour
{
    [Header("Grounded objects")]
    public TreeTextures[] SpecifyTreesTypes;
    public Layers[] SpecifyTerrainLayers;
    // Start is called before the first frame update

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public VegetationConstraint getVegetationConst(int i=0){
        switch (i)
        {
            
            default: return GetComponent<GrassConstraint>();
        }
    }
    
}
