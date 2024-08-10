using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class RewardCalculation
{
    private CarAgent carAgent;
    private CarInformation carInformation;
    private Evaluator evaluator = Evaluator.getInstance();

    public RewardCalculation(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
    }

    public float CalculateIndividualReward()
    {
        float individualReward = 0.0f;
        carAgent.trackRecognition.TrackRecognize();
        if (carAgent.movingPreviousTile)
        {
            individualReward = carAgent.movingPreviousTileReward * carAgent.alpha;
        }
        else if (carAgent.movingForwardTile)
        {
            individualReward = carAgent.movingForwardTileReward;
        }
        else if (carAgent.movingBackwardTile)
        {
            individualReward = carAgent.movingBackwardTileReward;
        }
        else if (carAgent.stayingSameTile)
        {
            individualReward = carAgent.stayingSameTileReward;
        }
        return individualReward;
    }

    public float CalculateCommonReward()
    {
        float commonReward = carInformation.commonReward * carAgent.commonRewardRate;
        carInformation.receivedCommonRewardCarNum++;
        carAgent.canGetCommonReward = false;
        return commonReward;
    }

    public float CalculateAngleReward(Vector3 moveVec, float angle, float vertical)
    {
        float angleReward = ((1f - angle / 90f) * Mathf.Clamp01(Mathf.Max(0, vertical)) + Mathf.Min(0, vertical)) * Time.fixedDeltaTime;
        return angleReward;
    }

    public float CalculateDistanceReward(float distance, float distanceReward, float distanceThreshold)
    {
        if ((0 < distance) && (distance < distanceThreshold))
        {
            return distanceReward;
        }
        return 0.0f;
    }

    public float CalculateSlipStreamReward()
    {
        float slipStreamReward = 0.0f;
        
        if(carAgent.SlipStreamDistance != -1.0f)
        {
            if(carAgent.SlipStreamDistance <= 5.0f)
            {
                slipStreamReward = carAgent.slipStreamReward *(1-carAgent.alpha);
            }
        }
        return slipStreamReward;
    }

    public float CalculateStepDifferent()
    {
        int 
        carAgent.

        return 0 
    }

    public void setCrashReward(float crashReward)
    {
        carAgent.SetReward(crashReward);
    }
}