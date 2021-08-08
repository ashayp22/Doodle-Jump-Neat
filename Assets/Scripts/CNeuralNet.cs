using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNeuralNet
{

    private List<SNeuron> vecpNeurons;

    private int depth;

    public CNeuralNet(List<SNeuron> n, int d)
    {
        vecpNeurons = n;
        depth = d;
    }

    //update network for this clock cycle
    public List<double> Update(List<double> inputs, string runtype) //parameter is 'snapshot' or 'active' 
    {
        //create a vector for the outputs
        List<double> outputs = new List<double>();

        //if the mode is snapshot, then we require all the neurons to be 
        //iterated through as many times as the network is deep. If the 
        //mode is set to active, then the method can return an output after just one iteration

        int FlushCount = 0;

        if(runtype.Equals("snapshot"))
        {
            FlushCount = depth;
        } else
        {
            FlushCount = 1;
        }


        //iterate through the network FlushCount times

        for (int i = 0; i < FlushCount; i++)
        {
            //clear the output vector
            outputs = new List<double>();

            //this is an index into the current neuron
            int cNeuron = 0;

            //first set the outputs of the 'input' neurons to be equal 
            //to the values passed into the function in inputs

            while(vecpNeurons[cNeuron].neuronType.Equals("input"))
            {
                vecpNeurons[cNeuron].dOutput = inputs[cNeuron];
                cNeuron++;
            }

            //set the output of the bias to 1
            vecpNeurons[cNeuron].dOutput = 1;

            //then we step through the network a neuron at a time

            while(cNeuron < vecpNeurons.Count)
            {
                //this will hold the sum of all the inputs * weights
                double sum = 0;

                //sum this neuron's inputs by iterating through all the links into the neuron
                for(int lnk = 0; lnk < vecpNeurons[cNeuron].vecLinksIn.Count; lnk++)
                {
                    //get this link's weight
                    double Weight = vecpNeurons[cNeuron].vecLinksIn[lnk].dWeight;

                    //get the output from the neuron this link is coming from
                    double NeuronOutput = vecpNeurons[cNeuron].vecLinksIn[lnk].pIn.dOutput;

                    //add to sum
                    sum += Weight * NeuronOutput;
                }


                //now we put the sum through the activation function and assign the 
                //value to the neuron's output
                vecpNeurons[cNeuron].dOutput = Sigmoid(sum, vecpNeurons[cNeuron].activationResponse);

                if(vecpNeurons[cNeuron].neuronType.Equals("output"))
                {
                    //add to out outputs
                    outputs.Add(vecpNeurons[cNeuron].dOutput);
                }

                //next neuron
                cNeuron++;
            }

        } //next ieration through the network


        //the network outputs need to be reset if tis type of update is performed
        //otherweise it is possible for dependencies to be built on the order the training data is presented

        if(runtype.Equals("snapshot"))
        {
            for(int n = 0; n < vecpNeurons.Count; n++)
            {
                vecpNeurons[n].dOutput = 0;
            }
        }

        //return the outputs
        return outputs;
    }

    public double Sigmoid(double sum, double activation)
    {
        return 1 / (1 + Mathf.Exp((-1 * (float)sum) / (float)activation));
    }

    //draws the neural network on the screen
    public int getNNsize()
    {
        return vecpNeurons.Count;
    }

    public int getIdFromIndex(int index)
    {
        return vecpNeurons[index].neuronId;
    }

    public Vector2 getNodePosition(int index) //returns the position of a node based on its index
    {
        return new Vector2((float)vecpNeurons[index].SplitX, (float)vecpNeurons[index].SplitY);
    }

    public Vector2 getNodePositionFromID(int id) //returns the position of a node based on its ID
    {
        foreach(SNeuron s in vecpNeurons)
        {
            if(s.neuronId == id)
            {
                return new Vector2((float)s.SplitX, (float)s.SplitY);
            }
        }
        return new Vector2(0, 0);
    }
    
    public List<SLink> getForwardConnections(int index)
    {
        return vecpNeurons[index].vecLinksOut;
    } 

    public string gettypefromid(int id)
    {
        foreach(SNeuron n in vecpNeurons)
        {
            if(n.neuronId == id)
            {
                return n.neuronType;
            }
        }
        return "";
    }

}
