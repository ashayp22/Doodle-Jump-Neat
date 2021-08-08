using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SInnovation
{
    //fields

    private string InnovationType; //neuron or link

    private int InnovationID;

    private int NeuronIn;
    private int NeuronOut;

    private int NeuronID;

    private string NeuronType;


    //constructor

    public SInnovation(string type, int id, int neuronin, int neuronout, int neuronid, string neurontype)
    {
        InnovationID = id;
        InnovationType = type;
        NeuronIn = neuronin;
        NeuronOut = neuronout;
        NeuronID = neuronid;
        NeuronType = neurontype;
    }

    public bool sameInputOutput(int input, int output)
    {
        return (NeuronIn == input) && (NeuronOut == output);
    }

    public string getInnovationType()
    {
        return InnovationType;
    }

    public int getInnovationNumber()
    {
        return InnovationID;
    }

    public int getNeuronId()
    {
        return NeuronID;
    }
}
