using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipStream
{
    private CarAgent carAgent;
    private RewardCalculation rewardCalculation;
    public SlipStream(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.rewardCalculation = carAgent.rewardCalculation;
    }

    public void judgeSlipStream(float distance)
    {
        
    }
}