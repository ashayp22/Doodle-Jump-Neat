using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        this.GetComponent<SpriteRenderer>().color = new Color32((byte)GameManager.colorChosen[0], (byte)GameManager.colorChosen[1], (byte)GameManager.colorChosen[2], 255);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
