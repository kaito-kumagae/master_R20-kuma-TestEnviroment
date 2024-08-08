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
        streamWriter.WriteLine("speed : " + carAgent.speed.ToString());
        streamWriter.WriteLine("minSpeed : " + carAgent.minSpeed.ToString());
        streamWriter.WriteLine("maxSpeed : " + carAgent.maxSpeed.ToString());
        streamWriter.WriteLine("torque : " + carAgent.torque.ToString());
        streamWriter.WriteLine("previousVertical : " + carAgent.previousVertical.ToString());
        streamWriter.WriteLine("previousHorizontal : " + carAgent.previousHorizontal.ToString());
        streamWriter.WriteLine("id : " + carAgent.id.ToString());
        streamWriter.WriteLine("noise : " + carAgent.noise.ToString());
        streamWriter.WriteLine("rayDistance : " + carAgent.rayDistance.ToString());
        streamWriter.WriteLine("communicateDistance : " + carAgent.communicateDistance.ToString());
        streamWriter.WriteLine("communicationCarsNum : " + carAgent.communicationCarsNum.ToString());
        streamWriter.WriteLine("GoalStepTime : " + carAgent.GoalStepTime.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("REWARD");
        streamWriter.WriteLine("needDistanceReward : " + carAgent.needDistanceReward.ToString());
        streamWriter.WriteLine("canGetCommonReward : " + carAgent.canGetCommonReward.ToString());
        streamWriter.WriteLine("movingForwardTileReward : " + carAgent.movingForwardTileReward.ToString());
        streamWriter.WriteLine("commonRewardRate : " + carAgent.commonRewardRate.ToString());
        streamWriter.WriteLine("distanceThreshold : " + carAgent.distanceThreshold.ToString());
        streamWriter.WriteLine("crashReward : " + carAgent.crashReward.ToString());
        streamWriter.WriteLine("movingPreviousTileReward : " + carAgent.movingPreviousTileReward.ToString());
        streamWriter.WriteLine("movingBackwardTileReward : " + carAgent.movingBackwardTileReward.ToString());
        streamWriter.WriteLine("stayingSameTileReward : " + carAgent.stayingSameTileReward.ToString());
        streamWriter.WriteLine("slipStreamReward : " + carAgent.slipStreamReward.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("GENERATE CAR");
        streamWriter.WriteLine("generateNew : " + carAgent.generateNew.ToString());
        streamWriter.WriteLine("time : " + carAgent.time.ToString());
        streamWriter.WriteLine("generateInterval : " + carAgent.generateInterval.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("ENVIRONMENT PARAMETER");
        streamWriter.WriteLine("limitCarNum : " + carAgent.limitCarNum.ToString());
        streamWriter.WriteLine("alpha : " + carAgent.alpha.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("SWITCH");
        streamWriter.WriteLine("resetOnCollision : " + carAgent.resetOnCollision.ToString());
        streamWriter.WriteLine("changeColor : " + carAgent.changeColor.ToString());
        streamWriter.WriteLine("changeSpeed : " + carAgent.changeSpeed.ToString());
        streamWriter.WriteLine("alphaFlag : " + carAgent.alphaFlag.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("PASSING");
        streamWriter.WriteLine("countPassing : " + carAgent.countPassing.ToString());
        streamWriter.WriteLine("");
        streamWriter.WriteLine("CAR INFORMATION");
        streamWriter.WriteLine("stepTime : " + carAgent.stepTime.ToString());
        streamWriter.WriteLine("commonRewardInterval : " + carAgent.commonRewardInterval.ToString());
        streamWriter.WriteLine("currentCarNnm : " + carInformation.currentCarNum.ToString());
        streamWriter.Close();
    }
}
