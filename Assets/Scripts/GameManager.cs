using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    //prefabs/instances
    public Player doodlerPrefab;
    private List<Player> doodlerList;

    //platform info
    public Platform platformPrefab;
    private List<Platform> platformList;
    private Vector2 spawnPosition;
    private int platformCount;

    public Text scoreText;
    public Text generationText;

    public Node nodePrefab;
    public Line linePrefab;

    //camera
    public Camera camera;

    //vars
    private int score; //score of the game
    private int bestGen;
    private int highscore;
    private int timer; //timer of the game(equal to num of loops/frames)

    //settings
    public static int YOUNG_BONUS_AGE_THRESHOLD = 10;
    public static double YOUNG_FITNESS_BONUS = 1.2;
    public static int OLD_AGE_THRESHOLD = 50;
    public static double OLD_AGE_PENALTY = 0.7;
    public static int NUM_GENS_ALLOWED_NO_IMPROVEMENT = 3;
    public static int NUM_AI = 30;
    public static double CROSSOVER_RATE = 0.7;
    public static int MAX_PERMITTED_NEURONS = 12;
    public static double CHANCE_ADD_NODE = 0.03;
    public static int NUM_TRYS_TO_FIND_OLD_LINK = 10;
    public static double CHANCE_ADD_LINK = 0.7;
    public static double CHANCE_ADD_RECURRENT_LINK = 0.05;
    public static int NUM_TRYS_TO_FIND_LOOPED_LINK = 0;
    public static int NUM_ADD_LINK_ATTEMPTS = 10;
    public static double MUTATION_RATE = 0.2;
    public static double PROBABILITY_WEIGHT_REPLACED = 0.1;
    public static double MAX_WEIGHT_PERTUBATION = 0.5;
    public static double ACTIVATION_MUTATION_RATE = 0.1;
    public static double MAX_ACTIVATION_PERTUBATION = 0.5;

    public static byte[] colorChosen = { 0, 255, 0, 255 };

    //neat
    private Cga NEAT;

    //Neural network stuff
    public GameObject NNCenter;
    private int topDoodler; //the doodler highest in the air, and whose NN will be displayed


    // Start is called before the first frame update
    void Start()
    {
        //creates everything

        //creates doodlers
        doodlerList = new List<Player>();

        for(int i = 0; i < NUM_AI; i++)
        {
            Player  doodlerInstance = Instantiate(doodlerPrefab) as Player;
            doodlerInstance.transform.position = new Vector2(0, -1.5f);
            doodlerInstance.setNumber(1 + i);
            doodlerInstance.name = "doodler" + (1 + i);
            doodlerList.Add(doodlerInstance);
        }
        topDoodler = 0;


        //creates the starting platform
        platformCount = 0;
        platformList = new List<Platform>();
        spawnPosition = new Vector2(0, -3f);
        //adds starting platform
        Platform p = Instantiate(platformPrefab) as Platform;
        p.transform.position = spawnPosition;
        p.name = "platform" + platformCount;
        platformCount++;
        platformList.Add(p);

        //vars
        score = 0;
        highscore = 0;
        timer = 0;
        bestGen = 1;

        //Neat
        //inputs: x distance to nearest tile, y distance to nearest tile
        NEAT = new Cga(GameManager.NUM_AI, 2, 2);

    }

    void LateUpdate()
    {

        //first, check if any if the doodlers are dead

        for (int i = 0; i < doodlerList.Count; i++)
        {
            if (camera.transform.position.y - doodlerList[i].transform.position.y > 4.25f) //too far down
            {
                doodlerList[i].setDead();
            }
        }


        //second, get the top doodler

        topDoodler = 0;
        float height = doodlerList[topDoodler].transform.position.y;
        //the top doodler is the doodler that is the highest in the air, and isn't dead
        for(int i = 1; i < doodlerList.Count; i++)
        {
            if(!doodlerList[i].getIsDead() && doodlerList[i].transform.position.y > height) 
            {
                height = doodlerList[i].transform.position.y;
                topDoodler = i;
            }
        }


        //third, move the camera
        if (doodlerList[topDoodler].transform.position.y > camera.transform.position.y) //if the doodler is above the camera, move the camera
        {
            score += 4;
            Vector3 newPos = new Vector3(0, doodlerList[topDoodler].transform.position.y, -10f);
            camera.transform.position = newPos;
        }


        if (score > highscore)
        {
            highscore = score;
            bestGen = NEAT.generation;
        }

        //increase the timer & stuckTimer
        timer++;

        //Debug.Log("stuck: " + stuckTimer);

        //fourth, change the display + increase fitnesses for surviving
        increaseAIScores();
        drawNeuralNetwork();
        displayText(); //displays the text

    }

    void FixedUpdate()
    {
        //moving the doodler
        //doodlerInstance.moveDoodler(Input.GetAxis("Horizontal") * 5);

        //Debug.Log(Input.GetAxis("Horizontal") * 5);
             
    }

    private void Update()
    {
        updatePlatforms(); //updates the platforms first

        if (!checkRoundOver()) //round isn't over
        {
            //do the movement for every ai
            for (int i = 0; i < doodlerList.Count; i++)
            {
                if (doodlerList[i].getIsDead())
                {
                    continue; //if it is dead, can't move
                }

                //gets the inputs for the bird
                List<double> inputs = new List<double>();

                //input1: x distance to closest platform above
                //input2: y distance to closest platform above

                int closestPlatform = -1;
                //float distance = 1000000000;
                //for (int j = 0; j < platformList.Count; j++)
                //{
                    //float dX = doodlerList[i].transform.position.x;
                    //float dY = doodlerList[i].transform.position.y;
                    //float pX = platformList[j].transform.position.x;
                    //float pY = platformList[j].transform.position.y;
                    //float d = Mathf.Sqrt(Mathf.Pow(dX - pX, 2) + Mathf.Pow(dY - pY, 2)); //pythagorean thm
                    //if (d < distance && pY > dY) //platform is higher than the doodler
                    //{
                        //distance = d;
                        //closestPlatform = j;
                    //}

                //}

                closestPlatform = 1; //closest platform is the one above the starting

                doodlerList[i].setClosestLine(platformList[closestPlatform].transform.position);

                //now calculates x and y distance, all normalized

                double xDistance = (platformList[closestPlatform].transform.position.x - doodlerList[i].transform.position.x) / 5f;
                double yDistance = (platformList[closestPlatform].transform.position.y - doodlerList[i].transform.position.y) / 5f;

                inputs.Add(xDistance);
                inputs.Add(yDistance);

                List<double> outputs = NEAT.UpdateMember(i, inputs);

                double direction = outputs[1] - outputs[0]; //direction is difference

                if(direction > 0.1)
                {
                    direction = 0.1;
                } else if(direction < -0.1)
                {
                   direction = -0.1;
                }

                //Debug.Log("input y: " + inputs[0] + " Input x: " + inputs[1] + "  output: " + outputs[0] + " output2: " + outputs[1]);

                doodlerList[i].moveDoodler((float)(direction) * 50);

            }

        }
        else //round is over
        {
            Debug.Log("dead");
            //have to restart

            //go through neat
            List<double> fitnesses = new List<double>();
            foreach (Player b in doodlerList)
            {
                fitnesses.Add(b.getScore()); //adds its score
            }

            for(int i = 0; i < fitnesses.Count; i++)
            {
                //Debug.Log(fitnesses[i]);
            }


            NEAT.Epoch(fitnesses); //epoch passes

            reset(); //restarts the game 
        }

        //keycode stuff
        if (Input.GetKeyDown(KeyCode.S)) //skip generation
        {
            manualReset();
        } else if (Input.GetKeyDown(KeyCode.R)) //restart whole thing
        {
            restart();
        }

    }

    private void updatePlatforms()
    {
        //creating the platforms

        while (spawnPosition.y < doodlerList[topDoodler].transform.position.y + 4)
        {
            spawnPosition.y += Random.Range(.5f, 1.5f);
            spawnPosition.x = Random.Range(-1.8f, 1.8f);
            Platform p = Instantiate(platformPrefab) as Platform;
            p.transform.position = spawnPosition;
            p.name = "platform" + platformCount;
            platformCount++;
            platformList.Add(p);
        }

        //deleting the platforms too low

        int i = 0;
        while (i < platformList.Count)
        {
            bool remove = false;
            if (doodlerList[topDoodler].transform.position.y - platformList[i].transform.position.y > 3) //too low
            {
                remove = true;
            }

            if (remove)
            {
                Destroy(platformList[i].gameObject);
                platformList.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }

    private bool checkRoundOver() //checks if the round is over
    {
        foreach(Player p in doodlerList)
        {
            if(!p.getIsDead()) //at least one player isn't dead
            {
                return false;
            }
        }
        return true;
    }

    private void increaseAIScores() //increases the scores of the ai, every repetition
    {
        foreach(Player p in doodlerList)
        {
            if(!p.getIsDead())
            {
                p.increaseScore();
            }

        }
    }

    private void displayText() //displays the text
    {
        scoreText.text = "Score: " + score + "\nHighscore: " + highscore;
        generationText.text = "Generation: " + NEAT.generation + "\nBest Generation: " + bestGen;
    }

    private void manualReset() //makes every ai dead, manual reset
    {
        foreach (Player b in doodlerList)
        {
            b.setDead();
        }
    }

    private void reset() //resets everything
    {
        //game manager vars

        if (score > highscore)
        {
            highscore = score;
            bestGen = NEAT.generation;
        }

        score = 0;
        timer = 0;
        topDoodler = 0;
        addition = new Vector3(4.0f, 0.5f);
        platformCount = 0;
        

        //now instances

        //moves the camera down
        camera.transform.position = new Vector3(0, 0, -10);

        foreach (Platform b in platformList)
        {
            Destroy(b.gameObject);
        }

        
        //creates the starting platform
        platformList = new List<Platform>();
        spawnPosition = new Vector2(0, -3f);
        //adds starting platform
        Platform p = Instantiate(platformPrefab) as Platform;
        p.transform.position = spawnPosition;
        platformList.Add(p);

        //now the ai
        foreach (Player b in doodlerList) //each bird is in the starting position
        {
            b.transform.position = new Vector2(0, -1.5f);
            b.reset();
        }

    }

    private void restart()
    {
        manualReset();
        score = 0;
        highscore = 0;
        timer = 0;
        bestGen = 1;

        //Neat
        //inputs: x distance to nearest tile, y distance to nearest tile
        NEAT = new Cga(GameManager.NUM_AI, 2, 2);
        reset();
    }



    private Vector3 addition = new Vector3(4.0f, 0.5f);

    private void drawNeuralNetwork()
    {

        if((0.5f + doodlerList[topDoodler].transform.position.y) > addition.y) //if the top doodler is above the NN
        {
            addition = new Vector3(4f, 0.5f + doodlerList[topDoodler].transform.position.y);
        }

        //choose a bird's nn to display
        int selectedAI = topDoodler;

        //destroy any children of the gameboject
        foreach (Transform child in NNCenter.transform)
        {
            Destroy(child.gameObject);
        }

        //now display the nn

        //display the nodes

        for (int i = 0; i < NEAT.getNNNodeSize(selectedAI); i++) //for each of the nodes
        {
            //construct the node
            Node n = Instantiate(nodePrefab) as Node;

            int id = NEAT.getNNId(selectedAI, i);

            n.transform.position = NEAT.getNNNodePosFromID(selectedAI, id);
            n.transform.parent = NNCenter.transform;


            //construct its connections

            List<SLink> connectionList = NEAT.getNNConnections(selectedAI, i);

            foreach (SLink link in connectionList) //creates the lines
            {

                Line line = Instantiate(linePrefab) as Line;

                line.Initialize(link.dWeight, n.transform.position, NEAT.getNNNodePosFromID(selectedAI, link.pOut.neuronId), link.pIn.neuronId, link.pOut.neuronId);
                line.transform.parent = NNCenter.transform;

                line.transform.position += addition;

            }

            n.transform.position += addition;

        }

    }


    //to do:
    //make sure not reuccrent
    //make sure the outputs can't be connected
}
