using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObservations
{

    private CarAgent carAgent;
    private CarInformation carInformation;
    public RewardCalculation rewardCalculation;

    public AddObservations(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
        this.rewardCalculation = carAgent.rewardCalculation;
    }

    public List<float> MakeObservationsList()
    {
        List<float> observations = new List<float>();
        float angle = Vector3.SignedAngle(carAgent._track.forward, carAgent.transform.forward, Vector3.up);
        carAgent.foundCarBackward = false;
        carAgent.foundCarForward = false;
        carAgent.foundCarSide = false;

        observations.Add(angle / 180f);
        
        string tag;
        Vector3 diff;
        int detectedCarId;
        Vector3 otherAgentPosition;
        float distance;
        float distanceReward;

        (float, float, float)[] rayDirections = //ray
        {
            (1.5f, .5f, 25f),     // right forward
            (1.5f, 0f, 0f),      // forward
            (1.5f, -.5f, -25f),  // left forward
            (-1.5f, .5f, 155f),  // right backward
            (-1.5f, 0f, 180f),   // backward
            (-1.5f, -.5f, -155f),// left backward
            (0f, .5f, 90f),      // rightside
            (0f, -.5f, -90f)     // leftside
        };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            distance = ObserveRay(rayDirections[i].Item1, rayDirections[i].Item2, rayDirections[i].Item3, out diff, out tag, out detectedCarId, out otherAgentPosition);
            if (carAgent.needDistanceReward == true)
            {
                distanceReward = rewardCalculation.CalculateDistanceReward(distance, carAgent.distanceReward[0], 0.2f);
                carAgent.AddReward(distanceReward);
            }
            observations.Add(distance);
            observations.Add(diff.x);
            observations.Add(diff.z);
            float FrontRear = rayDirections[i].Item1;
            ObjectObservation(tag, detectedCarId, otherAgentPosition, ref observations, FrontRear);
        }
            observations.Add(carAgent.speed);
            observations.Add(carAgent.torque);

            return observations;
        }

    public void ObjectObservation(string tag, int detectedCarId, Vector3 otherAgentPosition, ref List<float> observations, float FrontRear)
    {   
        if(tag == "car")
        {
            CheckCarInfo(FrontRear);
            if(carAgent.countPassing)
            {
                if(carAgent.foundCarForward)
                {
                    addCarId(detectedCarId, otherAgentPosition);
                }
                else if(carAgent.foundCarBackward)
                {
                    removeCarId(detectedCarId);
                }
            }
        }

    observations.Add(tag == "car" ? 1 : 0);
    observations.Add(tag == "wall" ? 1 : 0);
    }

    public void CheckCarInfo(float FrontRear)
    {
        if(FrontRear > 0)
        {
            carAgent.foundCarForward = true;
        }
        else if(FrontRear < 0)
        {
            carAgent.foundCarBackward = true;
        }
        else
        {
            carAgent.foundCarSide = true;
        }
    }

    private float ObserveRay(float z, float x, float angle, out Vector3 diff, out string tag, out int detectedCarId, out Vector3 otherAgentPosition)
    {

        diff = Vector3.zero;
        tag = "none";
        detectedCarId = -1;
        otherAgentPosition = Vector3.zero;
        var tf = carAgent.transform;

        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        const float RAY_DIST = 5f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
        RaycastHit hit;

        // laser visualization
        Ray ray = new Ray(position, dir);
        Debug.DrawRay(ray.origin, ray.direction*RAY_DIST, Color.red);

        // See if there is a hit in the given direction
        var rayhit = Physics.Raycast(position, dir, out hit, RAY_DIST);
        if (rayhit)
        {
            tag = hit.collider.tag;
            otherAgentPosition = hit.collider.transform.localPosition;
            if (hit.collider.tag == "car"){
                CarAgent agent = hit.collider.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
                detectedCarId = agent.id;
                var self_dir = Quaternion.Euler(0, carAgent.torque * carAgent.prevHorizontal * 90f, 0) * (carAgent.transform.forward * carAgent.prevVertical * carAgent.speed);
                var agent_dir = Quaternion.Euler(0, agent.torque * agent.prevHorizontal * 90f, 0) * (agent.transform.forward * agent.prevVertical * agent.speed);
                diff = agent_dir - self_dir;
            }
        }
        return hit.distance >= 0 ? (hit.distance / RAY_DIST) * Random.Range(1-carAgent.noise, 1+carAgent.noise) : -1f;
    }

    private void addCarId(int detectedCarId, Vector3 otherAgentPosition)
    {
        if (!carAgent.detectedFrontCarIdList.Contains(detectedCarId))
        {
            if (Mathf.Abs(carAgent.transform.localPosition.x - otherAgentPosition.x) < 1.5f) //attention
            {
                //Debug.Log("detect new car forward");
                if (carAgent.detectedFrontCarIdList.Count >= 5)
                {
                    carAgent.detectedFrontCarIdList.RemoveAt(0);
                }
                carAgent.detectedFrontCarIdList.Add(detectedCarId);
            }
        }
    }

 private void removeCarId(int detectedCarId)
    {
        if (carAgent.detectedFrontCarIdList.Contains(detectedCarId))
        {
            //Debug.Log("remove car id");
            carAgent.detectedFrontCarIdList.Remove(detectedCarId);
            carInformation.passingCounter++;
        }
    }   
}