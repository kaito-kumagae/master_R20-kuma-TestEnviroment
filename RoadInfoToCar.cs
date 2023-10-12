using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class RoadInfoToCar
{
    private CarAgent carAgent;
    private CarInformation carInformation;

    public RoadInfoToCar(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
    }

    public void RecognizeLane()
    {
        var carCenter = carAgent.transform.position + Vector3.up;

        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            if (hit.collider.CompareTag("MainLaneTile"))
            {
                
            }
            else if (hit.collider.CompareTag("MergingLaneTile"))
            {
                
            }
        }
    }
}