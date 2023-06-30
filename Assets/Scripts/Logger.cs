using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private CarInformation carInformation;

    public void PrintLog()
    {
        Debug.Log("throughput : " + carInformation.throughCarNum);
        Debug.Log("crash : " + (evaluator.getNumCrash()-carInformation.crashCarNumLog));
        Debug.Log("passing : " + carInformation.passingCounter);
        Debug.Log("-------------------------------");
    }

    // void Start()
    // {
    // }
}
