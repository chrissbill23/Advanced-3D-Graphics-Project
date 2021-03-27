using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Animal : MonoBehaviour {
    [Header("General properties")]
    public AnimalType typeOfAnimal = AnimalType.Grounded;
    public bool canBeEaten = false;
    public AnimalFeed foodType = AnimalFeed.Herbivore;
    public int[] foodIndexes = new int[]{0};
    public Zone[] livesIn = new Zone[]{Zone.FootHill};
    public string nameOfTheAnimal = "Unknown";
    private bool genderMale = true;
    private bool currentlyCoupling = false;

    private VegetationConstraint[] zone;
    
    [Header("Biological properties")]
    public float swap_rate = 0.01f;
    public float mutate_rate = 0.01f;
    public float swap_strength = 10.0f;
    public float mutate_strength = 0.5f;
    public float max_angle = 10.0f;
    public float steepnessLimit = 45.0f;//Added
    public float max_speed = 2f;
    public float max_energy = 10.0f;
    public int max_kids = 1;
    public float energy_loss = 0.1f;
    public float energy_gain = 10.0f;
    public float energy_gain_reproduction = 1f;
    public float energy_loss_reproduction = 0.7f;
    private float energy;
    private int currKids = 0;

    public float max_vision = 20.0f;
    public float angle_step = 10.0f;
    public int nb_eyes = 5;

    private int[] network_struct;
    private SimpleNeuralNet brain = null;
    private Transform tfm;
    private CustomTerrain terrain = null;
    private GeneticAlgo genetic_algo = null;
    private int[,] details = null;
    private Vector2 detail_sz;
    private Vector2 terrain_sz;
    private Material mat = null;
    private float[] vision;
    private int stop = 1;

    void Start() {
        vision = new float[nb_eyes];
        // Network: 1 input per receptor, 1 for reproduction, 1 output per actuator
        network_struct = new int[]{nb_eyes, 5, 1};
        energy = max_energy;
        tfm = transform;

        genderMale = UnityEngine.Random.value < 0.5;

        // Renderer used to update animal color
        // Needs to be updated for more complex models
        MeshRenderer renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null){
            mat = renderer.material;
            /*if(genderMale)
                mat.color = Color.blue;
            else
            {
                mat.color = Color.white;
            }*/
        }
        gameObject.layer = LayerMask.NameToLayer("Animal");
        
    }

    void Update() {
        if (brain == null)
            brain = new SimpleNeuralNet(network_struct);
        if (terrain == null)
            return;
        if (details == null) {
            updateSetup();
            return;
        }
        

        energy -= energy_loss;
        int dx = (int)((tfm.position.x / terrain_sz.x) * detail_sz.x);
        int dy = (int)((tfm.position.z / terrain_sz.y) * detail_sz.y);
        // If over grass, eat it, gain energy and spawn offspring
        lookAround(dx,dy);
        /*if (dx >= 0 && dx < details.GetLength(1) &&
            dy >= 0 && dy < details.GetLength(0) &&
            details[dy, dx] > 0) {
            details[dy, dx] = 0;
            energy += energy_gain;
            if (energy > max_energy)
                energy = max_energy;
            genetic_algo.addOffspring(this);
        }*/
        // Die when out of energy
        if (energy < 0) {
            energy = 0.0f;
            genetic_algo.removeAnimal(this);
        }
        if (mat != null)
            mat.color = Color.white * (energy / max_energy);

        // Update receptor
        updateVision();
        // Use brain
        float[] output = brain.getOutput(vision);
        // Act using actuators
        float angle = (output[0] * 2.0f - 1.0f) * max_angle;
        tfm.Rotate(0.0f, angle, 0.0f);
    }

    private void updateVision() {
        float start_angle = -((float)nb_eyes / 2.0f) * angle_step;
        Vector2 ratio = detail_sz / terrain_sz;
        for (int i = 0; i < nb_eyes; i++) {
            Quaternion rot = tfm.rotation * Quaternion.Euler(0.0f, start_angle+angle_step*i, 0.0f);
            Vector3 v = rot * Vector3.forward;
            float sx = tfm.position.x * ratio.x;
            float sy = tfm.position.z * ratio.y;
            vision[i] = 1.0f;
            for (float d = 1.0f; d < max_vision; d += 0.5f) {
                float px = (sx + d * v.x * ratio.x);
                float py = (sy + d * v.z * ratio.y);
                if (px < 0)
                    px += detail_sz.x;
                else if (px >= detail_sz.x)
                    px -= detail_sz.x;
                if (py < 0)
                    py += detail_sz.y;
                else if (py >= detail_sz.y)
                    py -= detail_sz.y;

                //Edited codes
                if(searchFood((int)px,(int)py,d)){
                    vision[i] = d / max_vision;
                    break;
                }
                /*if ((int)px >= 0 && (int)px < details.GetLength(1) &&
                    (int)py >= 0 && (int)py < details.GetLength(0) &&
                    details[(int)py, (int)px] > 0) {
                    vision[i] = d / max_vision;
                    break;
                }*/
            }
        }
    }

    public void setup(CustomTerrain ct, GeneticAlgo ga) {
        terrain = ct;
        genetic_algo = ga;
        updateSetup();
        zone = new VegetationConstraint[livesIn.Length];
        for(int i = 0; i<livesIn.Length; i++)
            zone[i] = terrain.getVegetationFromZone(livesIn[i]);//Added
    }

    private void updateSetup() {
        detail_sz = terrain.detailSize();
        Vector3 gsz = terrain.terrainSize();
        terrain_sz = new Vector2(gsz.x, gsz.z);
        details = terrain.getDetails();
    }

    public void inheritBrain(SimpleNeuralNet other, bool mutate) {
        brain = new SimpleNeuralNet(other);
        if (mutate)
            brain.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
    }
    public SimpleNeuralNet getBrain() {
        if (brain == null)
            brain = new SimpleNeuralNet(network_struct);
        return brain;
    }
    public float getHealth() {
        return energy / max_energy;
    }


    protected virtual void init(){}
    public virtual bool checkConstraint(){
        var terr = Terrain.activeTerrain;
        bool check = terrain.getSteepness(transform.position.x , transform.position.z) <= steepnessLimit && 
        Array.Exists(zone, el => transform.position.y >= el.min_altitude*terrain.max_height && transform.position.y <= el.max_altitude*terrain.max_height) &&
        transform.position.x > 0 && transform.position.x < terr.terrainData.size.x && transform.position.z > 0 && transform.position.z < terr.terrainData.size.z;
        //print(" "+terrain.max_height+" "+transform.position.y+" "+zone.max_altitude+" "+check);
        
        return check;
    }

    protected virtual void lookAround(int dx, int dy){
        // If over grass, eat it, gain energy and spawn offspring
        foreach(int i in foodIndexes){
            if (dx >= 0 && dx < details.GetLength(1) &&
                dy >= 0 && dy < details.GetLength(0) &&
                terrain.getDetails(i, dy, dx)> 0) {
                terrain.saveDetails(i,dy, dx ,0);
                energy += energy_gain;
                if (energy > max_energy)
                    energy = max_energy;
                genetic_algo.addOffspring(this);
                terrain.saveDetails(i);
            }

        }
    }
    public virtual void die(){
            energy = 0.0f;
            currKids = 0;
            genetic_algo.removeAnimal(this);
    }
    public int getKids(){
        return currKids;
    }
    public float getSpeed(){
        return max_speed*getHealth()*stop*Time.deltaTime;
    }
    public void setStop(int value = 0){
        stop = value;
    }
    public virtual void couple(Animal anim){
        if(anim.nameOfTheAnimal != "Unknown" && anim.nameOfTheAnimal == nameOfTheAnimal 
        && anim.genderMale != genderMale && energy >= energy_loss_reproduction && !currentlyCoupling && !anim.currentlyCoupling){
            currentlyCoupling = true;
            anim.currentlyCoupling =true;
            if(anim.currKids >= max_kids || currKids >= max_kids){
                if(anim.currKids >= anim.max_kids){
                  anim.die();  
                }else
                   die(); 
                return;
            }
            setStop();
            anim.setStop();
            if(getHealth() > anim.getHealth() && currKids > anim.getKids()){
                genetic_algo.addOffspring(this);
            } else{
                genetic_algo.addOffspring(anim);
            }
            currKids += 1;
            anim.currKids = anim.currKids +1;
            energy = energy - energy_loss_reproduction + energy_gain_reproduction;
            anim.energy = anim.energy - anim.energy_loss_reproduction + energy_gain_reproduction;
            if(anim.energy <= 0)
                anim.die();
            if(energy <= 0)
                die();
            setStop(1);
            anim.setStop(1);
            currentlyCoupling = false;
            anim.currentlyCoupling =false;
        }
    }
    public virtual bool eatPrey(ref Animal anim){
        if(foodType != AnimalFeed.Herbivore){
            if (energy < 0) {
                energy = 0.0f;
                genetic_algo.removeAnimal(this);
                return false;
            }
            if (mat != null)
                mat.color = Color.white * (energy / max_energy);
            if(anim.name != "Unknown" && anim.nameOfTheAnimal != nameOfTheAnimal && energy > energy_loss && anim.canBeEaten){
                setStop();
                anim.setStop();
                energy += energy_gain;
                if (energy > max_energy)
                    energy = max_energy;
                genetic_algo.addOffspring(this);
                anim.die();
                setStop(1);
                return true;
            }
            return false;
        }
        return true;
    }
    protected virtual bool searchFood(int px, int py, float d){
        foreach(int j in foodIndexes){
            if ((int)px >= 0 && (int)px < details.GetLength(1) &&
                (int)py >= 0 && (int)py < details.GetLength(0) &&
                terrain.getDetails(j,(int)py, (int)px)> 0) {
                return true;
            }
        }
        return false;
    }

}
