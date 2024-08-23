using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using System;


public class CarAgent : Agent
{
    private Initialization initialization;
    [HideInInspector]
    public Movement movement;
    [HideInInspector]
    public RewardCalculation rewardCalculation;
    [HideInInspector]
    public TrackRecognition trackRecognition;
    public Communication communication;
    public SlipStream slipStream;
    private Crash crash;
    private AddObservations addObservations;
    private Action action;
    public BrainAlpha brainAlpha;
    
    

    [Header("CAR PARAMETER")]
    public float speed = 10f;
    public float minSpeed = 5;
    public float maxSpeed = 15;
    public float torque = 1f;
    public float previousVertical = 0f;
    public float previousHorizontal = 0f;
    public int id = 0;
    public float noise = 0.1f;
    public float rayDistance = 5f;
    public float communicateDistance = 100f;
    public int communicationCarsNum = 1;
    public int GoalStepTime = 0;
    public int ActualArrivalTime = 0;
    [Space(2)]
    [Header("REWARD")]
    public bool needDistanceReward = true;
    public bool canGetCommonReward = true;
    public int movingForwardTileReward = 1;
    public float commonRewardRate = 1;
    public float distanceThreshold = 0.2f;
    public float[] distanceReward = new float[8];
    public float crashReward = -10f;
    public float movingPreviousTileReward = -1f;
    public float movingBackwardTileReward = -1f;
    public float stayingSameTileReward = -0.01f;
    public float slipStreamReward = 0.003f;
    [Space(2)]
    [Header("GENERATE CAR")]
    public bool generateNew = true;
    public int time = 0;
    public int generateInterval = 300;
    [Space(2)]
    [Header("ENVIRONMENT PARAMETER")]
    public int limitCarNum = 300;
    public int limitTruckNum = 10;
    public float alpha;
    [Space(2)]
    [Header("SWITCH")]
    public bool resetOnCollision = true;
    public bool changeColor = true;
    public bool changeSpeed = true;
    public bool alphaFlag = true;
    [Space(2)]
    [Header("PASSING")]
    public bool countPassing = true;
    public List<int> detectedFrontCarIdList = new List<int>(5);
    [Space(2)]
    [Header("GAME OBJECT")]
    public GameObject frame;
    public CarInformation carInformation;
    [Space(2)]
    [Header("CAR INFORMATION")]
    //public int loop = 0;
    public int stepTime = 0;
    public int commonRewardInterval = 500;
    [Space(2)]
    [Header("TEST PARAMETER")]
    public int stopTime = 5;
    public float timer = 0;
    private Evaluator evaluator = Evaluator.getInstance();
    [HideInInspector]
    public List<float> previousObservations;
    [HideInInspector]
    public int new_id = 0;
    [HideInInspector]
    public Transform currentTrack, previousTrack;
    [HideInInspector]
    public Vector3 _initPosition;
    [HideInInspector]
    public Quaternion _initRotation;
    [HideInInspector]
    public bool foundCarForward, foundCarBackward, foundCarSide;
    [HideInInspector]
    public bool foundTrackForward;
    [HideInInspector]
    public bool movingPreviousTile, movingForwardTile, movingBackwardTile, stayingSameTile;
    [HideInInspector]
    public int startCarNum;
    [HideInInspector]
    public float SlipStreamDistance = -1.0f;
    [HideInInspector]
    private List<float> observations;
    [HideInInspector]
    public BoxCollider boxCol;
    public GameObject cube;
    
    public float differentTime = 0;
    

    public override void Initialize()
    {
        initialization = new Initialization(this);
        movement = new Movement(this);
        trackRecognition = new TrackRecognition(this);
        rewardCalculation = new RewardCalculation(this);
        crash = gameObject.AddComponent(typeof(Crash)) as Crash;
        crash.Initialize(this);
        addObservations = new AddObservations(this);
        communication = new Communication(this);
        action = new Action(this);
        initialization.Initialize();
        slipStream = new SlipStream(this);
        startCarNum = carInformation.startCarNum;
    }

    void Update()
    {
        timer = Time.realtimeSinceStartup;
        if (!generateNew || id > startCarNum - 1) return;
        if (time > generateInterval && carInformation.currentCarNum < limitCarNum)
        {
            if(this.tag == "car" && carInformation.currentCarNum < limitCarNum)
            {
                var gameObject = Instantiate(this, _initPosition, _initRotation);
                new_id += startCarNum;
                gameObject.id = new_id;
                gameObject.transform.parent = this.transform.parent.gameObject.transform;
                gameObject.transform.localPosition = _initPosition;
                gameObject.transform.localRotation = _initRotation;
                gameObject.speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                gameObject.canGetCommonReward = true;
                GoalStepTime = UnityEngine.Random.Range(1401, 2036);
                Debug.Log("GoalStepTime : " + GoalStepTime);
                if(alphaFlag)
                {
                    gameObject.alpha = UnityEngine.Random.Range(0.0f, 1.0f);
                    //Debug.Log("alpha : " + gameObject.alpha);
                }
                try 
                {
                    gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed, maxSpeed, minSpeed);
                }
                catch 
                {
                }
                carInformation.currentCarNum++;
                time = 0;
            }
            if(this.tag == "TruckCar" && carInformation.currentTruckNum < limitTruckNum)
            {
                var gameObject = Instantiate(this, _initPosition, _initRotation);
                new_id += startCarNum;
                gameObject.id = new_id;
                gameObject.transform.parent = this.transform.parent.gameObject.transform;
                gameObject.transform.localPosition = _initPosition;
                gameObject.transform.localRotation = _initRotation;
                gameObject.speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
                gameObject.canGetCommonReward = true;
                //GoalStepTime = UnityEngine.Random.Range(1401, 2036);
                GoalStepTime = 1401;
                if(alphaFlag)
                {
                    gameObject.alpha = UnityEngine.Random.Range(0.0f, 1.0f);
                    //Debug.Log("alpha : " + gameObject.alpha);
                }
                try 
                {
                    gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed, maxSpeed, minSpeed);
                }
                catch 
                {
                }
                carInformation.currentCarNum++;
                carInformation.currentTruckNum++;
                time = 0;
            }
        }
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        action.ActionProcess(actionBuffers);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
 
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        observations = addObservations.MakeObservationsList();
        foreach (var v in observations)
        {
            vectorSensor.AddObservation(v);
        }
        previousObservations = observations;
    }

    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            transform.localPosition = _initPosition;
            transform.localRotation = _initRotation;
            this.speed = UnityEngine.Random.Range(minSpeed, maxSpeed);
            if (changeColor)
            {
                try 
                {
                    frame.GetComponent<ColorController>().ChangeColor(this.speed, maxSpeed, minSpeed);
                }
                catch 
                {
                }
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        crash.CrashProcess(other);
    }
}

