using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Communication
{
    private CarAgent carAgent;
    Color myColor;
    public Communication(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.myColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
    }

    public void CommunicationCars(ref List<float> observations)
    {
        Vector3 relativeCoordinates = Vector3.zero;
        Vector3 relativeSpeed = Vector3.zero;

        List<List<float>> currentCommunications = new List<List<float>>();
        Vector3 centerPosition = carAgent.transform.position + new Vector3(0f, 0.5f ,0f);
        Collider[] hits = Physics.OverlapSphere(centerPosition, carAgent.communicateDistance);
        int count = 0;
        foreach (Collider hit in hits)
        {
            if (hit.tag != "TruckCar") continue;
            CarAgent otherAgent = hit.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
            if (otherAgent.id != carAgent.id)
            {
                count++;
                float distance = Vector3.Distance(carAgent.transform.position, otherAgent.transform.position);
                if (currentCommunications.Count == carAgent.communicationCarsNum)
                {
                    if (currentCommunications[carAgent.communicationCarsNum-1][0] > distance)
                    {
                        relativeCoordinates = carAgent.transform.localPosition - otherAgent.transform.localPosition;
                        var selfDir = Quaternion.Euler(0, carAgent.torque * carAgent.previousHorizontal * 90f, 0) * (carAgent.transform.forward * carAgent.previousVertical * carAgent.speed);
                        var agentDir = Quaternion.Euler(0, otherAgent.torque * otherAgent.previousHorizontal * 90f, 0) * (otherAgent.transform.forward * otherAgent.previousVertical * otherAgent.speed);
                        relativeSpeed = agentDir - selfDir;
                        currentCommunications[carAgent.communicationCarsNum-1] = new List<float>{distance, relativeSpeed.x, relativeSpeed.z,
                                                                    relativeCoordinates.x, relativeCoordinates.z, otherAgent.speed, otherAgent.torque};
                        currentCommunications.Sort((x, y) => x[0].CompareTo(y[0]));
                    }
                }
                else
                {
                    relativeCoordinates = carAgent.transform.localPosition - otherAgent.transform.localPosition;
                    var selfDir = Quaternion.Euler(0, carAgent.torque * carAgent.previousHorizontal * 90f, 0) * (carAgent.transform.forward * carAgent.previousVertical * carAgent.speed);
                    var agentDir = Quaternion.Euler(0, otherAgent.torque * otherAgent.previousHorizontal * 90f, 0) * (otherAgent.transform.forward * otherAgent.previousVertical * otherAgent.speed);
                    relativeSpeed = agentDir - selfDir;
                    currentCommunications.Add(new List<float>{distance, relativeSpeed.x, relativeSpeed.z,
                                                                    relativeCoordinates.x, relativeCoordinates.z, otherAgent.speed, otherAgent.torque});
                    currentCommunications.Sort((x, y) => x[0].CompareTo(y[0]));
                }
            }
        }

        foreach (List<float> carInfo in currentCommunications)
        {
            Debug.DrawLine(centerPosition, centerPosition-new Vector3(carInfo[3], 0, carInfo[4]), myColor, 0.1f);
            for (int i=1; i < 7; i++)
            {
                observations.Add(carInfo[i]);
            }
        }
        
        if (carAgent.communicationCarsNum - count > 0)
        {
            for (int i=0; i < carAgent.communicationCarsNum - count; i++)
            {
                for (int j=0; j < 6; j++)
                {
                    observations.Add(0);
                }
            }
        }
    }
}