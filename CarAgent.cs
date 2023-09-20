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

    [Header("CAR PARAMETER")]
    public float speed = 10f;
    public float minSpeed = 5;
    public float maxSpeed = 15;
    public float torque = 1f;
    public float prevVertical = 0f;
    public float prevHorizontal = 0f;
    public int id = 0;
    public float noise = 0.1f;
    [Space(2)]
    [Header("REWARD")]
    public bool needDistanceReward = true;
    public bool canGetCommonReward = true;
    public int trackReward = 1;
    public float commonRewardRate = 1;
    public float distanceThreshold = 0.2f;
    public float[] penaltyRewards = new float[8];
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
    [Space(2)]
    [Header("iranai")]
    public bool[] diffXYZ = new bool[] {true, false, true};
    private Evaluator evaluator = Evaluator.getInstance();

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
    


    public override void Initialize()
    {
        initialization = new Initialization(this);
        movement = new Movement(this);
        crash = gameObject.AddComponent(typeof(Crash)) as Crash;
        crash.Initialize(this);
        rewardCalculation = new RewardCalculation(this);
        initialization.Initialize();
    }

    void Update()
    {
        timer = Time.realtimeSinceStartup;
        if (!generateNew || id > 1) return;
        //Debug.Log(time);
        if (time > generateInterval && carInformation.carNum < limitCarNum)
        {
            //Debug.Log("add new car");
            var gameObject = Instantiate(this, _initPosition, _initRotation);
            new_id += 2;
            gameObject.id = new_id;
            gameObject.transform.parent = this.transform.parent.gameObject.transform;
            gameObject.transform.localPosition = _initPosition;
            gameObject.transform.localRotation = _initRotation;
            gameObject.speed = Random.Range(minSpeed, maxSpeed+1);
            gameObject.canGetCommonReward = true;
            gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed, maxSpeed, minSpeed);
            //Debug.Log("Generaiterate GenerateInterval");
            carInformation.carNum++;
            time = 0;
        }
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        movement.MoveCar(horizontal, vertical, dt);
    }
    private List<float> prev_observations;

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

    private bool foundCarForward, foundCarBackward, foundCarSide;
    public override void CollectObservations(VectorSensor vectorSensor)
    {
        List<float> observations = new List<float>();
        float angle = Vector3.SignedAngle(_track.forward, transform.forward, Vector3.up);
        foundCarBackward = false;
        foundCarForward = false;
        foundCarSide = false;

        observations.Add(angle / 180f);
        
        string tag;
        
        Vector3 diff;

        int detectedId;

        Vector3 otherAgentPosition;

        float distance;
        float distanceReward;

        //vectorSensor.AddObservation(ObserveRay(1.5f, .5f, 25f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(1.5f, .5f, 25f, out diff, out tag, out detectedId, out otherAgentPosition); //right forward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[0], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarForward = true;
            if (countPassing == true)
            {
                addElement(detectedId, otherAgentPosition);
            }
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //vectorSensor.AddObservation(ObserveRay(1.5f, 0f, 0f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(1.5f, 0f, 0f, out diff, out tag, out detectedId, out otherAgentPosition); //forward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[1], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //vectorSensor.AddObservation((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //vectorSensor.AddObservation(speed);
        //vectorSensor.AddObservation(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarForward = true;
            if (countPassing == true)
            {
                addElement(detectedId, otherAgentPosition);
            }
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(1.5f, -.5f, -25f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(1.5f, -.5f, -25f, out diff, out tag, out detectedId, out otherAgentPosition); //left forward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[2], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarForward = true;
            if (countPassing == true)
            {
                addElement(detectedId, otherAgentPosition);
            }
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(-1.5f, .5f, 155f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(-1.5f, .5f, 155f, out diff, out tag, out detectedId, out otherAgentPosition); //right backward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[3], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarBackward = true;
            if (countPassing == true)
            {
                removeElement(detectedId);
            }

        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(-1.5f, 0, 180f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(-1.5f, 0f, 180f, out diff, out tag, out detectedId, out otherAgentPosition); //backward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[4], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarBackward = true;
            if (countPassing == true)
            {
                removeElement(detectedId);
            }
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(-1.5f, -.5f, -155f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(-1.5f, -.5f, -155f, out diff, out tag, out detectedId, out otherAgentPosition); //left backward
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[5], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarBackward = true;
            if (countPassing == true)
            {
                removeElement(detectedId);
            }
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(0f, .5f, 90f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(0f, .5f, 90f, out diff, out tag, out detectedId, out otherAgentPosition); //right
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[6], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarSide = true;
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        //observations.Add(ObserveRay(0f, -.5f, -90f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(0f, -.5f, -90f, out diff, out tag, out detectedId, out otherAgentPosition); //left
        if (needDistanceReward == true)
        {
            distanceReward = rewardCalculation.CalculateDistanceReward(distance, penaltyRewards[7], 0.2f);
            AddReward(distanceReward);
        }
        observations.Add(distance);
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        ChoiceDiff(diff, ref observations);
        if (tag == "car")
        {
            observations.Add(1);
            foundCarSide = true;
        }
        else
        {
            observations.Add(0);
        }
        if (tag == "wall")
        {
            observations.Add(1);
        }
        else
        {
            observations.Add(0);
        }
        observations.Add(this.speed);
        observations.Add(this.torque);
        foreach (var v in observations)
        {
            vectorSensor.AddObservation(v);
        }
        prev_observations = observations;
    }

    //private float ObserveRay(float z, float x, float angle, out float speed, out float torque, out Quaternion rotation, out string tag)
    private float ObserveRay(float z, float x, float angle, out Vector3 diff, out string tag, out int detectedId, out Vector3 otherAgentPosition)
    {
        //speed = 0.0f;
        //torque = 0.0f;
        //rotation = Quaternion.identity;
        diff = Vector3.zero;
        tag = "none";
        detectedId = -1;
        otherAgentPosition = Vector3.zero;
        var tf = transform;

        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        const float RAY_DIST = 5f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
        RaycastHit hit;

        // laser visualization
        Ray ray = new Ray(position, dir);
        Debug.DrawRay(ray.origin, ray.direction*RAY_DIST, Color.red);


        // See if there is a hit in the given direction
        var rayhit = Physics.Raycast(position, dir, out hit, RAY_DIST);
        if (rayhit)
        {
            tag = hit.collider.tag;
            otherAgentPosition = hit.collider.transform.localPosition;
            if (hit.collider.tag == "car"){
                CarAgent agent = hit.collider.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
                detectedId = agent.id;
                // if (foundCarBackward == true)
                // {
                //     detectedBackwardId = agent.id;
                //     if (this.detectedFrontCarIdList.Contains(detectedBackwardId))
                //     {
                //         Debug.Log("remove car id");
                //         removeElement(detectedBackwardId);
                //     }
                // }
                var self_dir = Quaternion.Euler(0, this.torque * this.prevHorizontal * 90f, 0) * (this.transform.forward * this.prevVertical * this.speed);
                var agent_dir = Quaternion.Euler(0, agent.torque * agent.prevHorizontal * 90f, 0) * (agent.transform.forward * agent.prevVertical * agent.speed);
                diff = agent_dir - self_dir;
		        //Debug.Log("diff"+string.Join(",", diff));
                //Debug.Log("diff.x : "+diff.x);
		        //Debug.Log("diff.z : "+diff.z);
                //Debug.Log("diff.z : "+diff.z);
                //speed = agent.prevVertical * agent.speed / this.speed;
                //torque = agent.prevHorizontal * agent.torque / this.torque;
                //rotation = agent.transform.rotation;
            }
        }
        return hit.distance >= 0 ? (hit.distance / RAY_DIST) * Random.Range(1-noise, 1+noise) : -1f;
    }

    private void addElement(int detectedId, Vector3 otherAgentPosition)
    {
        if (!this.detectedFrontCarIdList.Contains(detectedId))
        {
            if (Mathf.Abs(transform.localPosition.x - otherAgentPosition.x) < 1.5f) //attention
            {
                //Debug.Log("detect new car forward");
                if (this.detectedFrontCarIdList.Count >= 5)
                {
                    this.detectedFrontCarIdList.RemoveAt(0);
                }
                this.detectedFrontCarIdList.Add(detectedId);
            }
        }
    }

    private void removeElement(int detectedId)
    {
        if (this.detectedFrontCarIdList.Contains(detectedId))
        {
            //Debug.Log("remove car id");
            this.detectedFrontCarIdList.Remove(detectedId);
            carInformation.passingCounter++;
        }
    }

    // private void shiftIndexes()
    // {
    //     for (int i = 0; i < this.detectedFrontCarIdList.Count - 1; i++)
    //     {
    //         this.detectedFrontCarIdList[i] = this.detectedFrontCarIdList[i + 1];
    //     }
    //     this.detectedFrontCarIdList.RemoveAt(this.detectedFrontCarIdList.Count - 1);
    // }

    private void ChoiceDiff(Vector3 diff, ref List<float> observations)
    {
        if (diffXYZ[0])
        {
            observations.Add(diff.x);
        }

        if (diffXYZ[1])
        {
            observations.Add(diff.y);
        }

        if (diffXYZ[2])
        {
            observations.Add(diff.z);
        }
    }

    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            //transform.localPosition = Vector3.zero;
            //transform.localPosition = new Vector3(0, 0, 5 - id * 7);
            transform.localPosition = _initPosition;
            transform.localRotation = _initRotation;
            this.speed = Random.Range(minSpeed, maxSpeed+1);
            //transform.localRotation = Quaternion.identity;
            //time = 0;
            if (changeColor)
            {
                frame.GetComponent<ColorController>().ChangeColor(this.speed, maxSpeed, minSpeed);
            }
            //Debug.Log("Generaite Episode Begin" );
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        crash.CrashProcess(other);
    }
}