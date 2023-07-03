using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialization : MonoBehaviour
{
    private CarAgent carAgent;

    public Initialization(CarAgent carAgent)
    {
        this.carAgent = carAgent;
    }

    public void Initialize()
    {
        carAgent._initPosition = transform.localPosition;
        carAgent._initRotation = transform.localRotation;
        carAgent.time = 0;
        carAgent.new_id = 0;
    }
}