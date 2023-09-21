using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateCarParameters : MonoBehaviour
{
    private CarAgent[] carAgents;
    
    public void RemoveMyIdFromAllcarAgents(int ownId)
    {
        carAgents = GetComponentsInChildren<CarAgent>();
        foreach (CarAgent carAgent in carAgents)
        {
            carAgent.detectedFrontCarIdList.Remove(ownId);
        }
    }
}