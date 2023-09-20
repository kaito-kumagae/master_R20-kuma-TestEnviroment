using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInformation : MonoBehaviour
{
    public float carNum = 0;
    public int carNumLog = 0;
    public int totalCarNum = 0;
    public int totalCarNumLog = 0;
    public int crashCarNumLog = 0;
    public float throughCarNum = 0;
    public int rewardTime = 0;
    public float reward = 0.0f;

    public float commonReward = 0;
    public int getRewardCarNum = 0;
    public int getRewardCarNumLog = 0;

    public int startPositionX;
    public int[,] startPositionXList = new int[,] {{1, 2}, {0, 2}, {0, 1}};
    public int passingCounter = 0;

    // void Start()
    // {
    //     startPositionX = Random.Range(0, 3);
    // }
    // public void choicePositionX()
    // {
    //     int twoDimensionsIndex = Random.Range(0, 2);
    //     startPositionX = startPositionXList[startPositionX, twoDimensionsIndex];
    // }
}
