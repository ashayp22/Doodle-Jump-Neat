using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SNeuronGene
{
    //field

    private int ID;

    private string NeuronType; //input, output, hidden, bias, none

    private bool isRecurrent;

    private double ActivationResponse; //sets the curvature of the sigmoid function

    private double SplitY, SplitX;

    //constructor
    public SNeuronGene(int id, double x, double y, bool r, string type)
    {
        NeuronType = type;
        isRecurrent = r;
        SplitX = x;
        SplitY = y;
        ID = id;
        ActivationResponse = 1;
    }

    //accessor + modifier

    public bool getRecurrent()
    {
        return isRecurrent;
    }

    public bool isBias()
    {
        return NeuronType.Equals("bias");
    }

    public bool isInput()
    {
        return NeuronType.Equals("input");
    }

    public void setRecurrent()
    {
        isRecurrent = true;
    }

    public int getID()
    {
        return ID;
    }

    public double getSplitX()
    {
        return SplitX;
    }

    public double getSplitY()
    {
        return SplitY;
    }

    public string getNeuronType()
    {
        return NeuronType;
    }

    public double getActivation()
    {
        return ActivationResponse;
    }

    public void mutateActivationResponse(double max)
    {
        ActivationResponse += Random.Range((float)(-1 * max), (float)max);
    }

}
