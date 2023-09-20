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
        streamWriter.WriteLine("penaltyRewards(right forward) : " + carAgent.penaltyRewards[0].ToString());
        streamWriter.WriteLine("penaltyRewards(forward) : " + carAgent.penaltyRewards[1].ToString());
        streamWriter.WriteLine("penaltyRewards(left forward) : " + carAgent.penaltyRewards[2].ToString());
        streamWriter.WriteLine("penaltyRewards(right backward) : " + carAgent.penaltyRewards[3].ToString());
        streamWriter.WriteLine("penaltyRewards(backward) : " + carAgent.penaltyRewards[4].ToString());
        streamWriter.WriteLine("penaltyRewards(left backward) : " + carAgent.penaltyRewards[5].ToString());
        streamWriter.WriteLine("penaltyRewards(right) : " + carAgent.penaltyRewards[6].ToString());
        streamWriter.WriteLine("penaltyRewards(left) : " + carAgent.penaltyRewards[7].ToString());
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
        streamWriter.WriteLine("carNum : " + carInformation.currentCarNum.ToString());
        streamWriter.Close();
    }
}
