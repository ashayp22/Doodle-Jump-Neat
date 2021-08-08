using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNeuron 
{
    public List<SLink> vecLinksIn = new List<SLink>();
    public List<SLink> vecLinksOut = new List<SLink>();

    //sum of weights x inputs

    public double sumActivation;

    //the output from this neuron

    public double dOutput;

    //what type of neuron this is?
    public string neuronType;

    //id
    public int neuronId;

    //sets the curvature of the sigmoid function
    public double activationResponse;

    //used in visualization of the phenotype
    public int PosX, PosY;
    public double SplitX, SplitY;

    //constructor

    public SNeuron(string type, int id, double y, double x, double ActResponse)
    {
        neuronType = type;
        neuronId = id;
        sumActivation = 0;
        dOutput = 0;
        PosX = 0;
        PosY = 0;
        SplitX = x;
        SplitY = y;
        activationResponse = ActResponse;
    }

    public void addIn(SLink arr)
    {
        vecLinksIn.Add(arr);
    }

    public void addOut(SLink arr)
    {
        vecLinksOut.Add(arr);
    }

    public string gettype()
    {
        return neuronType;
    }

}
