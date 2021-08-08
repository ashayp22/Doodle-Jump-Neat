using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))] //require a rigidbody

public class Player : MonoBehaviour
{

    private bool isDead;
    private int score = 0;
    private int number;
    private List<string> platformTouches;

    private List<Vector2> previousLocations;

    //private Line closestLine;
    //public Line linePrefab;

    // Start is called before the first frame update
    void Start()
    {
        isDead = false;
        platformTouches = new List<string>();
        previousLocations = new List<Vector2>();
       // closestLine = Instantiate(linePrefab) as Line;
        //closestLine.transform.parent = this.transform;
    }

    public void setNumber(int i)
    {
        number = i;
    }

    void Update()
    {
        if(isDead)
        {
            this.gameObject.SetActive(false);
            this.transform.position = new Vector2(0, 0);
        }     

        if(!isDead)
        {
            //add the previous locations
            previousLocations.Add(transform.position);
        }

        //remove last in previous locations

        if(previousLocations.Count > 10)
        {
            previousLocations.RemoveAt(0); //removes the oldest position
        }

    }

    void FixedUpdate()
    {
        if(!isDead)
        {
            //moving the doodler

            //going out of bounds and wrapping around
            if (transform.position.x <= -2.5f)
            {
                transform.position = new Vector2(2.5f, transform.position.y);
            }
            else if (transform.position.x >= 2.5f)
            {
                transform.position = new Vector2(-2.5f, transform.position.y);
            }

            //check for getting stuck
            if (previousLocations.Count > 8)
            {
                int times = 0;
                Vector2 loc = previousLocations[previousLocations.Count - 1];

                for (int i = previousLocations.Count - 2; i >= 0; i--)
                {
                    if (previousLocations[i].x == loc.x && previousLocations[i].y == loc.y)
                    {
                        times++;
                    }
                    else
                    {
                        times = 0;
                        loc = previousLocations[i];
                    }

                    if (times == 3) //same spot in 6 frames
                    {
                        Debug.Log("applied");
                        Rigidbody2D doodlerRB = GetComponent<Rigidbody2D>();
                        Vector2 velocity = doodlerRB.velocity;
                        velocity.y = Platform.jumpForce;
                        doodlerRB.velocity = velocity;
                        break;
                    }


                }
            }

            

        }
   
    }

    public void moveDoodler(float movement)
    {
        //changes the velocity based on movement
        if(!isDead) //can't be dead
        {
            Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
            velocity.x = movement;
            GetComponent<Rigidbody2D>().velocity = velocity;
        }
    }

    //dead accesor/mutator
    public bool getIsDead()
    {
        return isDead;
    }

    public void setDead()
    {
        isDead = true;
    }

    public void reset() //resets
    {
        isDead = false;
        gameObject.SetActive(true);
        score = 0;
        platformTouches = new List<string>();
    }

    public void increaseScore()
    {
        score++;
    }

    public int getScore()
    {
        return score;
    }

    public int getNumber()
    {
        return number;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player") //if the collision is with the doodler, ignore
        {
            Physics2D.IgnoreCollision(collision.collider.gameObject.GetComponent<Collider2D>(), GetComponent<Collider2D>());
        }
        else
        {
            if (collision.relativeVelocity.y > 0) //makes sure the doodler is coming from above
            {
                //collided with a platform, change platform list
                platformTouches.Add(collision.collider.name);

                //checks for touching the same platform three times
                string current = platformTouches[0];
                int times = 0;
                for (int i = 1; i < platformTouches.Count; i++)
                {
                    if (platformTouches[i].Equals(current))
                    {
                        times++;
                    }
                    else
                    {
                        times = 0;
                        current = platformTouches[i];
                    }

                    if (times == 1) //touch the same platform 4 times(since the 4 time is when the 3rd is detected)
                    {
                        setDead();
                    }
                }
            }
        }
    }

    public void setClosestLine(Vector2 pos)
    {
        //closestLine.Initialize2(20, transform.position, pos);
    }

}
