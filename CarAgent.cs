using System.Collections;
using System.Collections.Generic;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using System.Linq;

public class CarAgent : Agent
{
    private Initialization initialization;
    private Movement movement;
    private Crash crash;
    private AddObservations addObservations;

    [Header("CAR PARAMETER")]
    public float speed = 10f;
    public float minSpeed = 5;
    public float maxSpeed = 15;
    public float torque = 1f;
    public float prevVertical = 0f;
    public float prevHorizontal = 0f;
    public int id = 0;
    public float noise = 0.1f;
    public float rayDistance = 5f;
    [Space(2)]
    [Header("REWARD")]
    public bool needDistanceReward = true;
    public bool canGetCommonReward = true;
    public int trackReward = 1;
    public float commonRewardRate = 1;
    public float distanceThreshold = 0.2f;
    public float[] distanceReward = new float[8];
    public float crashReward = -10f;
    [Space(2)]
    [Header("GENERATE CAR")]
    public bool generateNew = true;
    public int time = 0;
    public int generateInterval = 300;
    [Space(2)]
    [Header("ENVIRONMENT PARAMETER")]
    public int limitCarNum = 300;
    [Space(2)]
    [Header("SWITCH")]
    public bool resetOnCollision = true;
    public bool changeColor = true;
    public bool changeSpeed = true;
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
    public int commonRewardInterval = 500;
    [Space(2)]
    [Header("TEST PARAMETER")]
    public int stopTime = 5;
    public float timer = 0;
    
    private Evaluator evaluator = Evaluator.getInstance();
    private List<float> prev_observations;

    [HideInInspector]
    public int new_id = 0;
    [HideInInspector]
    public Transform _track, _prev_track;
    [HideInInspector]
    public RewardCalculation rewardCalculation;
    [HideInInspector]
    public Vector3 _initPosition;
    [HideInInspector]
    public Quaternion _initRotation;
    [HideInInspector]
    public bool foundCarForward, foundCarBackward, foundCarSide;

    public override void Initialize()
    {
        initialization = new Initialization(this);
        movement = new Movement(this);
        rewardCalculation = new RewardCalculation(this);
        crash = gameObject.AddComponent(typeof(Crash)) as Crash;
        crash.Initialize(this);
        addObservations = new AddObservations(this);
        initialization.Initialize();
    }

    void Update()
    {
        timer = Time.realtimeSinceStartup;
        if (!generateNew || id > 1) return;
        if (time > generateInterval && carInformation.currentCarNum < limitCarNum)
        {
            var gameObject = Instantiate(this, _initPosition, _initRotation);
            new_id += 2;
            gameObject.id = new_id;
            gameObject.transform.parent = this.transform.parent.gameObject.transform;
            gameObject.transform.localPosition = _initPosition;
            gameObject.transform.localRotation = _initRotation;
            gameObject.speed = Random.Range(minSpeed, maxSpeed+1);
            gameObject.canGetCommonReward = true;
            gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed, maxSpeed, minSpeed);
            carInformation.currentCarNum++;
            time = 0;
        }
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        movement.MoveCar(horizontal, vertical, dt);
    }
    
    public override void OnActionReceived(float[] vectorAction)
    {
        if (generateNew)
        {
            time++;
        }

        if (id == 0)
        {
            carInformation.CarInformationController(stopTime, commonRewardInterval);
        }

        if (carInformation.rewardTime >= commonRewardInterval && canGetCommonReward)
        {
            float commonReward = rewardCalculation.CalculateCommonReward();
            AddReward(commonReward);
        }

        if (carInformation.rewardTime < commonRewardInterval)
        {
            if (!canGetCommonReward)
            {
                canGetCommonReward = true;
            }
        }

        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];
        vertical = Mathf.Clamp(vertical, -1.0f, 1.0f);
        horizontal = Mathf.Clamp(horizontal, -1.0f, 1.0f);

        var lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        float individualReward = rewardCalculation.CalculateIndividualReward();

        var moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, _track.forward);
        float angleReward = rewardCalculation.CalculateAngleReward(moveVec, angle, vertical);

        AddReward(individualReward + angleReward);

        if (foundCarBackward && !foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)speed, false, vectorAction);
        }
        if (foundCarForward && !foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)speed, true, vectorAction);
        }

        evaluator.addFullData(Time.frameCount, transform.position, prev_observations, horizontal, vertical);
    }

    public override float[] Heuristic()
    {
        var action = new float[2];
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
        return action;
    }
 
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        List<float> observations = addObservations.MakeObservationsList();
        foreach (var v in observations)
        {
            vectorSensor.AddObservation(v);
        }
        prev_observations = observations;
    }

    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            transform.localPosition = _initPosition;
            transform.localRotation = _initRotation;
            this.speed = Random.Range(minSpeed, maxSpeed+1);
            if (changeColor)
            {
                frame.GetComponent<ColorController>().ChangeColor(this.speed, maxSpeed, minSpeed);
            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        crash.CrashProcess(other);
    }
}