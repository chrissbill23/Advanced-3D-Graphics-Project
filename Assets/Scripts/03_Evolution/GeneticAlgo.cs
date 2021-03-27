using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GeneticAlgo : MonoBehaviour {

    [Header("Genetic algorithm parameters")]
    public int pop_size = 100;
    public int min_pop_size = 10;
    public GameObject[] prefab;

    [Header("Dynamic elements")]
    public float vegetation_growth_rate = 1.0f;

    [Header("Vegetation (Details) indexes")]
    public int[] vegetations = new int[]{0};//Added

    private float curr_growth;

    private List<GameObject> animals;
    private Dictionary<string,int> typeAnimals;

    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;
    
    void Start() {
        terrain = Terrain.activeTerrain;
        cterrain = GetComponent<CustomTerrain>();

        curr_growth = 0.0f;

        animals = new List<GameObject>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;
        for (int i = 0; i < pop_size; i++) {
            GameObject animal = makeAnimal();
            if (animal.GetComponent<Animal>().checkConstraint())
            {
                animals.Add(animal);
            } else{
                removeAnimal(animal.GetComponent<Animal>());
            }
        }
    }

    void Update() {
        if(animals.Count <= min_pop_size){
            while (animals.Count < pop_size) {
                GameObject animal = makeAnimal();
                if (animal.GetComponent<Animal>().checkConstraint())
                {
                    animals.Add(animal);
                } else{
                    removeAnimal(animal.GetComponent<Animal>());
                }
            }
        }

        updateResources();
        cterrain.debug.text = animals.Count.ToString() + " animals";
    }

    public void updateResources() {//Edited
        System.Random random = new System.Random();
        int index = random.Next(0, vegetations.Length);
        int tot = cterrain.getDetLayers();
        
        if(vegetations[index] >= tot)
            return;
        
        Vector2 detail_sz = cterrain.detailSize();
        int[,] details = cterrain.getDetails(vegetations[index]);
        curr_growth += vegetation_growth_rate;
        Vector2 prop = cterrain.getPropDetAlpha();
        VegetationConstraint v = GetComponent<ObjectsInTerrain>().getVegetationConst(vegetations[index]);
        while (curr_growth > 1.0f) {
            int x = (int)(UnityEngine.Random.value * detail_sz.x);
            int y = (int)(UnityEngine.Random.value * detail_sz.y);
            if(v.enable){
                details[ y,x] = v.getDet(x,y, ref detail_sz, ref cterrain);
                cterrain.saveDetails(vegetations[index],y, x, details[ y,x]);
                //print(" "+details[y,x]+" "+index+" "+vegetations[index]);
            } else{
                details[y,x] = 1;
                cterrain.saveDetails(vegetations[index],y, x, 1);
            }
            curr_growth -= 1.0f;
        }
        cterrain.saveDetails(vegetations[index]);
    }

    public GameObject makeAnimal(Vector3 position) {//Edited
        
        var random = new System.Random();
        int index = random.Next(prefab.Length);
        GameObject prefab2 = prefab[index];
        GameObject animal = Instantiate(prefab2, transform);
        animal.GetComponent<Animal>().setup(cterrain, this);
        animal.transform.position = position;
        animal.transform.Rotate(0.0f, UnityEngine.Random.value * 360.0f, 0.0f);
        return animal;
    }
    public GameObject makeAnimal() {
        Vector3 scale = terrain.terrainData.heightmapScale;
        float x = UnityEngine.Random.value * width;
        float z = UnityEngine.Random.value * height;
        float y = cterrain.getInterp(x/scale.x, z/scale.z);
        return makeAnimal(new Vector3(x, y, z));
    }

    public void addOffspring(Animal parent) {
        GameObject animal = makeAnimal(parent.transform.position);
        animal.GetComponent<Animal>().inheritBrain(parent.getBrain(), true);
        animals.Add(animal);
    }

    public void removeAnimal(Animal animal) {
        animals.Remove(animal.transform.gameObject);
        Destroy(animal.transform.gameObject);
    }

    protected virtual List<GameObject> getAllAnimals(){
        return new List<GameObject>();
    }

}
