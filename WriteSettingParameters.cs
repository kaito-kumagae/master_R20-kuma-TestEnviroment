using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

// !! This script should be set to course !!

public class WriteSettingParameters : MonoBehaviour
{
    private StreamWriter streamWriter;
    public CarAgent carAgent;
    public CarInformation carInformation;

    void Start()
    {
        streamWriter = new StreamWriter("settingParameters.txt");
        streamWriter.WriteLine("COURSE");
        streamWriter.WriteLine("course : " + this.name);
        streamWriter.WriteLine("");
        streamWriter.WriteLine("CAR PARAMETER");
        streamWriter.WriteLine("minSpeed : " + carAgent.minSpeed.ToString());
        streamWriter.WriteLine("maxSpeed : " + carAgent.maxSpeed.ToString());
        streamWriter.WriteLine("torque : " + carAgent.torque.ToString());
        streamWriter.WriteLine("noise : " + carAgent.noise.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("REWARD");
        streamWriter.WriteLine("needDistanceReward : " + carAgent.needDistanceReward.ToString());
        streamWriter.WriteLine("canGetCommonReward : " + carAgent.canGetCommonReward.ToString());
        streamWriter.WriteLine("trackReward : " + carAgent.trackReward.ToString());
        streamWriter.WriteLine("commonRewardRate : " + carAgent.commonRewardRate.ToString());
        streamWriter.WriteLine("distanceThreshold : " + carAgent.distanceThreshold.ToString());
        streamWriter.WriteLine("distanceReward(right forward) : " + carAgent.distanceReward[0].ToString());
        streamWriter.WriteLine("distanceReward(forward) : " + carAgent.distanceReward[1].ToString());
        streamWriter.WriteLine("distanceReward(left forward) : " + carAgent.distanceReward[2].ToString());
        streamWriter.WriteLine("distanceReward(right backward) : " + carAgent.distanceReward[3].ToString());
        streamWriter.WriteLine("distanceReward(backward) : " + carAgent.distanceReward[4].ToString());
        streamWriter.WriteLine("distanceReward(left backward) : " + carAgent.distanceReward[5].ToString());
        streamWriter.WriteLine("distanceReward(right) : " + carAgent.distanceReward[6].ToString());
        streamWriter.WriteLine("distanceReward(left) : " + carAgent.distanceReward[7].ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("GENERATE CAR");
        streamWriter.WriteLine("generateNew : " + carAgent.generateNew.ToString());
        streamWriter.WriteLine("generateInterval : " + carAgent.generateInterval.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("ENVIRONMENT PARAMETER");
        streamWriter.WriteLine("limitCarNum : " + carAgent.limitCarNum.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("SWITCH");
        streamWriter.WriteLine("resetOnCollision : " + carAgent.resetOnCollision.ToString());
        streamWriter.WriteLine("changeSpeed : " + carAgent.changeSpeed.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("PASSING");
        streamWriter.WriteLine("countPassing : " + carAgent.countPassing.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("CAR INFORMAION");
        streamWriter.WriteLine("commonRewardInterval : " + carAgent.commonRewardInterval.ToString());
        streamWriter.WriteLine("carNum : " + carInformation.carNum.ToString());
        streamWriter.Close();
    }
}
