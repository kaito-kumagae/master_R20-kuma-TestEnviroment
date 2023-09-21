using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RewardCalculation
{
    private CarAgent carAgent;
    private CarInformation carInformation;
    private Evaluator evaluator = Evaluator.getInstance();

    public RewardCalculation(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
    }

    public float CalculateIndividualReward()
    {
        float individualReward = 0.0f;
        var carCenter = carAgent.transform.position + Vector3.up; 

        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHitTile = hit.transform;

            if(carAgent._track == null)
            {
                carAgent._prev_track = carAgent._track;
                carAgent._track = newHitTile;
            }
            // move another tile
            else if (newHitTile != carAgent._track)
            {
                var relativePosition = carAgent.transform.position - newHitTile.position;
                evaluator.addHorizontalSensor(Time.realtimeSinceStartup, relativePosition.x * newHitTile.forward.z - relativePosition.z * newHitTile.forward.x, relativePosition.x * newHitTile.forward.x - relativePosition.z * newHitTile.forward.z, carAgent.id, carAgent.speed);
                // move previous tile
                if(newHitTile == carAgent._prev_track)
                {
                    individualReward = carAgent.movingPreviousTileReward;
                }
                // moving forward
                else
                {
                    float angle = Vector3.Angle(carAgent._track.forward, newHitTile.position - carAgent._track.position);
                    if (angle < 90f)
                    {
                        individualReward = carAgent.trackReward;

                        // if the tile's tag id "CheckPoint"
                        if (hit.collider.tag == "CheckPoint")
                        {
                            carInformation.throughCarNum++;
                        }
                        
                        // if the tile's tag id "endTile"
                        if (hit.collider.tag == "endTile")
                        {
                            carAgent.transform.localPosition = new Vector3(carAgent.transform.localPosition.x,0, 0);
                            if (carAgent.changeSpeed)
                            {
                                carAgent.speed = Random.Range(carAgent.minSpeed, carAgent.maxSpeed+1);
                                carAgent.frame.GetComponent<ColorController>().ChangeColor(carAgent.speed, carAgent.maxSpeed, carAgent.minSpeed);
                            }
                            if (carAgent.countPassing == true)
                            {
                                carAgent.detectedFrontCarIdList.Clear();
                            }
                        }
                    }
                    // moving backward
                    else
                    {
                        individualReward = carAgent.movingBackwardTileReward;
                    }
                }
                // if the tile's tag id "startTile"
                if (newHitTile.GetComponent<Collider>().tag == "startTile")
                {
                    evaluator.addThroughCars(Time.realtimeSinceStartup);
                }
                carAgent._prev_track = carAgent._track;
                carAgent._track = newHitTile;
            }
            else
            {
                individualReward = carAgent.stayingSameTileReward;
            }
        }
        return individualReward;
    }

    public float CalculateCommonReward()
    {
        float commonReward = carInformation.commonReward * carAgent.commonRewardRate;
        carInformation.receivedCommonRewardCarNum++;
        carAgent.canGetCommonReward = false;
        return commonReward;
    }

    public float CalculateAngleReward(Vector3 moveVec, float angle, float vertical)
    {
        float angleReward = ((1f - angle / 90f) * Mathf.Clamp01(Mathf.Max(0, vertical)) + Mathf.Min(0, vertical)) * Time.fixedDeltaTime;
        return angleReward;
    }

    public float CalculateDistanceReward(float distance, float distanceReward, float distanceThreshold)
    {
        if ((0 < distance) && (distance < distanceThreshold))
        {
            return distanceReward;
        }
        return 0.0f;
    }

    public void setCrashReward(float crashReward)
    {
        carAgent.SetReward(crashReward);
    }
}