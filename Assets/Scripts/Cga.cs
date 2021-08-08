using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//README:

//code author: Ashay Parikh
//code language: C#
//sources/help:  AI Techniques for Game Programming by Mat Buckland (some source code in C++)
//Evolving Neural Networks through Augmenting Topologies by Kenneth O. Stanley and Risto Miikkulainen
//please cite above authors if using the code

/*//NEAT SETTINGS NORMALLY
    public static int YOUNG_BONUS_AGE_THRESHOLD = 10;
    public static double YOUNG_FITNESS_BONUS = 1.2;
    public static int OLD_AGE_THRESHOLD = 50;
    public static double OLD_AGE_PENALTY = 0.7;
    public static int NUM_GENS_ALLOWED_NO_IMPROVEMENT = 3;
    public static int NUM_BIRDS = 20;
    public static double CROSSOVER_RATE = 0.7;
    public static int MAX_PERMITTED_NEURONS = 12;
    public static double CHANCE_ADD_NODE = 0.25;
    public static int NUM_TRYS_TO_FIND_OLD_LINK = 10;
    public static double CHANCE_ADD_LINK = 0.3;
    public static double CHANCE_ADD_RECURRENT_LINK = 0.05;
    public static int NUM_TRYS_TO_FIND_LOOPED_LINK = 0;
    public static int NUM_ADD_LINK_ATTEMPTS = 10;
    public static double MUTATION_RATE = 0.2;
    public static double PROBABILITY_WEIGHT_REPLACED = 0.1;
    public static double MAX_WEIGHT_PERTUBATION = 0.5;
    public static double ACTIVATION_MUTATION_RATE = 0.1;
    public static double MAX_ACTIVATION_PERTUBATION = 0.5;
*/

/*when will this class be used:
 * declare and initialize
 * update the population(move in game)
 * get the generation number
 * epoch after the population dies
 * displaying the nodes
 * code for it
 *for (int i = 0; i < NEAT.getNNNodeSize(selectedBird); i++) //for each of the nodes
       {
           //construct the node
           Node n = Instantiate(nodePrefab) as Node;

           int id = NEAT.getNNId(selectedBird, i);

           n.transform.position = NEAT.getNNNodePosFromID(selectedBird, id);
           n.transform.parent = NNCenter.transform;


           //construct its connections

           List<SLink> connectionList = NEAT.getNNConnections(selectedBird, i);

           foreach(SLink link in connectionList) //creates the lines
           {

               Line line = Instantiate(linePrefab) as Line;

               line.Initialize(link.dWeight, n.transform.position, NEAT.getNNNodePosFromID(selectedBird, link.pOut.neuronId));
               line.transform.parent = NNCenter.transform;

               line.transform.position += addition;

           }

           n.transform.position += addition;

       } 
 * 
 *
*/

public class Cga //manipulates the species, genomes and innovations
{
    //fields

    private List<CGenome> vecGenomes;

    private List<CSpecies> vecSpecies;

    private List<CNeuralNet> vecNeuralNet;

    public int generation = 1;

    //constructor
    public Cga(int numAI, int inputs, int outputs) //parameters: number of ai/members of the population, the number of inputs for the NN, and the number of outputs for the NN
    { 
        vecGenomes = new List<CGenome>();

        vecSpecies = new List<CSpecies>();

        vecNeuralNet = new List<CNeuralNet>();

        //creates the appropirate innovations

        //neuron innovations
        for(int i = 0; i < inputs; i++)
        {
            CInnovation.CreateNewInnovation(-1, -1, "neuron", i + 1, "input"); //adds innovation
        }

        //bias
        CInnovation.CreateNewInnovation(-1, -1, "neuron", inputs + 1, "bias");

        for (int i = 0; i < outputs; i++)
        {
            CInnovation.CreateNewInnovation(-1, -1, "neuron", inputs + i + 2, "output"); //adds innovation
        }

        //link innovations

        for (int i = 0; i < inputs; i++) //every input is connecting to an output
        {
            for(int j = 0; j < outputs; j++)
            {
                CInnovation.CreateNewInnovation(i + 1, inputs + j + 2, "link", -1, "none");
            }
        }

        for (int j = 0; j < outputs; j++)
        {
            CInnovation.CreateNewInnovation(inputs + 1, inputs + j + 2, "link", -1, "none");
        }

        //creates the ai and their neural networks
        for (int i = 0; i < numAI; i++)
        {
            vecGenomes.Add(new CGenome(CGenome.NextGenomeID++, inputs, outputs)); //adds the new genome
            vecNeuralNet.Add(vecGenomes[i].CreatePhenotype(CalculateNetDepth(vecGenomes[i]))); //creates its phenotype
        }


    }


