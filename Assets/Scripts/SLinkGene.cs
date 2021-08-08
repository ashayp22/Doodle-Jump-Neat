using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLinkGene
{

    //the IDs of the two neurons this link connects

    private int FromNeuron, ToNeuron;

    private double weight;

    private bool isEnabled; //flag to indicate if it is enabled

    private bool isRecurrent; //flag to indicate if it is recurrent

    private int InnovationID; //innovation id

    //constructor
    public SLinkGene(int inN, int outN, bool enable, bool rec, double w, int tag)
    {
        FromNeuron = inN;
        ToNeuron = outN;
        weight = w;
        isEnabled = enable;
        isRecurrent = rec;
        InnovationID = tag;
    }

    public SLinkGene() //temporary constructor
    {

    }

    public double getWeight()
    {
        return weight;
    }

    public void changeWeight(double newWeight)
    {
        weight += newWeight;

        if(weight <= -1)
        {
            weight = -1;
        }

        if(weight > 1)
        {
          weight = 1;
        }

    }

    public bool sameLink(int i, int o)
    {
        return (i == FromNeuron) && (o == ToNeuron);
    }
    

    public int getFromNeuron()
    {
        return FromNeuron;
    }

    public int getToNeuron()
    {
        return ToNeuron;
    }

    public bool getIsEnabled()
    {
        return isEnabled;
    }

    public bool getIsRecurrent()
    {
        return isRecurrent;
    }


    public void disableGene()
    {
        isEnabled = false;
    }

    public int getInnovationId()
    {
        return InnovationID;
    }

}
