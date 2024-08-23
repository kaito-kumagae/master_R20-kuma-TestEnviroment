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
        float differentTimeReward;
        float endZ = 290f;
        float timePerStep = 0.02f;  // 1 step = 0.02s

        float remainingDistance = endZ - carAgent.transform.position.z;  // 残りの距離
        float remainingSteps = (remainingDistance / carAgent.speed) / timePerStep;  // 残りのステップ数
        
        // ここで differentTime を再計算
        carAgent.differentTime = (carAgent.stepTime + remainingSteps) - carAgent.GoalStepTime;
        
        // 報酬計算
        if (carAgent.differentTime <= -116f || carAgent.differentTime >= 116f)
        {
            differentTimeReward = -10f;
        }
        else
        {
            differentTimeReward = -0.00073365f * Mathf.Pow(Mathf.Abs(carAgent.differentTime), 2) + 10f;
        }

        float tileIndex = Mathf.Floor(carAgent.transform.position.z / 10f); // 現在のタイルインデックス (0~28)
        float exponent = (tileIndex + 1) / 29f;  // (1/29, 2/29, ..., 29/29)
        float discountFactor = Mathf.Exp(exponent);  // exp(1/29), exp(2/29), ..., exp(29/29)
        float finalReward = differentTimeReward * discountFactor;

        // デバッグ用出力
         //Debug.Log("remainingSteps : " + remainingSteps);
         //Debug.Log("differentTime : " + carAgent.differentTime + ": step ");
         Debug.Log("finalReward : " + finalReward);

        return finalReward;
    }


    public void setCrashReward(float crashReward)
    {
        carAgent.SetReward(crashReward);
    }
}