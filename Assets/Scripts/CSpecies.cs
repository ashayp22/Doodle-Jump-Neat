using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSpecies
{

    //keep a local copy of the first member of this species

    private CGenome leader;

    private List<CGenome> vecMembers; //pointers to all the genomes within this species

    private int speciesID; //id number of species

    private double bestFitness; //best fitness found so far by this species

    private double avgFitness; //average fitness of the species

    //generations since fitness has improved, we can use this info to kill off a species if required
    private int gensNoImprovement;

    private int age; //age of the species

    private double spawnsRequired; //spawns required for the next population

    
    public CSpecies(CGenome first, int id)
    {
        vecMembers = new List<CGenome>();
        speciesID = id;
        leader = first;
        vecMembers.Add(leader);
        gensNoImprovement = 0;
        avgFitness = 0;
    }


    //this method boosts the fitness of the young, penalizes the fitnesses of the old and then performs fitness sharing over all members of the species

    public void AdjustFitnesses()
    {
        double total = 0;
        for(int gen = 0; gen < vecMembers.Count; gen++)
        {
            double fitness = vecMembers[gen].getFitness();

            //boost the fitness scores if the species is young
            if(age < GameManager.YOUNG_BONUS_AGE_THRESHOLD)
            {
                fitness *= GameManager.YOUNG_FITNESS_BONUS;
            }

            //punish the older species

            if(age > GameManager.OLD_AGE_THRESHOLD)
            {
                fitness *= GameManager.OLD_AGE_PENALTY;
            }

            total += fitness;

            //apply fitness sharing to the adjusted fitnessed
            double AdjustedFitness = fitness / vecMembers.Count;

            vecMembers[gen].setAdjustedFitness(AdjustedFitness);
        }

        double newAvg = total / vecMembers.Count;

        if (avgFitness + 10 >= newAvg)
        {
            gensNoImprovement++;
        }

        avgFitness = newAvg;
    }

    //adds a new individual to the species
    public void AddMember(CGenome new_org)
    {
        vecMembers.Add(new_org);
    }


    public void Purge() //purges entire species except its leader
    {
        int bestIndex = 0;
        double bestFitness = vecMembers[0].getFitness();
        for(int i = 0; i < vecMembers.Count; i++)
        {
            if(vecMembers[i].getFitness() > bestFitness)
            {
                bestFitness = vecMembers[i].getFitness();
                bestIndex = i;
            }
        }

 
        leader = vecMembers[bestIndex]; //sets the leader
        vecMembers = new List<CGenome>(); //purges
        vecMembers.Add(leader); //only leader is there
    }

    //calculates how many offspring this species should spawn
    public void CalculateSpawnAmount(double averagePopulation)
    {
        spawnsRequired = 0;
        foreach(CGenome genome in vecMembers) {
            spawnsRequired += genome.getAdjustedFitness() / averagePopulation;
        }
    }

    //spawns an individual from the species selected at random from the best survival rate
    public CGenome Spawn()
    {
        //chooses tournament selection
        int position = 0;
        double bestFitness = 0;
        for(int i = 0; i < vecMembers.Count/4; i++)
        {
            int newPos = Random.Range(0, vecMembers.Count - 1);
            if(vecMembers[newPos].getAdjustedFitness() > bestFitness)
            {
                position = newPos;
                bestFitness = vecMembers[newPos].getAdjustedFitness();
            }
        }


        return vecMembers[position];
    }


    //accesor methods

    public CGenome getLeader()
    {
        return leader;
    }

    public double NumToSpawn()
    {
        return spawnsRequired;
    }

    public int NumMembers()
    {
        return vecMembers.Count;
    }

    public int GensNoImprovement()
    {
        return gensNoImprovement;
    }

    public int ID()
    {
        return speciesID;
    }

    public double SpeciesLeaderFitness()
    {
        return leader.getFitness();
    }

    public double BestFitness()
    {
        return bestFitness;
    }

    public int Age()
    {
        return age;
    }


}