    //methods

    //called when all the members of the population is dead
    public void Epoch(List<double> FitnessScores)
    {
        //pre-condition: A list representing the fitness scores of each member of the population, is passed in(must be double)
        //post-condition: an epoch will occur and the genetic algorith/NEAT will work: mate the best birds and mutate the babies, and create its data
        
        //first check to make sure we have the correct amount of fitness scores
        if(FitnessScores.Count != vecGenomes.Count)
        {
            Debug.Log("EPOCH NOT ENOUGH FITNESS");
        }

        //first of all, any phnotypes created during the previous generation are deleted. The program then examines each species
        //in turn and deletes all of its members apart from the best performing one. If a species hasn't made any fitness improvements, the species is killed off

        ResetAndKill();

        //update the genomes with the fitnesses scored in the last run
        for(int gen = 0; gen < vecGenomes.Count; gen++)
        {
            vecGenomes[gen].setFitness(FitnessScores[gen]);
        }

        //sort genomes and keep a record of the best performers
        SortAndRecord();

        //separate the population into species of similar topology, adjust fitnesses and calculate spawn levels
        SpeciateAndCalculateSpawnLevels();

        //continue with epoch method...


        //holds the new population of genomes
        List<CGenome> newPopulation = new List<CGenome>();

        //request the offspring from each species. The number of children 
        //to spawn is a double which we need to convert into an int
        int NumSpawnedSoFar = 0;

        CGenome baby = new CGenome();


        //now to iterate though each species selecting offspring to be mated and mutated
        for(int spc = 0; spc < vecSpecies.Count; spc++)
        {
            //becuase of the number to spawn from each species is a double
            //rounded down or up to an integer may cause an overflow of genomes spawned
            //this makes sure it doesn't happen

            if(NumSpawnedSoFar < GameManager.NUM_AI)
            {
                //this is the amount of offspring this species is requred to spawn
                //rounded simply rounds the double up or down
                int NumToSpawn = (int)Mathf.Round((float)vecSpecies[spc].NumToSpawn());

                //Debug.Log("num to spawn: " + NumToSpawn + "    " + spc);
               // Debug.Log("leader fitmess: " + vecSpecies[spc].getLeader().getFitness());

                bool ChosenBestYet = false;

                while(NumToSpawn > 0)
                {
                    //first grab the best preforming genome from this species
                    //and transfer to the new population without mutation
                    //this provides per species elitism

                    if(!ChosenBestYet)
                    {
                        baby = vecSpecies[spc].getLeader();
                        ChosenBestYet = true;
                    } else
                    {
                        //if the number of individuals in this species is only one
                        //then we can only perform mutation
                        if(vecSpecies[spc].NumMembers() == 1)
                        {
                            //spawn a child
                            baby = vecSpecies[spc].Spawn();
                        } else
                        {
                            //if greater than one we can use the crossover operator

                            //spawn1
                            CGenome g1 = vecSpecies[spc].Spawn();

                            if (Random.Range(0f, 1f) < GameManager.CROSSOVER_RATE)
                            {

                                //spawn2, make sure it is not the same as g1
                                CGenome g2 = vecSpecies[spc].Spawn();

                                //number of attempts at finding a different genome
                                int NumAttempts = 5;

                                while ((g1.getGenomeID() == g2.getGenomeID()) && (NumAttempts > 5)) {
                                    g2 = vecSpecies[spc].Spawn();
                                    NumAttempts--;
                                }

                                if (g1.getGenomeID() != g2.getGenomeID())
                                {
                                    baby = Crossover(g1, g2);
                                }

                            }  else
                            {
                                baby = g1;
                            }

                        }

                        CGenome.NextGenomeID++;

                        baby.setGenomeID(CGenome.NextGenomeID);
                        //now we have a spawned child lets mutate it! First there is a chance a neuron may be added

                        if(baby.getNumGenes() < GameManager.MAX_PERMITTED_NEURONS)
                        {
                            baby.AddNeuron(GameManager.CHANCE_ADD_NODE, GameManager.NUM_TRYS_TO_FIND_OLD_LINK);
                        }

                        //nows there's a chance a link may be added
                        baby.AddLink(GameManager.CHANCE_ADD_LINK, GameManager.CHANCE_ADD_RECURRENT_LINK, GameManager.NUM_TRYS_TO_FIND_LOOPED_LINK, GameManager.NUM_ADD_LINK_ATTEMPTS);

                        //mutate the weights
                        baby.MutateWeights(GameManager.MUTATION_RATE, GameManager.PROBABILITY_WEIGHT_REPLACED, GameManager.MAX_WEIGHT_PERTUBATION);

                        //mutate the activation response
                        baby.MutateActivationResponse(GameManager.ACTIVATION_MUTATION_RATE, GameManager.MAX_ACTIVATION_PERTUBATION);
        
                    }

                    //sort the babies genes by their innvovation numbers
                    baby.SortGenes();

                    //add to new pop
                    newPopulation.Add(baby);

                    NumSpawnedSoFar++;

                    if (NumSpawnedSoFar == GameManager.NUM_AI)
                    {
                        NumToSpawn = 0;
                    }

                    NumToSpawn--;
                } //end while

            } //end if
        } //next species

        //if there is an underflow due to a rounding error when adding up all the species spawn amounts, and the amount of offspring falls short of
        //the population size, additional children need to be created and added to the new population. This is achieved simply, by using tournament selection
        //over the entire population

        if(NumSpawnedSoFar < GameManager.NUM_AI)
        {
            //calculate the amount of additional children required
            int Rqd = GameManager.NUM_AI- NumSpawnedSoFar;

            while(Rqd > 0)
            {
                newPopulation.Add(TournamentSelection(GameManager.NUM_AI / 2));
                Rqd--;
            }
        }

        //replaces the current population with the new one
        vecGenomes = newPopulation;

        //create the phenotypes
        List<CNeuralNet> new_phenotypes = new List<CNeuralNet>();

        for(int gen = 0; gen < vecGenomes.Count; gen++)
        {
            //calculate the max network depth
            int depth = CalculateNetDepth(vecGenomes[gen]);

            CNeuralNet phenotype = vecGenomes[gen].CreatePhenotype(depth);

            new_phenotypes.Add(phenotype);
        }

        //replaces the current phenotypes with new ones
        vecNeuralNet = new_phenotypes;

        generation++; //one more generation
        
    }

