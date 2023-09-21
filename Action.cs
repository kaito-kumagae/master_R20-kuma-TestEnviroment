using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public void ActionProcess(float[] vectorAction)
    {
        var lastPos = carAgent.transform.position;

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

        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];
        vertical = Mathf.Clamp(vertical, -1.0f, 1.0f);
        horizontal = Mathf.Clamp(horizontal, -1.0f, 1.0f);

        carAgent.MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        float individualReward = rewardCalculation.CalculateIndividualReward();

        var moveVec = carAgent.transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, carAgent._track.forward);
        float angleReward = rewardCalculation.CalculateAngleReward(moveVec, angle, vertical);

        carAgent.AddReward(individualReward + angleReward);

        if (carAgent.foundCarBackward && !carAgent.foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)carAgent.speed, false, vectorAction);
        }
        if (carAgent.foundCarForward && !carAgent.foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)carAgent.speed, true, vectorAction);
        }
        evaluator.addFullData(Time.frameCount, carAgent.transform.position, carAgent.prev_observations, horizontal, vertical);
    }
}