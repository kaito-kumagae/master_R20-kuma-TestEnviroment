using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class BrainAlpha : Agent
{
    public CarAgent carAgent;
    public float alpha = 0.0f;
    void Update()
    {
        
    }
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        alpha = actionBuffers.ContinuousActions[0];
        carAgent.alpha = alpha;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = alpha;
    }
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        vectorSensor.AddObservation(carAgent.speed);
        vectorSensor.AddObservation(carAgent.stepTime);
        vectorSensor.AddObservation(carAgent.GoalStepTime);
    }

    private void OnEnable()
    {
        carAgent.rewardFlag += callSetReward;
    }

    private void OnDisable()
    {
        carAgent.rewardFlag -= callSetReward;
    }

    private void callSetReward(int flag)
    {
        SetReward(carAgent.rewardCalculation.CalculateStepDifferent())
    }
    // public override void OnEpisodeBegin()
    // {vectorSensor.AddObservation(v);
    // }
}