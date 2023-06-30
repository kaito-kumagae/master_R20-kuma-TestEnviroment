using System.Collections;
using System.Collections.Generic;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;
using System.Linq;

public class CarAgent : Agent
{
    public float speed = 10f;
    public float maxSpeed = 15;
    public float minSpeed = 5;
    public float torque = 10f;
    public float prevVertical = 0f;
    public float prevHorizontal = 0f;

    public int score = 0;
    public bool resetOnCollision = true;

    public int id = 0;
    private int new_id = 0;

    private Transform _track, _prev_track;

    public int time = 0;
    public bool generateNew = true;
    public int generateInterval = 300;
    public float noise = 0.0f;

    private Vector3 _initPosition;
    private Quaternion _initRotation;
    //private int _notMoveCount = 0;
    private Evaluator evaluator = Evaluator.getInstance();

    public int trackReward = 1;

    
    public GameObject frame;
    public bool changeColor = true;
    
    public bool distancePenalty = true;
    public float[] PenaltyRewards = new float[8];

    public CarInformation carInformation; // すべての車の情報をもつオブジェクト
    //private int rewardTime = 0; // 全体報酬用タイマー
    public float rewardRate = 1;
    private bool getReward = true;
    //private int testCount = 0;
    public int testStopCount = 5;

    private float[] p = new float[] {-1.9f, 0f, 1.9f};

    public int limitCarNum = 30;
    public bool[] diffXYZ = new bool[] {true, false, true};
    public bool changeSpeed = true;

    public List<int> detectedFrontCarIdList = new List<int>(5);
    public bool countPassing = true;

    public override void Initialize()
    {
        GetTrackIncrement();
        _initPosition = transform.localPosition;
        _initRotation = transform.localRotation;
        time = 0;
        new_id = 0;
    }

