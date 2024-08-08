using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class Action
{
    private CarAgent carAgent;
    private Evaluator evaluator = Evaluator.getInstance();
    private CarInformation carInformation;
    private RewardCalculation rewardCalculation;

    public Action(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
        this.rewardCalculation = carAgent.rewardCalculation;
    }

    public void ActionProcess(ActionBuffers actionBuffers)
    {
        var lastPos = carAgent.transform.position;
        carAgent.stepTime++;
        //Debug.Log("currentStepTime : " + carAgent.stepTime);
        if (carAgent.generateNew)
        {
            carAgent.time++;
        }

        if (carAgent.id == 0)
        {
            carInformation.CarInformationController(carAgent.stopTime, carAgent.commonRewardInterval);
        }

        if (carInformation.rewardTime >= carAgent.commonRewardInterval && carAgent.canGetCommonReward)
        {
            float commonReward = rewardCalculation.CalculateCommonReward();
            carAgent.AddReward(commonReward);
        }
        else if (!carAgent.canGetCommonReward && carInformation.rewardTime < carAgent.commonRewardInterval)
        {
            carAgent.canGetCommonReward = true;
        }

        float horizontal = actionBuffers.ContinuousActions[0];
        float vertical = actionBuffers.ContinuousActions[1];
        vertical = Mathf.Clamp(vertical, -1.0f, 1.0f);
        horizontal = Mathf.Clamp(horizontal, -1.0f, 1.0f);

        carAgent.movement.MoveCar(horizontal, vertical, Time.fixedDeltaTime);
        carAgent.slipStream.judgeSlipStream();
        float individualReward = rewardCalculation.CalculateIndividualReward();
        float slipStreamReward = rewardCalculation.CalculateSlipStreamReward();
        var moveVec = carAgent.transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, carAgent.currentTrack.forward);
        float angleReward = rewardCalculation.CalculateAngleReward(moveVec, angle, vertical);
        carAgent.AddReward(slipStreamReward);
        carAgent.AddReward(individualReward + angleReward);

        if (carAgent.foundCarBackward && !carAgent.foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)carAgent.speed, false, actionBuffers);
        }
        if (carAgent.foundCarForward && !carAgent.foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)carAgent.speed, true, actionBuffers);
        }
        evaluator.addFullData(Time.frameCount, carAgent.transform.position, carAgent.previousObservations, horizontal, vertical);
    }
}