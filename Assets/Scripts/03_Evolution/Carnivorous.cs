using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carnivorous : Animal
{

    protected override void lookAround(int dx, int dy){
        Collider[] l = Physics.OverlapSphere(gameObject.transform.position, 1f);
        foreach(Collider c in l){
                if(gameObject.layer == c.gameObject.layer){
                    Animal a = c.gameObject.GetComponent<Animal>();
                    if(a != null && a.canBeEaten){
                        print(eatPrey(ref a));
                    }
                    
                }
        }
    }
    protected override bool searchFood(int px, int py, float d){
        RaycastHit hitInfo;
        return Physics.SphereCast(transform.position, d, new Vector3((float)px,0f, (float)py), out hitInfo);
    }
    
}