    private CGenome TournamentSelection(int N)
    {
        double BestFitnessSoFar = 0;

        int chosenOne = 0;

        //Select N memebrs from the population at random testing against the best found so far
        for (int i = 0; i < N; i++) {
            int ThisTry = Random.Range(0, vecGenomes.Count - 1);

            if(vecGenomes[ThisTry].getAdjustedFitness() > BestFitnessSoFar)
            {
                chosenOne = ThisTry;

                BestFitnessSoFar = vecGenomes[ThisTry].getAdjustedFitness();
            }
        }

        //return the champion
        return vecGenomes[chosenOne];
    }


    private void ResetAndKill() //any phenotypes created during the previous generation are deleted
    { 
        vecNeuralNet = new List<CNeuralNet>();
    }

    private void SortAndRecord() 
    {
        //then examines each species in turn and deletes all of its msmbers apart from the best performing one
        //if a species hasn't made any improvment, it is killed off

        foreach(CSpecies species in vecSpecies)
        {
            species.Purge(); //purges except its best genome
        }

        List<int> spotsRemoved = new List<int>();

        for(int i = 0; i < vecSpecies.Count; i++)
        {
            if(vecSpecies[i].GensNoImprovement() >= GameManager.NUM_GENS_ALLOWED_NO_IMPROVEMENT)
            {
                spotsRemoved.Add(i);
            }
        }

        int removed = 0;
        //removes the bad species
        for(int i = 0; i < spotsRemoved.Count; i++)
        {
            //Debug.Log("species removed");
            vecSpecies.RemoveAt(spotsRemoved[i] - removed);
            removed++;
        }
    }

