using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackRecognition
{
    private CarAgent carAgent;
    private CarInformation carInformation;
    private Evaluator evaluator = Evaluator.getInstance();
    private CalculateFuelDispersion calculateFuelDispersion;


    public TrackRecognition(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
        this.calculateFuelDispersion = carAgent.calculateFuelDispersion;
    }

    public void TrackRecognize()
    {
        carAgent.movingPreviousTile = false;
        carAgent.movingForwardTile = false;
        carAgent.movingBackwardTile = false;
        carAgent.stayingSameTile = false;

        var carCenter = carAgent.transform.position + Vector3.up; 

        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHitTile = hit.transform;

            if(carAgent.currentTrack == null)
            {
                carAgent.previousTrack = carAgent.currentTrack;
                carAgent.currentTrack = newHitTile;
            }
            // move another tile
            else if (newHitTile != carAgent.currentTrack)
            {
                var relativePosition = carAgent.transform.position - newHitTile.position;
                evaluator.addHorizontalSensor(Time.realtimeSinceStartup, relativePosition.x * newHitTile.forward.z - relativePosition.z * newHitTile.forward.x, relativePosition.x * newHitTile.forward.x - relativePosition.z * newHitTile.forward.z, carAgent.id, carAgent.speed);
                // move previous tile
                if(newHitTile == carAgent.previousTrack)
                {
                    carAgent.movingPreviousTile = true;
                }
                // moving forward
                else
                {
                    float angle = Vector3.Angle(carAgent.currentTrack.forward, newHitTile.position - carAgent.currentTrack.position);
                    if (angle < 90f)
                    {
                        carAgent.movingForwardTile = true;

                        // if the tile's tag id "CheckPoint"
                        if (hit.collider.tag == "CheckPoint")
                        {
                            carInformation.throughCarNum++;
                        }
                        
                        // if the tile's tag id "endTile"
                        if (hit.collider.tag == "endTile")
                        {
                            carAgent.transform.localPosition =carAgent._initPosition; //new Vector3(carAgent.transform.localPosition.x,0, 0);
                            if (carAgent.changeSpeed)
                            {
                                carAgent.speed = Random.Range(carAgent.minSpeed, carAgent.maxSpeed+1);
                                carAgent.frame.GetComponent<ColorController>().ChangeColor(carAgent.speed, carAgent.maxSpeed, carAgent.minSpeed);
                            }
                            if (carAgent.countPassing == true)
                            {
                                carAgent.detectedFrontCarIdList.Clear();
                            }
                           carAgent.calculateFuelDispersion.CollectFuelConsumption();
                        }
                    }
                    // moving backward
                    else
                    {
                        carAgent.movingBackwardTile = true;
                    }
                }
                // if the tile's tag id "startTile"
                if (newHitTile.GetComponent<Collider>().tag == "startTile")
                {
                    evaluator.addThroughCars(Time.realtimeSinceStartup);
                }
                carAgent.previousTrack = carAgent.currentTrack;
                carAgent.currentTrack = newHitTile;
            }
            else
            {
                carAgent.stayingSameTile = true;
            }
        }
    }
}