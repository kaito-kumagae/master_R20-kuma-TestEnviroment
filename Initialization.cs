using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Initialization
{
    private CarAgent carAgent;

    public Initialization(CarAgent carAgent)
    {
        this.carAgent = carAgent;
    }

    public void Initialize()
    {
        carAgent.rewardCalculation.CalculateIndividualReward();
        carAgent._initPosition = carAgent.transform.localPosition;
        carAgent._initRotation = carAgent.transform.localRotation;
        carAgent.time = 0;
        carAgent.new_id = 0;
    }
}