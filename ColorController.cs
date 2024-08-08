using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorController : MonoBehaviour
{
    // private float r = 28f;
    // private float g = 39f;
    // private float b = 204f;
    // private float a = 255;

    public void ChangeColor(float speed, float maxSpeed, float minSpeed)
    {
        //b -= (speed - 5) * 10;
        //GetComponent<Renderer>().material.color = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
        float m = (maxSpeed-minSpeed)/4;
        if (speed < (m+minSpeed))
        {
            GetComponent<Renderer>().material.color = Color.red;
        }

        else if ((m+minSpeed) <= speed && speed < (2*m+minSpeed))
        {
            GetComponent<Renderer>().material.color = Color.yellow;
        }

        else if ((2*m+minSpeed) <= speed && speed < (3*m+minSpeed))
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }

        else
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
    }
}