    private void SpeciateAndCalculateSpawnLevels()
    {
        //calculates the compatibility distance of each genome against the representative genome from each live species. 
        //if the value is within a set tolerance, the individual is added to that species
        //if no species match is found, then a new species is created and the genome is added to that

        foreach(CGenome genome in vecGenomes)
        {
            double minimumCompatibility = 100000000;
            int speciesIndex = -1;

            int index = 0;
            foreach(CSpecies species in vecSpecies)
            {
                double current = genome.GetCompatibilityScore(species.getLeader());

                if (current < minimumCompatibility && current <= 0.4) //has to be very closely compatible to count
                {
                    minimumCompatibility = current;
                    speciesIndex = index;
                }
                index++;
            }

            if(speciesIndex != -1) //add to species
            {
                vecSpecies[speciesIndex].AddMember(genome);
            } else //create new species
            {
                vecSpecies.Add(new CSpecies(genome, vecSpecies.Count + 1));
            }

        }


        //then, adjustspeciesfitnesses adjusts and shares the fitness scores

        foreach(CSpecies species in vecSpecies)
        {
            species.AdjustFitnesses(); 
        }

        //next, calculate how many offspring each individual is predicted to spawn in the next generation
        //this is a floating point value calculated by dividing each genomes adjusted fitness score with the average adjusted fitness score of the population

        //calculates the population's average adjusted fitness score
        double averagePopulation = 0;

        foreach(CGenome g in vecGenomes)
        {
            averagePopulation += g.getAdjustedFitness();
        }

        averagePopulation /= vecGenomes.Count;

        //calculates the spawn amount for each species
        foreach(CSpecies species in vecSpecies)
        {
            species.CalculateSpawnAmount(averagePopulation);
        }


    }


    private CGenome Crossover(CGenome mom, CGenome dad) //crossover method
    {
        //first, calculate the genome we will using the disjoint/excess genes from
        //this is the fittst genome. if they are of equal fitnes use the shorter

        int bestParent; //1 is mom, 2 is dad

        if (mom.getFitness() == dad.getFitness())
        {
            //if they are of equal fitness and length just choose one at random
            if (mom.getNumLinks() == dad.getNumLinks())
            {
                bestParent = (int)Random.Range(1, 2);
            }
            else
            {
                if (mom.getNumLinks() < dad.getNumLinks())
                {
                    bestParent = 1;
                }
                else
                {
                    bestParent = 2;
                }
            }
        }
        else
        {
            if (mom.getFitness() > dad.getFitness())
            {
                bestParent = 1;
            }
            else
            {
                bestParent = 2;
            }
        }

        //these vectors will hold the offspring's neurons and genes
        List<SNeuronGene> BabyNeurons = new List<SNeuronGene>();
        List<SLinkGene> BabyGenes = new List<SLinkGene>();

        List<int> vecneurons = new List<int>(); //temporay list to store all added neuron IDs

        //create iterators so we can step through each parents genes and set 
        //them to the first gene of the parent

        int momIterator = 0;
        int dadIterator = 0;

        //this will hold a copy of the gene we wish to add at each step 
        SLinkGene SelectedGene = new SLinkGene();

        //step through each parents genes until we reach the end of both
        while (!((momIterator == mom.getNumLinks()) && (dadIterator == dad.getNumLinks())))
        {

            //the end of mom's genes have been reached
            if ((momIterator == mom.getNumLinks()) && (dadIterator != dad.getNumLinks()))
            {
                //if dad is fittest
                if (bestParent.Equals("dad"))
                {
                    //adds dads genes
                    SelectedGene = dad.getSLinkGene(dadIterator);
                }

                //move onto dad's next gene
                dadIterator++;
            }
            else if ((dadIterator == dad.getNumLinks()) && (momIterator != mom.getNumLinks())) //end of dad's genes
            {
                //if mom is the fittest
                if (bestParent.Equals("mom"))
                {
                    SelectedGene = mom.getSLinkGene(momIterator);
                }

                //move onto next mom
                momIterator++;
            }
            else if (mom.getSLinkGene(momIterator).getInnovationId() < dad.getSLinkGene(dadIterator).getInnovationId())
            {
                //mom's innovation number is less than dads
                //if mom is the fittest
                if (bestParent.Equals("mom"))
                {
                    SelectedGene = mom.getSLinkGene(momIterator);
                }

                //move onto next mom
                momIterator++;
            }
            else if (mom.getSLinkGene(momIterator).getInnovationId() > dad.getSLinkGene(dadIterator).getInnovationId())
            {
                //dad's innovation number is less than moms
                //if dad is fittest
                if (bestParent.Equals("dad"))
                {
                    //adds dads genes
                    SelectedGene = dad.getSLinkGene(dadIterator);
                }

                //move onto dad's next gene
                dadIterator++;

            }
            else if (mom.getSLinkGene(momIterator).getInnovationId() == dad.getSLinkGene(dadIterator).getInnovationId())
            {
                //innovation numbers are the same
                if (Random.Range(0f, 100f) < 50)
                {
                    SelectedGene = dad.getSLinkGene(dadIterator);
                }
                else
                {
                    SelectedGene = mom.getSLinkGene(dadIterator);
                }

                //move onto next gene

                dadIterator++;
                momIterator++;

            }

            //add the selected gene if not already added
            if (BabyGenes.Count == 0)
            {
                BabyGenes.Add(SelectedGene);
            }
            else
            {
                if (BabyGenes[BabyGenes.Count - 1].getInnovationId() != SelectedGene.getInnovationId())
                {
                    BabyGenes.Add(SelectedGene);
                }
            }

            //Check if we already have the neurons refered to in SelectedGene
            //If not, they need to be added
            if (!checkInList(vecneurons, SelectedGene.getFromNeuron()))
                vecneurons.Add(SelectedGene.getFromNeuron());

            if (!checkInList(vecneurons, SelectedGene.getToNeuron()))
                vecneurons.Add(SelectedGene.getToNeuron());


        } //end of while loop



        //now, create the required neurons. First sort them into order

        //quick selection sort
        for (int i = 0; i < vecneurons.Count; i++)
        {
            int min = i;
            for (int j = i + 1; j < vecneurons.Count; j++)
            {
                if (vecneurons[j] < vecneurons[min])
                {
                    min = j;
                }
            }

            int temp = vecneurons[min];
            vecneurons[min] = vecneurons[i];
            vecneurons[i] = temp;
        }

        //add neurons
        for (int i = 0; i < vecneurons.Count; i++)
        {
            //checks the mom
            int neuronID = vecneurons[i];
            int momIndex = mom.SNeuronGenePos(neuronID);
            int dadIndex = dad.SNeuronGenePos(neuronID);

            //checks mom first
            if (momIndex >= 0)
            {
                BabyNeurons.Add(mom.GetSNeuronGene(momIndex)); //adds neuron gene
            }
            else
            {
                BabyNeurons.Add(dad.GetSNeuronGene(dadIndex)); //adds dad's neuron gene
            }
        }

        //finally, create the genome
        CGenome babyGenome = new CGenome(CGenome.NextGenomeID++, BabyNeurons, BabyGenes, mom.getNumInputs(), mom.getNumOutputs());

        return babyGenome; //returns the baby

    }


