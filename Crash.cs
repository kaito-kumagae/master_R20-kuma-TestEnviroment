using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crash : MonoBehaviour
{
    private CarAgent carAgent;
    private Evaluator evaluator = Evaluator.getInstance();
    private CarInformation carInformation;

    public void Initialize(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        carInformation = this.carAgent.carInformation;
    }

    public void CrashProcess(Collision other)
    {
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("car"))
        {
            // increased from -1f -> -10f
            var carCenter = carAgent.transform.position + Vector3.up;
            carAgent.SetReward(-10f);
            carAgent.EndEpisode();
            if (carAgent.countPassing == true)
            {
                carAgent.detectedFrontCarIdList.Clear();
            }
            if(other.gameObject.CompareTag("car"))
            {
                var otherAgent = (CarAgent)other.gameObject.GetComponent(typeof(CarAgent));
                if((carAgent.id < otherAgent.id) && (isNot01(otherAgent.id)))
                {
                    if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
                    {
                        var newHit = hit.transform;
                        if (newHit.GetComponent<Collider>().tag == "startTile")
                        {
                            if(carAgent.generateNew)
                            {
                                Destroy(other.gameObject);
                                carAgent.carInformation.carNum--;
                                carAgent.carInformation.totalCarNum++;
                            }
                        }

                        else
                        {
                            evaluator.addCrashCars(Time.realtimeSinceStartup,carAgent.speed);
                            if(carAgent.generateNew)
                            {
                                evaluator.addCrashCars(Time.realtimeSinceStartup,otherAgent.speed);
                                Destroy(other.gameObject);
                                carInformation.carNum--;
                                carInformation.totalCarNum++;
                            }
                        }
                    }
                }
            }
            else
            {
                    evaluator.addCrashCars(Time.realtimeSinceStartup, carAgent.speed);
            }
        }
    }
    private bool isNot01(int otherAgentId)
    {
        if ((carAgent.id == 0) || (carAgent.id == 1))
        {
            if ((otherAgentId == 0) || (otherAgentId == 1))
            {
                return false;
            }
        }
        return true;
    }
}