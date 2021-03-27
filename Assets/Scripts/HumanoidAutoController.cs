using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Action{
    Walk,
    Run,
    Jump,
    Fly, 
    GoBack,
    Rest
}
public enum Objects{
    Food,
    Other
}
[RequireComponent(typeof(CharacterController))]
public class HumanoidAutoController : MonoBehaviour {

    
    [Header("General properties")]
    public AnimalType typeOfAnimal = AnimalType.Grounded;
    public int[] foodIndexes = new int[]{0};
    public Zone[] livesIn = new Zone[]{Zone.FootHill};
    public string nameOfTheAnimal = "Unknown";
    private bool genderMale = true;

    private VegetationConstraint[] zone;
    
    [Header("Biological properties")]
    public float swap_rate = 0.01f;
    public float mutate_rate = 0.01f;
    public float swap_strength = 10.0f;
    public float mutate_strength = 0.5f;
    public float max_angle = 9.0f;
    public float steepnessLimit = 45.0f;//Added
    public float max_speed = 2f;
    public float max_energy = 10.0f;
    public float energy_loss = 0.1f;
    public float energy_gain = 10.0f;
    public float min_dist = 1f;


    private int[] network_struct;
    private SimpleNeuralNet brain = null;
    private Animator anim;
    private float angle;
    private Vector3 previous_location;
    private float wmagn = 1.0f;
    private int input = 3;
    private float energy;
    private Action previousAction;
    protected CustomTerrain cterrain;
    protected Terrain terrain;
    private float best_energy = 0;

    void Start() {
        anim = GetComponent<Animator>();
        network_struct = new int[]{input, input*2,input*4,input*2,input, 3};
        energy = max_energy;
        terrain = Terrain.activeTerrain;
        cterrain = terrain.GetComponent<CustomTerrain>();
        previous_location = transform.position;

    }

    void Update() {
        if(brain == null)
            brain = new SimpleNeuralNet(network_struct);
        checkConstraint();
        fly();
    }


    void OnControllerColliderHit(ControllerColliderHit hit) {
        
        angle = Vector3.Angle(Vector3.up, hit.normal);
        if(angle >= 45){
            angle = 45f;
        }
        angle = (angle / 45f);
        wmagn =  (1f-angle);
        RaycastHit hit2;
        float slope = 0f;
        slope = Vector3.Dot(anim.transform.right, (Vector3.Cross(Vector3.up, hit.normal)));
        if(slope >=0f){
            if(wmagn >= 0.8)
                wmagn =1f;
            else
                wmagn+=0.2f;
        }
        print(angle);
    }
    void fly(){
        float[] inp = new float[input];
        inp[0] = transform.position.x;
        inp[1] = transform.position.y;
        inp[2] = transform.position.z;
        float[] o = brain.getOutput(inp);
        Vector3 weights = new Vector3(o[0]*2-1, o[1]*2-1,o[2]*2-1);

        
        anim.SetFloat("InputZ", 1.0f, 0.0f, Time.deltaTime);
        anim.SetFloat("InputMagnitude", 0.6f, 0.0f, Time.deltaTime);
        System.Random rand = new System.Random();
        Vector3 forward = Vector3.forward; // Z direction with respect to the global axis.
        Vector3 right = Vector3.right; // X direction with respect to the global axis.
        Vector3 up = Vector3.up; // Y direction with respect to the global axis.

        forward.Normalize();
        right.Normalize();
        up.Normalize();
        Vector3 desiredMoveDirection = forward * weights.x+ right * weights.z + up * weights.y;
        desiredMoveDirection *= Time.deltaTime*4f;
        print(Quaternion.Angle(transform.rotation,Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), 0.2f)));
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), 0.01f);
        transform.position = transform.position + desiredMoveDirection;
        anim.SetBool("Walk",true);
    }
    void checkConstraint(){
        Collider[] l = Physics.OverlapSphere(anim.transform.position, 1f);
        foreach(Collider c in l){
            if(c.gameObject.tag == "Ground"){
                energy -= energy_loss;
            }
        }
        if(anim.transform.position.y >= terrain.terrainData.size.y|| anim.transform.position.y <= 3f || 
        anim.transform.position.x >= terrain.terrainData.size.x || anim.transform.position.x <= 3f || 
        anim.transform.position.z >= terrain.terrainData.size.z || anim.transform.position.z <= 3f){
            anim.transform.position *=-1f;
            energy -= energy_loss;
        }
        if(energy <= best_energy){
            energy = max_energy;
            brain.mutate(swap_rate, mutate_rate, swap_strength, mutate_strength);
            //anim.transform.position = previous_location;
            //anim.SetBool("Scream",true);
        } else {
            best_energy = energy;
            //anim.SetBool("Scream",false);
        }
    }
}
