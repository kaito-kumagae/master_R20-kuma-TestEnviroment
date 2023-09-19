using System.Collections;
using System.Collections.Generic;
using MLAgents;
using UnityEngine;

public class RewardCalculation : Agent
{
    private Evaluator evaluator;
    private CarInformation carInformation;
    private CarAgent carAgent;

    public RewardCalculation(CarAgent carAgent)
    {
        this.carAgent = carAgent;
    }

    public float CalculateIndividualReward()
    {
        float individualReward = 0.0f;
        var carCenter = transform.position + Vector3.up;

        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHitTile = hit.transform;

            if(this.carAgent._track == null)
            {
                this.carAgent._prev_track = this.carAgent._track;
                this.carAgent._track = newHitTile;
            }
            // move another tile
            else if (newHitTile != this.carAgent._track)
            {
                var relativePosition = transform.position - newHitTile.position;
                evaluator.addHorizontalSensor(Time.realtimeSinceStartup, relativePosition.x * newHitTile.forward.z - relativePosition.z * newHitTile.forward.x, relativePosition.x * newHitTile.forward.x - relativePosition.z * newHitTile.forward.z, this.carAgent.id, this.carAgent.speed);
                // move previous tile
                if(newHitTile == this.carAgent._prev_track)
                {
                    individualReward = -1;
                }
                // moving forward
                else
                {
                    float angle = Vector3.Angle(this.carAgent._track.forward, newHitTile.position - this.carAgent._track.position);
                    if (angle < 90f)
                    {
                        individualReward = this.carAgent.trackReward;

                        // if the tile's tag id "CheckPoint"
                        if (hit.collider.tag == "CheckPoint")
                        {
                            carInformation.throughCarNum++;
                        }
                        
                        // if the tile's tag id "endTile"
                        if (hit.collider.tag == "endTile")
                        {
                            transform.localPosition = new Vector3(transform.localPosition.x,0, 0);
                            if (this.carAgent.changeSpeed)
                            {
                                this.carAgent.speed = Random.Range(carAgent.minSpeed, carAgent.maxSpeed+1);
                                this.carAgent.frame.GetComponent<ColorController>().ChangeColor(this.carAgent.speed, carAgent.maxSpeed, carAgent.minSpeed);
                            }
                            if (carAgent.countPassing == true)
                            {
                                this.carAgent.detectedFrontCarIdList.Clear();
                            }
                        }
                    }
                    // moving backward
                    else
                    {
                        individualReward = -1;
                    }
                }
                // if the tile's tag id "startTile"
                if (newHitTile.GetComponent<Collider>().tag == "startTile")
                {
                    evaluator.addThroughCars(Time.realtimeSinceStartup);
                }
                this.carAgent._prev_track = this.carAgent._track;
                this.carAgent._track = newHitTile;
            }
            else
            {
                individualReward = -0.01f;
            }
        }
        return individualReward;
    }

    public float CalculateCommonReward()
    {
        float commonReward = carInformation.commonReward * carAgent.rewardRate;
        carInformation.getRewardCarNum++;
        this.carAgent.canGetCommonReward = false;
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
}