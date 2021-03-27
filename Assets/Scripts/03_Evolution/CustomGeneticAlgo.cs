using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGeneticAlgo : GeneticAlgo
{
    public List<GameObject> vegetationsList;
    public List<GameObject> animalsList = new List<GameObject>();
    protected override List<GameObject> getAllAnimals(){
        return animalsList;
    }
}
