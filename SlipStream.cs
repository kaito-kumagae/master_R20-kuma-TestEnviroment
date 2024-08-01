using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlipStream
{
    private CarAgent carAgent;
    private RewardCalculation rewardCalculation;
    public float radius;
    public SlipStream(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.rewardCalculation = carAgent.rewardCalculation;
        this.radius = carAgent.boxCol.size.z/2.0f*carAgent.transform.localScale.z+2.5f;
    }
    public void judgeSlipStream()
    {
        Vector3 centerPosition = carAgent.transform.position;
        Quaternion carOrientation = carAgent.transform.rotation; //new Quaternion(0,0,0,0);
        Vector3 halfExtents = new Vector3(0.8f,0.5f,4.0f);
        Vector3 BoxCenterPosition = centerPositionBox(centerPosition);
        Collider[] hits = Physics.OverlapBox(BoxCenterPosition, halfExtents, carOrientation);
        
        if(carAgent.tag == "TruckCar")
        {
            carAgent.cube.transform.localScale = halfExtents * 2;
            carAgent.cube.transform.localPosition = BoxCenterPosition;
            carAgent.cube.transform.localRotation = carOrientation;
        }
        foreach (Collider hit in hits)
        {
            if
                ((carAgent.tag == "TruckCar" && hit.tag == "car") || 
                (carAgent.tag == "TruckCar" && hit.tag == "TruckCar") || 
                (carAgent.tag == "car" && hit.tag == "car"))
            {
                CarAgent otherAgent = hit.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
                if(otherAgent.foundTrackForward)
                {
                    otherAgent.SlipStreamDistance = Vector3.Distance(carAgent.transform.localPosition, otherAgent.transform.localPosition);
                }
            }
        }
    }

    public Vector3 centerPositionBox(Vector3 centerPosition)
    {
        float angleY = carAgent.transform.eulerAngles.y+180f;
        float radians = angleY * Mathf.Deg2Rad;
        float x = centerPosition.x + radius * Mathf.Sin(radians);
        float z = centerPosition.z + radius * Mathf.Cos(radians);
        // float x = centerPosition.x*Mathf.cos(radians)-centerPosition.z*Mathf.sin(radians);
        // float z = centerPosition.x*Mathf.sin(radians)+centerPosition.z*Mathf.cos(radians);
        return new Vector3(x,centerPosition.y+1.0f,z);
    }
}