    private bool checkInList(List<int> vecneurons, int neuron) //checks a neuron to be in a list of neurons
    {
        foreach (int i in vecneurons)
        {
            if (i == neuron)
            {
                return true;
            }
        }
        return false;
    }

    private bool checkInList2(List<double> arr, double val) //checks a double to be in a list of doubles
    {
        foreach (double i in arr)
        {
            if (i == val)
            {
                return true;
            }
        }
        return false;
    }

    private int CalculateNetDepth(CGenome genome) //calculates the depth of the NN
    {
        List<double> yDepths = new List<double>();
        int depth = 0;
        for(int i = 0; i < genome.getNumGenes(); i++)
        {
            double y = genome.GetSNeuronGene(i).getSplitX();
            if(!checkInList2(yDepths, y))
            {
                yDepths.Add(y);
                depth++;
            }
        }
        return depth;
    }


    public List<double> UpdateMember(int index, List<double> inputs) //updates the AI
    {
        //pre-condition: the index of the Ai member(>0) and its inputs are passed in(must be appropriate size)
        //post-condition: a boolean representing if the AI will activate(do something, true) or not(false)
        List<double> outputs = vecGenomes[index].UpdateGenome(inputs);
        return outputs;
    }


    //returns the neural network information of a bird


    public int getNNId(int index, int nodeIndex) //returns the id of a node based on its index and the index(specific neural net) a
    {
        return vecNeuralNet[index].getIdFromIndex(nodeIndex);
    }

    public int getNNNodeSize(int index) //returns the size of a neural network based on index a
    {
        return vecNeuralNet[index].getNNsize();
    }


    public Vector2 getNNNodePosFromID(int index, int id) //returns the position of a node based on its id a
    {
        return vecNeuralNet[index].getNodePositionFromID(id);
    }

    public List<SLink> getNNConnections(int index, int aiIndex) //returns the connections of a ai a
    {
        return vecNeuralNet[index].getForwardConnections(aiIndex);
    }


}
