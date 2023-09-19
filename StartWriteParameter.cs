using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StartWriteParameter : MonoBehaviour
{
    private StreamWriter logParameters;
    public CarAgent car;
    public CarInformation carInformation;

    void Start()
    {
        logParameters = new StreamWriter("logParameters.txt");
        logParameters.WriteLine("course : " + this.name);
        logParameters.WriteLine("maxSpeed : " + car.maxSpeed.ToString());
        logParameters.WriteLine("minSpeed : " + car.minSpeed.ToString());
        logParameters.WriteLine("torque : " + car.torque.ToString());
        logParameters.WriteLine("generateNew : " + car.generateNew.ToString());
        logParameters.WriteLine("generateInterval : " + car.generateInterval.ToString());
        logParameters.WriteLine("noise : " + car.noise.ToString());
        logParameters.WriteLine("trackReward : " + car.trackReward.ToString());
        logParameters.WriteLine("commonRewardRate : " + car.commonRewardRate.ToString());
        logParameters.WriteLine("limitCarNum : " + car.limitCarNum.ToString());
        logParameters.WriteLine("rewardInterval : " + carInformation.rewardInterval.ToString());
        logParameters.WriteLine("carNum : " + carInformation.carNum.ToString());
        logParameters.Close();
    }

}
