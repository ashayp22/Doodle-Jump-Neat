using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CInnovation 
{

    //static class of all the innovation values
    public static List<SInnovation> dataBase = new List<SInnovation>();

     
    public static int CheckInnovation(int input, int output, string type) //checks to see if an innovation exists
    {
        foreach (SInnovation innovation in dataBase)
        {
            if (innovation.sameInputOutput(input, output) && innovation.getInnovationType().Equals(type)) //same innovation
            {
                return innovation.getInnovationNumber(); //returns its id
            }
        }
        return -1;
    }

    public static void CreateNewInnovation(int neuron1, int neuron2, string type, int neuronID, string typeNeuron)
    {
        SInnovation newInnovation = new SInnovation(type, dataBase.Count + 1, neuron1, neuron2, neuronID, typeNeuron); //creates a new innovation that is link
        dataBase.Add(newInnovation);
    }

    public static int GetNeuronId(int id)
    {
        foreach (SInnovation innovation in dataBase)
        {
            if(innovation.getInnovationNumber() == id)
            {
                return innovation.getNeuronId();
            }
        }
        return -1;
    }

    public static int NextNumber()
    {
        return dataBase.Count + 1;
    }

}
