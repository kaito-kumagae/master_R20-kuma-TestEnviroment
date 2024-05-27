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

    public void JudgeSlipStream(string tag)
    {
        if (carAgent.foundTruckBackward)
        {
            float carXPosition = carAgent.transform.position.x;
            if (carXPosition >= -3f && carXPosition <= -1f)
            {
                Debug.Log("SlipStream");
                float SlipStreamReward = rewardCalculation.AddSlipStreamReward(carAgent.slipStreamReward);
                carAgent.AddReward(SlipStreamReward);
            }
            else if(carXPosition < -3f && carXPosition > -1f)
            {
                Debug.Log("unSlipStream");
                carAgent.foundTruckBackward = false;
            }
            else if(tag == null)
            {
                Debug.Log("unSlipStream");
                carAgent.foundTruckBackward = false;
            }
        }
    }
}
