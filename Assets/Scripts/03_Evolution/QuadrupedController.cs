using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class QuadrupedController : MonoBehaviour
{
    // Start is called before the first frame update
    protected Terrain terrain;
    protected CustomTerrain cterrain;
    protected float width, height;
   //public Rigidbody2D body;
    private Animal anim;
    private Quaternion targetRotation;

    void Start() {
        terrain = Terrain.activeTerrain;
        cterrain = terrain.GetComponent<CustomTerrain>();
        width = terrain.terrainData.size.x;
        height = terrain.terrainData.size.z;
        anim = gameObject.GetComponent<Animal>();
    }

    void Update() {
        Vector3 scale = terrain.terrainData.heightmapScale;

        Vector3 v = transform.rotation * Vector3.forward * anim.getSpeed();
        Vector3 loc = transform.position + v;
        loc.y = cterrain.getInterp(loc.x/scale.x, loc.z/scale.z);
        transform.position = loc;
        if(!anim.checkConstraint()){
            anim.die();
            return;
        }
        controllerColliderHit();
    }
    void controllerColliderHit() {
        Collider[] l = Physics.OverlapSphere(gameObject.transform.position, anim.max_vision);
        foreach(Collider c in l){
            if(gameObject.layer == c.gameObject.layer){
                Animal a = c.gameObject.GetComponent<Animal>();
                if(a != null){
                    //anim.eatPrey(ref a);
                    anim.couple(a);
                }
            }
        }
    }
    //Detect collisions between the GameObjects with Colliders attached
   void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //Check for a match with the specified name on any GameObject that collides with your GameObject
        //print (hit);
       
    }
}
