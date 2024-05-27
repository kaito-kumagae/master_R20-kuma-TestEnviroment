using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddObservations
{
    private CarAgent carAgent;
    private CarInformation carInformation;
    private RewardCalculation rewardCalculation;
    private SlipStream slipStream;

    public AddObservations(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
        this.rewardCalculation = carAgent.rewardCalculation;
        this.slipStream = carAgent.slipStream;
    }

    public List<float> MakeObservationsList()
    {
        List<float> observations = new List<float>();
        float angle = Vector3.SignedAngle(carAgent.currentTrack.forward, carAgent.transform.forward, Vector3.up);
        carAgent.foundCarBackward = false;
        carAgent.foundCarForward = false;
        carAgent.foundTruckBackward = false;
        carAgent.foundCarSide = false;

        observations.Add(angle / 180f);
        
        string tag;
        Vector3 relativeSpeed;
        int detectedCarId;
        Vector3 otherAgentPosition;
        float distanceObservedObject;
        float distanceReward;

        (float, float, float)[] rayDirections = //ray
        {
            (1.5f, .5f, 25f),     // right forward
            (1.5f, 0f, 0f),      // forward
            (1.5f, -.5f, -25f),  // left forward
            (-1.5f, .5f, 155f),  // right backward
            (-1.5f, 0f, 180f),   // backward
            (-1.5f, -.5f, -155f),// left backward
            (0f, .5f, 90f),      // right side
            (0f, -.5f, -90f),    // left side
            // (0.75f, .5f, 57.5f), // right forward-side
            // (0.75f, -.5f, -57.5f), // left forward-side
            // (-0.75f, .5f, 122.5f), //right backward-side
            // (-0.75f, .5f, -122.5f) //left backward-side
        };

        for (int i = 0; i < rayDirections.Length; i++)
        {
            distanceObservedObject = ObserveRay(rayDirections[i].Item1, rayDirections[i].Item2, rayDirections[i].Item3, out relativeSpeed, out tag, out detectedCarId, out otherAgentPosition);
            if (carAgent.needDistanceReward)
            {
                distanceReward = rewardCalculation.CalculateDistanceReward(distanceObservedObject, carAgent.distanceReward[i], 0.2f); //要検討
                carAgent.AddReward(distanceReward);
            }
            observations.Add(distanceObservedObject);
            observations.Add(relativeSpeed.x);
            observations.Add(relativeSpeed.z);
            float carVerticalPosition = rayDirections[i].Item1;
            ObjectObservation(tag, detectedCarId, otherAgentPosition, ref observations, carVerticalPosition);

            if (rayDirections[i] == (1.5f, 0f, 0f))
            {
                TruckDetection(tag);
            }
        }
        observations.Add(carAgent.speed);
        observations.Add(carAgent.torque);

        return observations;
    }
    private void TruckDetection(string tag)
    {
        // トラック検知ロジックをここに追加します
        if (tag == "TruckCar")
        {
            carAgent.foundTruckBackward = true; // 例: トラックを検知したことを示す
            carAgent.slipStream.JudgeSlipStream(tag);
        }
    }
    private void ObjectObservation(string tag, int detectedCarId, Vector3 otherAgentPosition, ref List<float> observations, float carVerticalPosition)
    {   
        if (tag == "car")
        {
            if (carVerticalPosition > 0)
            {
                carAgent.foundCarForward = true;
                if (carAgent.countPassing)
                {
                    addOvertakingCarId(detectedCarId, otherAgentPosition);
                }
            }
            else if (carVerticalPosition < 0)
            {
                carAgent.foundCarBackward = true;
                if (carAgent.countPassing)
                {
                    removeOvertakenCarId(detectedCarId);
                }
            }
            else
            {
                carAgent.foundCarSide = true;
            } 
        }

        observations.Add(tag == "car" ? 1 : 0);
        observations.Add(tag == "wall" ? 1 : 0);
    }

    private float ObserveRay(float z, float x, float angle, out Vector3 relativeSpeed, out string tag, out int detectedCarId, out Vector3 otherAgentPosition)
    {
        relativeSpeed = Vector3.zero;
        tag = "none";
        detectedCarId = -1;
        otherAgentPosition = Vector3.zero;
        var tf = carAgent.transform;

        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
        RaycastHit hit;

        // laser visualization
        Ray ray = new Ray(position, dir);
        Debug.DrawRay(ray.origin, ray.direction*carAgent.rayDistance, Color.red);

        // See if there is a hit in the given direction
        var rayHit = Physics.Raycast(position, dir, out hit, carAgent.rayDistance);
        if (rayHit)
        {
            tag = hit.collider.tag;
            otherAgentPosition = hit.collider.transform.localPosition;
            if (hit.collider.tag == "car")
            {
                CarAgent agent = hit.collider.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
                detectedCarId = agent.id;
                var selfDir = Quaternion.Euler(0, carAgent.torque * carAgent.previousHorizontal * 90f, 0) * (carAgent.transform.forward * carAgent.previousVertical * carAgent.speed);
                var agentDir = Quaternion.Euler(0, agent.torque * agent.previousHorizontal * 90f, 0) * (agent.transform.forward * agent.previousVertical * agent.speed);
                relativeSpeed = agentDir - selfDir;
            }
        }
        return hit.distance >= 0 ? (hit.distance / carAgent.rayDistance) * Random.Range(1-carAgent.noise, 1+carAgent.noise) : -1f;
    }

    private void addOvertakingCarId(int detectedCarId, Vector3 otherAgentPosition)
    {
        if (!carAgent.detectedFrontCarIdList.Contains(detectedCarId))
        {
            if (Mathf.Abs(carAgent.transform.localPosition.x - otherAgentPosition.x) < 1.5f)
            {
                if (carAgent.detectedFrontCarIdList.Count >= 5)
                {
                    carAgent.detectedFrontCarIdList.RemoveAt(0);
                }
                carAgent.detectedFrontCarIdList.Add(detectedCarId);
            }
        }
    }

    private void removeOvertakenCarId(int detectedCarId)
    {
        if (carAgent.detectedFrontCarIdList.Contains(detectedCarId))
        {
            carAgent.detectedFrontCarIdList.Remove(detectedCarId);
            carInformation.passingCounter++;
        }
    }
}