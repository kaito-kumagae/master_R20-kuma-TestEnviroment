using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crash : MonoBehaviour
{
    private CarAgent carAgent;
    private Evaluator evaluator = Evaluator.getInstance();
    private CarInformation carInformation;

    // メンバー変数を初期化する
    public void Initialize(CarAgent carAgent)
    {
        this.carAgent = carAgent;
        this.carInformation = carAgent.carInformation;
    }

    public void CrashProcess(Collision other)
    {
        // 車が事故を起こした場合
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("car") || other.gameObject.CompareTag("TruckCar"))
        {
            var carCenter = carAgent.transform.position + Vector3.up;
            RaycastHit hit;

            // 車がTruckRespawnTile上にあり、トラックとの衝突であるかを確認
            if (Physics.Raycast(carCenter, Vector3.down, out hit, 2f))
            {
                var tile = hit.transform;
                if (tile.CompareTag("TruckRespawnTile") && other.gameObject.CompareTag("TruckCar"))
                {
                    carAgent.rewardCalculation.setCrashReward(0);
                }
                else
                {
                    carAgent.rewardCalculation.setCrashReward(carAgent.crashReward);
                }
            }

            carAgent.EndEpisode();
            if (carAgent.countPassing == true)
            {
                carAgent.detectedFrontCarIdList.Clear();
                GetComponentInParent<UpdateCarParameters>().RemoveMyIdFromAllcarAgents(carAgent.id);
            }

            // 衝突が他の車との場合
            if (other.gameObject.CompareTag("car"))
            {
                var otherAgent = (CarAgent)other.gameObject.GetComponent(typeof(CarAgent));
                if ((carAgent.id < otherAgent.id) && (IsNotErasedId(otherAgent.id)))
                {
                    if (Physics.Raycast(carCenter, Vector3.down, out hit, 2f))
                    {
                        var newHit = hit.transform;
                        if (newHit.GetComponent<Collider>().tag == "startTile")
                        {
                            if (carAgent.generateNew)
                            {
                                Destroy(other.gameObject);
                                carAgent.carInformation.currentCarNum--;
                            }
                        }
                        else
                        {
                            evaluator.addCrashCars(Time.realtimeSinceStartup, carAgent.speed);
                            if (carAgent.generateNew)
                            {
                                evaluator.addCrashCars(Time.realtimeSinceStartup, otherAgent.speed);
                                Destroy(other.gameObject);
                                carInformation.currentCarNum--;
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

    // id0とid1の車が衝突した場合に両方の車が消えるのを防ぐ
    private bool IsNotErasedId(int otherAgentId)
    {
        List<int> isNotErasedIdList = new List<int>();
        for (int i = 0; i < carAgent.startCarNum; i++)
        {
            isNotErasedIdList.Add(i);
        }
        if ((isNotErasedIdList.Contains(carAgent.id)) && (isNotErasedIdList.Contains(otherAgentId)))
        {
            return false;
        }
        return true;
    }
}
