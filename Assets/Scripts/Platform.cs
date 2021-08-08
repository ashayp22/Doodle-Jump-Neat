using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{

    public static float jumpForce = 9f;

    void OnCollisionEnter2D(Collision2D collision) //adds a force to the doodler that falls on the platform
    {

        if (collision.relativeVelocity.y <= 0) //makes sure the doodler is coming from above
        {
            //Debug.Log("collided");

            Rigidbody2D doodlerRB = collision.collider.GetComponent<Rigidbody2D>();


            if (doodlerRB != null) //changes the velocity, since force vectors makes calculations complicated
            {
                Vector2 velocity = doodlerRB.velocity;
                velocity.y = jumpForce;
                doodlerRB.velocity = velocity;
            }
        }
    }
}
