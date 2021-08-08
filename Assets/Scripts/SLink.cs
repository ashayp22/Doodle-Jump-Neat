using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SLink
{
    //pointers to the neurons this link connects

    public SNeuron pIn;
    public SNeuron pOut;

    //the connection weights

    public double dWeight;


    //is this link a recurrent link

    public bool recurrent;

    public SLink(double w, SNeuron inn, SNeuron outn, bool r)
    {
        pIn = inn;
        pOut = outn;
        dWeight = w;
        recurrent = r;
    }
}
