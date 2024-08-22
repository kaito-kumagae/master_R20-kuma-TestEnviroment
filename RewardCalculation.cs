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
        float differentTime;
        float differentTimeReward;
        float endZ = 290f;
        float speed = 10f;  // 10 tiles = 10m
        float timePerStep = 0.02f;  // 1 step = 0.02s

        // GoalStepTime: 目標到着ステップ数
        // ActualArrivalTime: 実際の到着ステップ数
        differentTime = carAgent.GoalStepTime - carAgent.ActualArrivalTime;

        float remainingDistance = endZ - carAgent.transform.position.z;  // 残りの距離
        float remainingSteps = (remainingDistance / speed) / timePerStep;  // 残りのステップ数
        
        // ここで differentTime を再計算
        differentTime = remainingSteps - (carAgent.stepTime + carAgent.GoalStepTime);
        
        // 報酬計算
        if (differentTime <= -116f || differentTime >= 116f)
        {
            differentTimeReward = -10f;
        }
        else
        {
            differentTimeReward = -0.00073365f * Mathf.Pow(Mathf.Abs(differentTime), 2) + 10f;
        }

        // デバッグ用出力
        Debug.Log("remainingSteps : " + remainingSteps);
        Debug.Log("differentTime : " + differentTime + ": step ");
        Debug.Log("differentTimeReward : " + differentTimeReward);

        return differentTimeReward;
    }


    public void setCrashReward(float crashReward)
    {
        carAgent.SetReward(crashReward);
    }
}