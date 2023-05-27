using System.Collections;
using System.Collections.Generic;
using MLAgents;
using MLAgents.Sensors;
using UnityEngine;

public class CarAgent : Agent
{
    public float speed = 10f;
    public float maxSpeed = 15f;
    public float minSpeed = 5f;
    public float torque = 10f;
    private float prevVertical = 0f;
    private float prevHorizontal = 0f;

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

    public CarInformation carInformation;
    public float rewardRate = 1;
    private bool getReward = true;

    public GameObject frame;

    private string crossTag = "";

    public int testCount = 0;
    public int testStopCount = 5;
    private float[] p = new float[] {-1.9f, 0.0f, 1.9f};
    public int limitCarNum = 30;

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
        if(time > generateInterval && carInformation.carNum < limitCarNum){
            //Debug.Log("add new car");
            var gameObject = Instantiate(this, _initPosition, _initRotation);
            Debug.Log(_initPosition.x+p[carInformation.startPositionX]);
            new_id += 2;
            gameObject.id = new_id;
            gameObject.transform.parent = this.transform.parent.gameObject.transform;
            gameObject.transform.localPosition = new Vector3(p[carInformation.startPositionX],_initPosition.y, _initPosition.z);
            gameObject.transform.localRotation = _initRotation;
            gameObject.speed = Random.Range(minSpeed, maxSpeed+1);
            gameObject.getReward = true;
            gameObject.frame.GetComponent<ColorController>().ChangeColor(gameObject.speed);
            if (_track.GetComponent<Collider>().tag == "startTile01")
            {
                crossTag = "intersection";
            }
            //Debug.Log(gameObject.transform);
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
            // evaluator.addRewardLog(Time.realtimeSinceStartup, id, carInformation.reward * rewardRate, "ThroughPutReward");
            carInformation.getRewardCarNum++;
            getReward = false;
        }
    }

    void CarInformationController()
    {
        carInformation.rewardTime++;
        
        if (carInformation.rewardTime == carInformation.rewardInterval)
        {
            carInformation.reward = carInformation.throughCarNum / carInformation.carNum;
            //Debug.Log(carInformation.throughCarNum);
            //Debug.Log(carInformation.reward);
            
            if (testStopCount != 0)
            {
                Debug.Log("car num : " + carInformation.carNum);
                Debug.Log("throughput : " + carInformation.throughCarNum);
                Debug.Log("total car num : " + carInformation.totalCarNum);
                Debug.Log("crash : " + (evaluator.getNumCrash()-carInformation.crashCarNumLog));
                Debug.Log("crash rate : " + (((float)evaluator.getNumCrash()-carInformation.crashCarNumLog) / (float)carInformation.totalCarNum));
                Debug.Log("------------");
                testCount += 1;
                if (testStopCount == testCount)
                {
                    Debug.Break();
                }
            }
            carInformation.totalCarNumLog += carInformation.totalCarNum-carInformation.carNumLog;
            carInformation.totalCarNum = (int)carInformation.carNum;
            carInformation.carNumLog = (int)carInformation.carNum;
            carInformation.crashCarNumLog = evaluator.getNumCrash();
            carInformation.throughCarNum = 0;
        }

        if (carInformation.getRewardCarNum >= carInformation.carNum)
        {
            carInformation.rewardTime = 0;
            carInformation.getRewardCarNumLog = carInformation.getRewardCarNum;
            carInformation.getRewardCarNum = 0;
        }
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
        //origin
        //float angle = Vector3.Angle(moveVec, _track.forward);
        float angle;
        if (crossTag.Equals(_track.GetComponent<Collider>().tag))
        {
            angle = Vector3.Angle(moveVec, _track.right);
        }
        else
        {
            angle = Vector3.Angle(moveVec, _track.forward);
        }
        //float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        //float bonus = (1f - angle / 90f) * Mathf.Clamp01(Mathf.Abs(vertical)) * Time.fixedDeltaTime;
        //float bonus = ((1f - angle / 90f) + vertical) * Time.fixedDeltaTime;
        float bonus = ((1f - angle / 90f) * Mathf.Clamp01(Mathf.Max(0, vertical)) + Mathf.Min(0, vertical)) * Time.fixedDeltaTime;
        AddReward(bonus + reward);
        // evaluator.addRewardLog(Time.realtimeSinceStartup, id, bonus, "Bonus");
        // evaluator.addRewardLog(Time.realtimeSinceStartup, id, reward, "Tile Reward");
        if(foundCarBackward && !foundCarSide){
            evaluator.addBehavior(Time.realtimeSinceStartup, speed, false, vectorAction);
        }
        if(foundCarForward && !foundCarSide){
            evaluator.addBehavior(Time.realtimeSinceStartup, speed, true, vectorAction);
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
        // origin
        //float angle = Vector3.SignedAngle(_track.forward, transform.forward, Vector3.up);
        float angle;
        if (crossTag.Equals(_track.GetComponent<Collider>().tag))
        {
            angle = Vector3.SignedAngle(_track.right, transform.forward, Vector3.up);
        }
        else
        {
            angle = Vector3.SignedAngle(_track.forward, transform.forward, Vector3.up);
        }
        foundCarBackward = false;
        foundCarForward = false;
        foundCarSide = false;

        observations.Add(angle / 180f);//x0
        //float speed, torque;
        string tag;
        //Quaternion rotation;
        Vector3 diff;
        //vectorSensor.AddObservation(ObserveRay(1.5f, .5f, 25f, out speed, out torque, out rotation, out tag));

        //origin
        observations.Add(ObserveRay(1.5f, .5f, 25f, out diff, out tag)); //front right x1
        //observations.Add(ObserveRay(1.5f, .5f, 45f, out diff, out tag)); //change angle

        //vectorSensor.AddObservation((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //vectorSensor.AddObservation(speed);
        //vectorSensor.AddObservation(torque);
        observations.Add(diff.x); //x2
        //observations.Add(diff.y);
        observations.Add(diff.z); //x3
        if(tag == "car"){
            observations.Add(1);//x4
            foundCarForward = true;
        }
        else{
            observations.Add(0);//x4
        }
        if(tag == "wall"){
            observations.Add(1);//x5
        }
        else{
            observations.Add(0);//x5
        }
        //vectorSensor.AddObservation(ObserveRay(1.5f, 0f, 0f, out speed, out torque, out rotation, out tag));
        observations.Add(ObserveRay(1.5f, 0f, 0f, out diff, out tag)); //front x6
        //vectorSensor.AddObservation((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //vectorSensor.AddObservation(speed);
        //vectorSensor.AddObservation(torque);
        observations.Add(diff.x);//x7
        //observations.Add(diff.y);
        observations.Add(diff.z);//x8
        if(tag == "car"){
            observations.Add(1);//x9
            foundCarForward = true;
        }
        else{
            observations.Add(0);//x9
        }
        if(tag == "wall"){
            observations.Add(1);//x10
        }
        else{
            observations.Add(0);//x10
        }
        //observations.Add(ObserveRay(1.5f, -.5f, -25f, out speed, out torque, out rotation, out tag));

        //origin
        observations.Add(ObserveRay(1.5f, -.5f, -25f, out diff, out tag)); //front left x11
        //observations.Add(ObserveRay(1.5f, -.5f, -45f, out diff, out tag)); //change angle

        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x12
        //observations.Add(diff.y);
        observations.Add(diff.z);//x13
        if(tag == "car"){
            observations.Add(1);//x14
            foundCarForward = true;
        }
        else{
            observations.Add(0);//x14
        }
        if(tag == "wall"){
            observations.Add(1);//x15
        }
        else{
            observations.Add(0);//x15
        }
        //observations.Add(ObserveRay(-1.5f, .5f, 155f, out speed, out torque, out rotation, out tag));

        //origin
        observations.Add(ObserveRay(-1.5f, .5f, 155f, out diff, out tag)); //rear right x16
        //observations.Add(ObserveRay(-1.5f, .5f, 135f, out diff, out tag)); //change angle

        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x17
        //observations.Add(diff.y);
        observations.Add(diff.z);//x18
        if(tag == "car"){
            observations.Add(1);//x19
            foundCarBackward = true;
        }
        else{
            observations.Add(0);//x19
        }
        if(tag == "wall"){
            observations.Add(1);//x20
        }
        else{
            observations.Add(0);//x20
        }
        //observations.Add(ObserveRay(-1.5f, 0, 180f, out speed, out torque, out rotation, out tag));
        observations.Add(ObserveRay(-1.5f, 0f, 180f, out diff, out tag)); //rear x21
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x22
        //observations.Add(diff.y);
        observations.Add(diff.z);//x23
        if(tag == "car"){
            observations.Add(1);//x24
            foundCarBackward = true;
        }
        else{
            observations.Add(0);//x24
        }
        if(tag == "wall"){
            observations.Add(1);//x25
        }
        else{
            observations.Add(0);//x25
        }
        //observations.Add(ObserveRay(-1.5f, -.5f, -155f, out speed, out torque, out rotation, out tag));

        //origin
        observations.Add(ObserveRay(-1.5f, -.5f, -155f, out diff, out tag)); //rear left x26
        //observations.Add(ObserveRay(-1.5f, -.5f, -135f, out diff, out tag)); //change angle

        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x27
        //observations.Add(diff.y);
        observations.Add(diff.z);//x28
        if(tag == "car"){
            observations.Add(1);//x29
            foundCarBackward = true;
        }
        else{
            observations.Add(0);//x29
        }
        if(tag == "wall"){
            observations.Add(1);//x30
        }
        else{
            observations.Add(0);//x30
        }
        //observations.Add(ObserveRay(0f, .5f, 90f, out speed, out torque, out rotation, out tag));
        observations.Add(ObserveRay(0f, .5f, 90f, out diff, out tag)); //right x31
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x32
        //observations.Add(diff.y);
        observations.Add(diff.z);//x33
        if(tag == "car"){
            observations.Add(1);//x34
            foundCarSide = true;
        }
        else{
            observations.Add(0);//x34
        }
        if(tag == "wall"){
            observations.Add(1);//x35
        }
        else{
            observations.Add(0);//x35
        }
        //observations.Add(ObserveRay(0f, -.5f, -90f, out speed, out torque, out rotation, out tag));
        observations.Add(ObserveRay(0f, -.5f, -90f, out diff, out tag)); //left x36
        //observations.Add((180.0f + Quaternion.Angle(rotation, this.transform.rotation)) / 360.0f);
        //observations.Add(speed);
        //observations.Add(torque);
        observations.Add(diff.x);//x37
        //observations.Add(diff.y);
        observations.Add(diff.z);//x38
        if(tag == "car"){
            observations.Add(1);//x39
            foundCarSide = true;
        }
        else{
            observations.Add(0);//x39
        }
        if(tag == "wall"){
            observations.Add(1);//x40
        }
        else{
            observations.Add(0);//x40
        }
        observations.Add(this.speed);//x41
        observations.Add(this.torque);//x42
        foreach(var v in observations)
        {
            vectorSensor.AddObservation(v);
        }
        prev_observations = observations;
    }

    //private float ObserveRay(float z, float x, float angle, out float speed, out float torque, out Quaternion rotation, out string tag)
    private float ObserveRay(float z, float x, float angle, out Vector3 diff, out string tag)
    {
        //speed = 0.0f;
        //torque = 0.0f;
        //rotation = Quaternion.identity;
        diff = Vector3.zero;
        tag = "none";
        var tf = transform;
    
        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f; 
        const float RAY_DIST = 5f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;
        RaycastHit hit;
        
        //laser visualization
        //Ray ray = new Ray(position, dir);
        //Debug.DrawRay(ray.origin, ray.direction*RAY_DIST, Color.red);
    
        // See if there is a hit in the given direction
        var rayhit = Physics.Raycast(position, dir, out hit, RAY_DIST);
        if(rayhit){
            tag = hit.collider.tag;
            if(hit.collider.tag == "car"){
                CarAgent agent = hit.collider.gameObject.GetComponent(typeof(CarAgent)) as CarAgent;
                var self_dir = Quaternion.Euler(0, this.torque * this.prevHorizontal * 90f, 0) * (this.transform.forward * this.prevVertical * this.speed);
                var agent_dir = Quaternion.Euler(0, agent.torque * agent.prevHorizontal * 90f, 0) * (agent.transform.forward * agent.prevVertical * agent.speed);
                diff = agent_dir - self_dir;

                //speed = agent.prevVertical * agent.speed / this.speed;
                //torque = agent.prevHorizontal * agent.torque / this.torque;
                //rotation = agent.transform.rotation;
            }
        }
        return hit.distance >= 0 ? (hit.distance / RAY_DIST) * Random.Range(1-noise, 1+noise) : -1f;
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
                if(newHit == _prev_track)
                { // 1回前のタイルに移動したらペナルティ
                    reward = -1;
                }
                else
                { // 前向きに移動していたら+1, 後ろ向きに移動していたら-1
                    // origin
                    //float angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                    float angle;
                    if (crossTag.Equals(_track.GetComponent<Collider>().tag))
                    {
                        angle = Vector3.Angle(_track.right, newHit.position - _track.position);
                    }
                    else
                    {
                        angle = Vector3.Angle(_track.forward, newHit.position - _track.position);
                    }
                    //reward = (angle < 90f) ? 1 : -1;
                    if (angle < 90f)
                    {
                        reward = trackReward;
                        if (hit.collider.tag == "CheckPoint")
                        {
                            carInformation.throughCarNum++;
                        }
                        if (hit.collider.tag == "endTile")
                        {
                            //transform.localPosition = new Vector3(transform.localPosition.x,_initPosition.y, _initPosition.z);
                            EndEpisode();
                            //transform.localPosition = _initPosition;
                            //transform.localRotation = _initRotation;
                        }
                    }
                    else
                    {
                        reward = -1;
                    }
                }
                if (newHit.GetComponent<Collider>().tag == "startTile" || newHit.GetComponent<Collider>().tag == "startTile01")
                {
                    evaluator.addThroughCars(Time.realtimeSinceStartup);
                }
                _prev_track = _track;
                _track = newHit; 
            }
            else
            {
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
            carInformation.choicePositionX();
            transform.localPosition = new Vector3(p[carInformation.startPositionX],_initPosition.y, _initPosition.z);
            Debug.Log(p[carInformation.startPositionX]);
            transform.localRotation = _initRotation;
            this.speed = Random.Range(minSpeed, maxSpeed+1);
            frame.GetComponent<ColorController>().ChangeColor(speed);
            if (_track.GetComponent<Collider>().tag == "startTile01")
            {
                //Debug.Log(1);
                crossTag = "intersection";
            }
            //transform.localRotation = Quaternion.identity;
            //time = 0;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall") || other.gameObject.CompareTag("car"))
        {
            // increased from -1f -> -10f
            SetReward(-10f);
            // evaluator.addRewardLog(Time.realtimeSinceStartup, id, -10, "Crash");
            EndEpisode();

            if(other.gameObject.CompareTag("car"))
            {
                var otherAgent = (CarAgent)other.gameObject.GetComponent(typeof(CarAgent));
                if(this.id < otherAgent.id)
                {
                    evaluator.addCrashCars(Time.realtimeSinceStartup);
                    if(generateNew){
                        otherAgent.evaluator.addCrashCars(Time.realtimeSinceStartup);
                        Destroy(other.gameObject);
                        carInformation.carNum--;
                        carInformation.totalCarNum++;
                    }
                }
            }
            else
            {
                evaluator.addCrashCars(Time.realtimeSinceStartup);
                carInformation.totalCarNum++;
            }
        }
    }
}