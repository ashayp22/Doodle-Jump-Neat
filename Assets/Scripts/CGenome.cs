using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGenome
{
    //fields

    private int GenomeId; //id num

    private List<SNeuronGene> vecNeurons; //all the neurons which make up this genome

    private List<SLinkGene> vecLinks; //all the links

    private CNeuralNet Phenotype; //phenotype

    private double fitness; //raw fitness score

    private double adjustedFitness; //after has been placed into species and adjusted accordingly

    private double amountToSpwn; //number of offspring this individual is required to spawn for the next generation

    private int numInputs, numOutputs; //keeps track of num of outputs and inputs

    private int speciesNum; //keeps track of whuch species this genome is in(used for display purposes)


    //static, for the genome ids
    public static int NextGenomeID = 0;


    //constructor

    //creates a minimal genome where there are output and input neurons and every input neuron is connected to each output neuron
    public CGenome(int id, int inputs, int output)
    {
        GenomeId = id;
        numInputs = inputs;
        numOutputs = output;

        //now create the minimal genome where every input is connected to an output

        //create neurons

        vecLinks = new List<SLinkGene>();
        vecNeurons = new List<SNeuronGene>();

        //inputs
        for(int i = 0; i < numInputs; i++)
        {
            vecNeurons.Add(new SNeuronGene(i + 1, 0, i*1, false, "input"));
        }

        //bias
        vecNeurons.Add(new SNeuronGene(inputs + 1, 0, inputs * 1, false, "bias"));

        //outputs

        for(int i = 0; i < numOutputs; i++)
        {
            vecNeurons.Add(new SNeuronGene(inputs + i + 2, 4, i * 1.5, false, "output"));
        }

        //now create the links between them
        for (int i = 0; i < numInputs; i++)
        {
            for(int j = 0; j < numOutputs; j++)
            {
                int innovation = CInnovation.CheckInnovation(i + 1, inputs + j + 2, "link");
                vecLinks.Add(new SLinkGene(i + 1, inputs + j + 2, true, false, Random.Range(0.01f, 1f), innovation)); //adds to vector

                if (innovation == -1)
                {
                    Debug.Log("error innovation not found");
                }
            }
        }

        //every bias to every output

        for(int i = 0; i < numOutputs; i++)
        {
            int innovation = CInnovation.CheckInnovation(inputs + 1, inputs + i + 2, "link");
            vecLinks.Add(new SLinkGene(inputs + 1, inputs + i + 2, true, false, -1, innovation)); //adds to list
        }


    }


    //this constructor creates a genome from a vector of SLinkGenes and a vector of SNeuronGenes and an ID number

    public CGenome(int id, List<SNeuronGene> neurons, List<SLinkGene> links, int inputs, int outputs)
    {
        GenomeId = id;
        numInputs = inputs;
        numOutputs = outputs;
        vecNeurons = neurons;
        vecLinks = links; 
    }

    //temporary constructor
    public CGenome()
    {

    }

    //main methods

    public CNeuralNet CreatePhenotype(int depth) //create a neural network from the genome
    {
        DeletePhenotype(); //make sure there is no existing phenotype for this genome

        //this will hold all the neurons required for the phenotype
        List<SNeuron> vecneurons = new List<SNeuron>();

        //first, create all the required neurons
        for(int i = 0; i < vecNeurons.Count; i++)
        {
            SNeuron pNeuron = new SNeuron(vecNeurons[i].getNeuronType(), vecNeurons[i].getID(), vecNeurons[i].getSplitY(), vecNeurons[i].getSplitX(), vecNeurons[i].getActivation());
            vecneurons.Add(pNeuron);
        }

        //now to create the links

        for(int cGene = 0; cGene < vecLinks.Count; cGene++)
        {
            //make sure that the link gene is enabled before the connection is created
            if(vecLinks[cGene].getIsEnabled())
            {
                //get the pointers to the relevant neurons
                int element = GetElementPos(vecLinks[cGene].getFromNeuron());


                SNeuron FromNeuron = vecneurons[element];

                element = GetElementPos(vecLinks[cGene].getToNeuron());

                SNeuron ToNeuron = vecneurons[element];

                //create a link between those two neurons and assign the weight stored in the gene

                SLink tmpLink = new SLink(vecLinks[cGene].getWeight(), FromNeuron, ToNeuron, vecLinks[cGene].getIsRecurrent());

                //add new links to the neuron
                FromNeuron.addOut(tmpLink);
                ToNeuron.addIn(tmpLink);
            }
        }

        //now the neurons contain all the connectivity information, a neural network may be created from them
        Phenotype = new CNeuralNet(vecneurons, depth);

        return Phenotype;

    }

    public void DeletePhenotype() //deletes the neural network
    {
        Phenotype = null;
    }

    public void AddLink(double MutationRate, double ChanceOfLooped, int NumTrysToFindLoop, int NumTrysToAddLink) //adds a link
    {
        if (Random.Range(0f, 1f) > MutationRate) return; //returns dependent on mut rate

        //define holders for the two neurons being linked. If we find two valid neurons to link these values will become >= 0

        int ID_neuron1 = -1;
        int ID_neuron2 = -1;

        bool recurrent = false; //flag set if a recurrent link is selected to be added

        //frist test to see if an attempt should be made to create a link that loops back into the same neuron
        if(Random.Range(0f, 1f) < ChanceOfLooped)
        {
            //YES: try NumTrysToFindLoop times to find a neuron that is not an input or bias neuron and does not already have a loopback connection
            while(NumTrysToFindLoop > 0)
            {
                Debug.Log("loop: " + NumTrysToFindLoop);
                //grab a random neuron
                int NeuronPos = Random.Range((int)numInputs + 1, vecNeurons.Count - 2);
                Debug.Log("size: " + vecNeurons.Count);
                Debug.Log("chosen: " + NeuronPos);

                //check to make sure the neuron does not already have a loopback link and that it is not an input or bias neuron
                if(!vecNeurons[NeuronPos].getRecurrent() && !vecNeurons[NeuronPos].isBias() && !vecNeurons[NeuronPos].isInput())
                {
                    ID_neuron1 = ID_neuron2 = vecNeurons[NeuronPos].getID();

                    vecNeurons[NeuronPos].setRecurrent();

                    recurrent = true;

                    NumTrysToFindLoop = 0;

                }

                NumTrysToFindLoop--;
            }
            
        } else
        {
            //NO: try to find two unlinked neurons. Make NumTrysToAddLink attempts

            while(NumTrysToAddLink > 0)
            {
                int index1 = Random.Range(0, vecNeurons.Count - 1);
                int index2 = Random.Range(numInputs + 1, vecNeurons.Count - 1);
                ID_neuron1 = vecNeurons[index1].getID();
                ID_neuron2 = vecNeurons[index2].getID();

                if(ID_neuron2 == 2)
                {
                    continue; //try again
                }

                //make sure that these two are not already linked and that they are not the same neuron
                if(!DuplicateLink(ID_neuron1, ID_neuron2) && !(ID_neuron1 == ID_neuron2))
                {
                    if((vecNeurons[index1].getNeuronType().Equals("output")) || linkNoWork(vecNeurons[index1].getSplitX(), vecNeurons[index2].getSplitX())) //the starting neuron is the output, can't happen; also the starting neuron has to be behind the ending neuron
                    {
                        ID_neuron1 = -1;
                        ID_neuron2 = -1;
                    } else
                    {
                        NumTrysToAddLink = 0;
                    }
                } else
                {
                    ID_neuron1 = -1;
                    ID_neuron2 = -1;
                }

                 NumTrysToAddLink--;
            }


        }

        if((ID_neuron1 < 0) || (ID_neuron2 < 0)) //return if unsuccessful in finding a link
        {
            return;
        }

        //check to see if we have already created this innovation(found by another genome)
        int id = CInnovation.CheckInnovation(ID_neuron1, ID_neuron2, "link");

        //is this link recurrent(the x pos will be higher for the first)
        if(vecNeurons[GetElementPos(ID_neuron1)].getSplitX() > vecNeurons[GetElementPos(ID_neuron2)].getSplitX())
        {
            Debug.Log("happened");
            recurrent = true;
        }

        Debug.Log("recuccurent: " + recurrent + " with " + ID_neuron1 + " , " + ID_neuron2);

        if(id < 0) //create a new innovation
        {

            //need to create new
            CInnovation.CreateNewInnovation(ID_neuron1, ID_neuron2, "link", -1, "none");

            //now create the gene
            int linkId = CInnovation.NextNumber(); ; //change here

            SLinkGene newGene = new SLinkGene(ID_neuron1, ID_neuron2, true, recurrent, Random.Range(-1f, 1f), linkId);

            vecLinks.Add(newGene); //adds to the list

        } else
        {
            //the innovation has already been created so all we need to do is create the new gene using existing innovation id

            SLinkGene newGene = new SLinkGene(ID_neuron1, ID_neuron2, true, recurrent, Random.Range(-1f, 1f), id);

            vecLinks.Add(newGene); //adds to the list
        }

        return;

    }

    private bool linkNoWork(double inNX, double outNX) //makes sure that the link is valid
    {
        if(outNX <= inNX) //the out neuron is before the in neuron(makes recurrent, don't want that in this case)
        {
            return true;
        }

        //foreach(SNeuronGene s in vecNeurons) //this makes sure that the links being created are between neurons that are one layer apart
        //{
        //if(s.getSplitX() > inNX && s.getSplitX() > outNX) //the neuron is in between the two neurons being connected, meaning that the link won't be one layer
        //{
        //return true;
        //}

        //}

        return false;
    }


    public void AddNeuron(double MutationRate, int NumTrysToFindOldLink) //add a neuron
    {
        if (Random.Range(0f, 1f) > MutationRate) return; //returns dependent on mut rate

        //if a valid link is found into which to insert the new neuron
        //this value is set to true

        bool bDone = false;

        //this will hold the index into vecLinks of the chosen link gene 
        int ChosenLink = 0;

        //first a link is chosen to split. If the genome is small the code makes sure
        //one of the older links is slit to ensure a chaining effect does not occur.
        //Here, if the genome contains less than 5 hidden neurons it is considered to be too small
        //to select a link at random.

        int SizeThreshold = numInputs + numOutputs + 5;

        if(vecLinks.Count < SizeThreshold)
        {
            while(NumTrysToFindOldLink > 0)
            {
                //choose a link with a bias towards the older links in the genome
                ChosenLink = Random.Range(0, vecLinks.Count - 1 - (int)Mathf.Sqrt(vecLinks.Count));

                //make sure that the link is enabled and that it is not a recurrent link

                int FromNeuron = vecLinks[ChosenLink].getFromNeuron();

                //link is enabled, not recurrent, or has a bias input

                if((vecLinks[ChosenLink].getIsEnabled()) && (!vecLinks[ChosenLink].getIsRecurrent()) && (!vecNeurons[GetElementPos(FromNeuron)].isBias()))
                {
                    bDone = true;

                    NumTrysToFindOldLink = 0;
                }

                NumTrysToFindOldLink--;
            }

            if (!bDone)
            {
                //failed to find a descent link
                return;
            }
        } else
        {
            //the genome is of sufficent size for any link to be acceptable
            while(!bDone)
            {

                ChosenLink = Random.Range(0, vecLinks.Count - 1);

                //make sure that the link is enabled and that it is not a recurrent link or has a BIAS input

                int FromNeuron = vecLinks[ChosenLink].getFromNeuron();

                if ((vecLinks[ChosenLink].getIsEnabled()) && (!vecLinks[ChosenLink].getIsRecurrent()) && (!vecNeurons[GetElementPos(FromNeuron)].isBias()))
                {
                    bDone = true;
                }


            }
        }


        //disable this gene 

        vecLinks[ChosenLink].disableGene();


        //grab the weight from the gene (we want to use this for the weight of one of the new links
        //so the split does not distrub anything the NN may have already learned)

        double OriginalWeight = vecLinks[ChosenLink].getWeight();

        //identify the neurons this link connects
        int from = vecLinks[ChosenLink].getFromNeuron();
        int to = vecLinks[ChosenLink].getToNeuron();

        //calculate the depth and width of the new neuron. We can use the depth to see if the link feeds backwards or forwards

        double newDepth = (vecNeurons[GetElementPos(from)].getSplitY() + vecNeurons[GetElementPos(to)].getSplitY()) / 2;

        double newWidth = (vecNeurons[GetElementPos(from)].getSplitX() + vecNeurons[GetElementPos(to)].getSplitX()) / 2;

        //Now to see if this innovation has been created previously by
        //another memeber of the population

        int id = CInnovation.CheckInnovation(from, to, "neuron");

        /*
         * It is possible for Neat to repeatdly do the folowing
         * 
         * 1. Find a link. Lest say we choose link 1 - 5
         * 2. Disable the link
         * 3. Add a new neuron and two new links
         * 4. The link disabled in Step 2 may be re-enabled when this genome is recombined with a genome that has that link enabled
         * 5. etc etc
         * 
         * Therefore, the following checks to see if a neuron ID is already being used. If it is, the function
         * creates a new innovation for the neuron
         */
        
        if(id >= 0)
        {
            int NeuronId = CInnovation.GetNeuronId(id);

            if(AlreadyHaveThisNeuronId(NeuronId))
            {
                id = -1;
            }
        }

        //since there is already a disabled gene between to and from, the new innovation will be created and not replace the disabled one

        if (id < 0) //this is a new innovation
        {
            int NewNeuronId = CInnovation.NextNumber();
            CInnovation.CreateNewInnovation(from, to, "neuron", NewNeuronId, "hidden");

            SNeuronGene newGene = new SNeuronGene(NewNeuronId, newWidth, newDepth, false, "hidden");

            vecNeurons.Add(newGene); //adds to the list

            //two new link innovations are required, one for each of the new links created when this gene is split


            //--------------------------------------------------------------first link

            int idLink1 = CInnovation.NextNumber();  //change here

            //create the new innovation

            CInnovation.CreateNewInnovation(from, NewNeuronId, "link", -1, "none");

            //create the new gene

            SLinkGene link1 = new SLinkGene(from, NewNeuronId, true, false, 1.0, idLink1); //has the same id as its innovation
            vecLinks.Add(link1);

            //----------------------------------------------------------------second link

            int idLink2 = CInnovation.NextNumber(); //change here

            //create the new innovation

            CInnovation.CreateNewInnovation(NewNeuronId, to, "link", -1, "none");

            SLinkGene link2 = new SLinkGene(NewNeuronId, to, true, false, OriginalWeight, idLink2); //has the same id as its innovation
            vecLinks.Add(link2);

        } else //existing innovation
        {
            //this innovation has already been created so grab the relevant neuron and link info from the innovation database

            int NewNeuronId = CInnovation.GetNeuronId(id);

            //get the innovation IDs for the two new link genes
            int idLink1 = CInnovation.CheckInnovation(from, NewNeuronId, "link");
            int idLink2 = CInnovation.CheckInnovation(NewNeuronId, to, "link");

            //this should never happen because the innovations *should* have already occured

            if((idLink1 < 0) || (idLink2 < 0))
            {
                Debug.Log("error: add node cgenome problem");
                return;
            }

            //now need to create 2 new genes to represent the new links 

            SLinkGene link1 = new SLinkGene(from, NewNeuronId, true, false, 1.0, idLink1);
            SLinkGene link2 = new SLinkGene(NewNeuronId, to, true, false, OriginalWeight, idLink2);

            vecLinks.Add(link1);
            vecLinks.Add(link2);

            //create the new neuron

            SNeuronGene gene = new SNeuronGene(NewNeuronId, newWidth, newDepth, false, "hidden");

            //and add it

            vecNeurons.Add(gene);
        }

        return;
    }

    public void MutateWeights(double mut_rate, double prob_new_mut, double MaxPertubation) //mutates the connection weights
    {
        foreach(SLinkGene link in vecLinks)
        {
            if(Random.Range(0f, 1f) < mut_rate)
            {
                if(Random.Range(0f, 1f)  < prob_new_mut) //changes the weight
                {
                    link.changeWeight(Random.Range(-0.4f, 0.4f));
                } else
                {
                    link.changeWeight(link.getWeight() + Random.Range(-1 * (float)MaxPertubation, (float)MaxPertubation));
                }
            }
        }
    }


    public void MutateActivationResponse(double mut_rate, double MaxPertubation) //changes the activation responses of the neurons
    {
        foreach(SNeuronGene gene in vecNeurons)
        {
            if(Random.Range(0f, 1f) < mut_rate)
            {
                //gene.mutateActivationResponse(MaxPertubation); //don't mutate activation
            }
        }
    }

    public double GetCompatibilityScore(CGenome genome) //calculates the cinoatibility score bwteen two genomes
    {
        //travel down the length of each genome counting the number of disjoint genes, the number of excess genes and the number of matched genes
        double NumDisjoint = 0;
        double NumExcess = 0;
        double NumMatched = 0;

        //this records the summed difference of weights in matched genes
        double WeightDifference = 0;

        //indexes in each genome. They are incremented as we step down each genomes length

        int g1 = 0;
        int g2 = 0;

        while( (g1 < vecLinks.Count - 1) || (g2 < genome.getNumLinks() - 1))
        {
            //we've reached the end of genome1 but not genome2 so increment the excess score
            if(g1 == vecLinks.Count - 1)
            {
                g2++;
                NumExcess++;

                continue;
            }

            //and vice versa
            if(g2 == genome.getNumLinks() - 2)
            {
                g1++;
                NumExcess++;
                continue;
            }

            //get the innovation numbers for each gene at this point

            int id1 = vecLinks[g1].getInnovationId();
            int id2 = genome.getSLinkGene(g2).getInnovationId();

            if(id1 == id2) //same innovation number
            {
                //get the weight difference between these two genes

                WeightDifference += Mathf.Abs((float)vecLinks[g1].getWeight() - (float)genome.getSLinkGene(g2).getWeight());

                g1++;
                g2++;
                NumMatched++;
            }

            //innovation numbers are different so increment the disjoint score
            if(id1 < id2)
            {
                NumDisjoint++;
                g1++;
            }

            if(id1 > id2)
            {
                NumDisjoint++;
                g2++;
            }


        } //end while

        //get the length of the longest genome

        int longest = genome.getNumLinks() + genome.getNumGenes();

        if(getNumGenes() + getNumLinks() > longest)
        {
            longest = getNumGenes() + getNumLinks();
        }

        //these are the multipliers used to tweak the final score

        double mDisjoint = 1;
        double mExcess = 1;
        double mMatched = 0.4;

        if(NumMatched == 0)
        {
            NumMatched = 0.01;
        }

        double score = (mExcess * NumExcess / (double)longest) + (mDisjoint * NumDisjoint / (double)longest) + (mMatched * WeightDifference / NumMatched);

        return score;

    }




    //methods on the side

    private bool DuplicateLink(int NeuronIn, int NeuronOut) //returns true if the specified link is already part of the genome
    {
        foreach(SLinkGene link in vecLinks)
        {
            if(link.sameLink(NeuronIn, NeuronOut)) //if it is the same link
            {
                return true;
            }
        }

        return false;
    }


    private int GetElementPos(int neuron_id) //returns the position of a neuron id
    {
        for(int i = 0; i < vecNeurons.Count; i++)
        {
            if(vecNeurons[i].getID() == neuron_id)
            {
                return i;
            }
        }
        return -1;
    }

    private bool AlreadyHaveThisNeuronId(int ID) //tests if the passed ID is the same as any existing neuron IDs, used in AddNeuron
    {
        foreach(SNeuronGene gene in vecNeurons)
        {
            if(gene.getID() == ID)
            {
                return true;
            }
        }
        return false;
    }


    //accessors

    public double getFitness()
    {
        return fitness;
    }

    public double getAdjustedFitness()
    {
        return adjustedFitness;
    }

    public int getNumGenes()
    {
        return vecNeurons.Count;
    }

    public int getNumLinks()
    {
        return vecLinks.Count;
    }


    public SLinkGene getSLinkGene(int i)
    {
        return vecLinks[i];
    }

    public SNeuronGene GetSNeuronGene(int i)
    {
        return vecNeurons[i];
    }

    public int SNeuronGenePos(int neuronID)
    {
        for(int i = 0; i < vecNeurons.Count; i++)
        {
            if(vecNeurons[i].getID() == neuronID)
            {
                return i;
            }
        }
        return -1;
    }

    public int getNumInputs()
    {
        return numInputs;
    }

    public int getNumOutputs()
    {
        return numOutputs;
    }

    public void setFitness(double fit)
    {
        fitness = fit;
    }

    public void setAdjustedFitness(double fit)
    {
        adjustedFitness = fit;
    }

    public int getGenomeID()
    {
        return GenomeId;
    }

    public void setGenomeID(int newid)
    {
        GenomeId = newid;
    }

    public void SortGenes()
    {
        //sorts the links

        for(int i = 0; i < vecLinks.Count; i++)
        {
            int min = i;
            for(int j = i + 1; j < vecLinks.Count; j++)
            {
                if(vecLinks[j].getInnovationId() < vecLinks[min].getInnovationId())
                {
                    min = j;
                }
            }

            SLinkGene temp = vecLinks[min];
            vecLinks[min] = vecLinks[i];
            vecLinks[i] = temp;
        }

        //sorts the neurons
        for (int i = 0; i < vecNeurons.Count; i++)
        {
            int min = i;
            for (int j = i + 1; j < vecNeurons.Count; j++)
            {
                if (vecNeurons[j].getID() < vecNeurons[min].getID())
                {
                    min = j;
                }
            }

            SNeuronGene temp = vecNeurons[min];
            vecNeurons[min] = vecNeurons[i];
            vecNeurons[i] = temp;
        }
    }

    public List<double> UpdateGenome(List<double> inputs)
    {
        return Phenotype.Update(inputs, "snapshot");
    }

}

