using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;

public class BrainAlpha : Agent
{
    public CarAgent carAgent;
    public float alpha = 0.0f;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        alpha = actionBuffers.ContinuousActions[0];
        alpha = Mathf.Clamp(alpha,0f,1f);
        carAgent.alpha = alpha;
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        //continuousActionsOut[0] = alpha;
        //Debug.Log("test : " + alpha);
    }
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        vectorSensor.AddObservation(carAgent.speed);
        vectorSensor.AddObservation(carAgent.stepTime);
        vectorSensor.AddObservation(carAgent.GoalStepTime);
        vectorSensor.AddObservation(carAgent.differentTime);
        
    }

    public void callSetReward()
    {
        SetReward(carAgent.rewardCalculation.CalculateStepDifferent());
    }
    // public override void OnEpisodeBegin()
    // {vectorSensor.AddObservation(v);
    // }
}