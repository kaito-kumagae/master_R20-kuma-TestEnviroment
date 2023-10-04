using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInformation : MonoBehaviour
{
    public int currentCarNum = 3;
    public int startCarNum = 3;
    public int throughCarNum = 0;
    public int rewardTime = 0;
    public float commonReward = 0.0f;
    public int receivedCommonRewardCarNum = 0;
    public int passingCounter = 0;
    private Logger logger;

    void Start()
    {
        logger = new Logger(this);
    }

    public void CarInformationController(int stopTime, int commonRewardInterval)
    {
        rewardTime++;
        if ((Time.realtimeSinceStartup >= stopTime) && (stopTime != 0))
        {
            logger.PrintLog();
            Debug.Break();
        }

        if (rewardTime == commonRewardInterval)
        {
            commonReward = (float)throughCarNum/(float)currentCarNum;
            throughCarNum = 0;
            passingCounter = 0;
        }

        if (receivedCommonRewardCarNum >= currentCarNum)
        {
            rewardTime = 0;
            receivedCommonRewardCarNum = 0;
        }
    }
}