    void Update()
    {
        
        if(!generateNew || id > 1) return;
        //Debug.Log(time);
        if(time > generateInterval && carInformation.carNum < limitCarNum)
        {
            //Debug.Log("add new car");
            // carInformation.choicePositionX();
            var gameObject = Instantiate(this, _initPosition, _initRotation);
            new_id += 2;
            gameObject.id = new_id;
            gameObject.transform.parent = this.transform.parent.gameObject.transform;
            gameObject.transform.localPosition = _initPosition;// new Vector3(p[carInformation.startPositionX], 0f, _initPosition.z);
            gameObject.transform.localRotation = _initRotation;
            gameObject.speed = Random.Range(minSpeed, maxSpeed+1);
            gameObject.getReward = true;
            gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed, maxSpeed, minSpeed);
            //Debug.Log("Generaiterate GenerateInterval");     
            carInformation.carNum++; 
            carInformation.totalCarNum++;
            time = 0;
        }
    }

    private void GetThroughPutReward()
    {
        if (carInformation.rewardTime >= carInformation.rewardInterval)
        {
            AddReward(carInformation.reward * rewardRate);
            carInformation.getRewardCarNum++;
            getReward = false;
        }
    }

    void CarInformationController()
    {
        carInformation.rewardTime++;

        if (Time.realtimeSinceStartup >= testStopCount)
        {
            PrintLog();
            Debug.Break();
        }
        
        if (carInformation.rewardTime == carInformation.rewardInterval)
        {
            carInformation.reward = carInformation.throughCarNum/carInformation.carNum;
            carInformation.totalCarNumLog += carInformation.totalCarNum;
            carInformation.totalCarNum = (int)carInformation.carNum;
            carInformation.carNum = (int)carInformation.carNum;
            carInformation.crashCarNumLog = evaluator.getNumCrash();
            carInformation.throughCarNum = 0;
            carInformation.passingCounter = 0;
        }    
    
        if (carInformation.getRewardCarNum >= carInformation.carNum)
        {
            carInformation.rewardTime = 0;
            carInformation.getRewardCarNumLog = carInformation.getRewardCarNum;
            carInformation.getRewardCarNum = 0;
        }
    }

    void PrintLog()
    {
        Debug.Log("throughput : " + carInformation.throughCarNum);
        //Debug.Log("total car num : " + carInformation.totalCarNum);
        //Debug.Log("存在する車の数 :  " + carInformation.carNum);
        Debug.Log("crash : " + (evaluator.getNumCrash()-carInformation.crashCarNumLog));
        //Debug.Log("crash rate : " + (((float)evaluator.getNumCrash()-carInformation.crashCarNumLog) / (float)carInformation.carNum));
        Debug.Log("passing : " + carInformation.passingCounter);
        Debug.Log("-------------------------------");
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        if(generateNew){
            time++;
        }
        
        
        if (carInformation.rewardTime < carInformation.rewardInterval) 
        { 
            if (! getReward) 
            {
                getReward = true;
            }  
        }
    
        float distance = speed * vertical;
        transform.Translate(distance * dt * Vector3.forward);

        float rotation = horizontal * torque * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
        prevHorizontal = horizontal;
        prevVertical = vertical;
    }
    private List<float> prev_observations;

    public override void OnActionReceived(float[] vectorAction)
    {
        if (id == 0)
        {
            CarInformationController();
        }

        if (getReward)
        {
            GetThroughPutReward();
        }
        
        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];
        vertical = Mathf.Clamp(vertical, -1.0f, 1.0f);
        horizontal = Mathf.Clamp(horizontal, -1.0f, 1.0f);

        var lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        float reward = GetTrackIncrement();
        
        var moveVec = transform.position - lastPos;
        /*
        if(Vector3.Distance(lastPos, transform.position) < 0.05f){
            _notMoveCount += 1;
            if(_notMoveCount > 10){
                _notMoveCount = 0;
                SetReward(-1f);
                EndEpisode();
            }
        }
        //*/
        float angle = Vector3.Angle(moveVec, _track.forward);
        //float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        //float bonus = (1f - angle / 90f) * Mathf.Clamp01(Mathf.Abs(vertical)) * Time.fixedDeltaTime;
        //float bonus = ((1f - angle / 90f) + vertical) * Time.fixedDeltaTime;
        float bonus = ((1f - angle / 90f) * Mathf.Clamp01(Mathf.Max(0, vertical)) + Mathf.Min(0, vertical)) * Time.fixedDeltaTime;
        AddReward(bonus + reward);
        if(foundCarBackward && !foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)speed, false, vectorAction);
        }
        if(foundCarForward && !foundCarSide)
        {
            evaluator.addBehavior(Time.realtimeSinceStartup, (int)speed, true, vectorAction);
        }

        score += (int)reward;

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
        //float speed, torque;
        string tag;
        //Quaternion rotation;
        Vector3 diff;

        int detectedId;

        Vector3 otherAgentPosition;

        float distance;

        //vectorSensor.AddObservation(ObserveRay(1.5f, .5f, 25f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(1.5f, .5f, 25f, out diff, out tag, out detectedId, out otherAgentPosition); //right forward
        DistancePenalty(distance, PenaltyRewards[0], 0.2f);
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
        //vectorSensor.AddObservation(ObserveRay(1.5f, 0f, 0f, out speed, out torque, out rotation, out tag));
        distance = ObserveRay(1.5f, 0f, 0f, out diff, out tag, out detectedId, out otherAgentPosition); //forward
        DistancePenalty(distance, PenaltyRewards[1], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[2], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[3], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[4], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[5], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[6], 0.2f);
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
        DistancePenalty(distance, PenaltyRewards[7], 0.2f);
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
        if(rayhit)
        {
            tag = hit.collider.tag;
            otherAgentPosition = hit.collider.transform.localPosition;
            if(hit.collider.tag == "car"){
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

    private void DistancePenalty(float distance, float penaltyReward, float penaltyDistance)
    {
        //Debug.Log("distance : " + distance);
        if ((0 < distance) && (distance < penaltyDistance))
        {
            if (distancePenalty)
            {
                AddReward(penaltyReward);
            }
        }
    }

    private float GetTrackIncrement()
    {
        float reward = 0;
        var carCenter = transform.position + Vector3.up;

        // Find what tile I'm on
        if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f))
        {
            var newHit = hit.transform;
            // Check if the tile has changed
            if(_track == null){
                _prev_track = _track;
                _track = newHit;
            }
            else if (newHit != _track) // 別のタイルに移動
            {
                var relPos = transform.position - newHit.position;
                evaluator.addHorizontalSensor(Time.realtimeSinceStartup, relPos.x * newHit.forward.z - relPos.z * newHit.forward.x, relPos.x * newHit.forward.x - relPos.z * newHit.forward.z, this.id, this.speed);
                if(newHit == _prev_track){ // 1回前のタイルに移動したらペナルティ
                    reward = -1;
                }
                else{ // 前向きに移動していたら+1, 後ろ向きに移動していたら-1
                    float angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                    if (angle < 90f)
                    {
                        if (hit.collider.tag == "CheckPoint")
                        {
                            carInformation.throughCarNum++; // 前進でチェックポイント踏んだらカウント  
                        }
                        reward = trackReward;
                        if (hit.collider.tag == "endTile")
                        {
                            // EndEpisode();
                            //transform.localPosition = new Vector3(transform.localPosition.x,_initPosition.y, _initPosition.z);
                            transform.localPosition = new Vector3(transform.localPosition.x,0, 0);
                            // transform.localPosition = _initPosition;
                            // transform.localRotation = _initRotation;
                            if (changeSpeed)
                            {
                                speed = Random.Range(minSpeed, maxSpeed+1);
                                frame.GetComponent<ColorController>().ChangeColor(this.speed, maxSpeed, minSpeed);
                            }
                            if (countPassing == true)
                            {
                                this.detectedFrontCarIdList.Clear();
                            }
                        }
                    }

                    else
                    {
                        reward = -1;
                    }
                }
                if (newHit.GetComponent<Collider>().tag == "startTile"){
                    evaluator.addThroughCars(Time.realtimeSinceStartup);
                }
                _prev_track = _track;
                _track = newHit;
            }
            else {
                reward = -0.01f;
            }
        }

        return reward;
    }

    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            //transform.localPosition = Vector3.zero;
            //transform.localPosition = new Vector3(0, 0, 5 - id * 7);
            //carInformation.choicePositionX();
            // transform.localPosition =  new Vector3(p[carInformation.startPositionX], 0f, _initPosition.z);
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
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("car"))
        {
            // increased from -1f -> -10f
            var carCenter = transform.position + Vector3.up;
            SetReward(-10f);
            EndEpisode();
            if (countPassing == true)
            {
                this.detectedFrontCarIdList.Clear();
            }
            if(other.gameObject.CompareTag("car"))
            {
                var otherAgent = (CarAgent)other.gameObject.GetComponent(typeof(CarAgent));
                if((this.id < otherAgent.id) && (isNot01(otherAgent.id)))
                {
                    if (Physics.Raycast(carCenter, Vector3.down, out var hit, 2f)) 
                    {
                        var newHit = hit.transform;
                        if (newHit.GetComponent<Collider>().tag == "startTile")
                        {
                            if(generateNew)
                            {
                                Destroy(other.gameObject);
                                carInformation.carNum--;
                                carInformation.totalCarNum++;
                            }
                        }

                        else 
                        {
                            evaluator.addCrashCars(Time.realtimeSinceStartup,speed);
                            if(generateNew)
                            {
                                otherAgent.evaluator.addCrashCars(Time.realtimeSinceStartup,otherAgent.speed);
                                Destroy(other.gameObject);
                                carInformation.carNum--;
                                carInformation.totalCarNum++;
                            }
                        }
                    }
                }
            }
            else
            {
                    evaluator.addCrashCars(Time.realtimeSinceStartup, speed);
                    //carInformation.totalCarNum++;
            }
        }
    }

    private bool isNot01(int otherAgentId)
    {
        if ((id == 0) || (id == 1))
        {
            if ((otherAgentId == 0) || (otherAgentId == 1))
            {
                return false;
            }
        }
        return true;
    }
}
