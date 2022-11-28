using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;



public class BeeClust : MonoBehaviour
{

    public float maxSensingRange;
    public bool simpleFollowTemp = true;  // using temp sensors to follow the gradient of temp?
    public float antennaAngle = 90.0f;   // arc angle between forward and the antenaes*/
    public float tempSensorLen = 1.0f; // length of temp sensors from the center of the robot
    public bool vectorAvgFollowTemp = false; // whether we use vector averaging with more sensors to calculate the turn angle after waiting
    private Vector3 tempSensorE; //Right temp sensor
    private Vector3 tempSensorW;//Left temp sensor
    private Vector3 tempSensorNW;//45 degrees left from forward
    private Vector3 tempSensorNE;//45 degrees lright from forward
    private Vector3 tempSensorN;//Forward temp sensor

    
    public float forwardSpeed;
    public float turnSpeed;   // hopw fast the robot can turn
    public float turnAngle1;
    public float turnAngle2;
    public float turnAngle3;
    public float sensor2angle;
    public float sensor3angle;
    public float wMax = 60.0f;
    public float theta = 1000.0f;
    public int s;  // temperature where the robot is
    public float w;
    public bool waiting = false;
    public bool frontSensingOnly = false;
    public int fixedTempTurnAngle = 90; // angle by which it turns towards the "hotter" sensor

    private RaycastHit frontSens; 
    private RaycastHit leftSens;
    private RaycastHit rightSens;
    private Vector3 irLeft1;
    private Vector3 irLeft2;
    private Vector3 irRight1;
    private Vector3 irRight2;
    private float turnAngle;         // how much to turn 
    private bool takeOver = false;   // if the robot is in waiting state
    private bool turning = false;    // if its time to turn. In random movement, we move forward a bit then turn and repeat
    private Quaternion startBearing;   // rotations and stuff
    private Vector3 initPos;
    private Vector3 nextPosition;
    Parameters master;

    public string path_Collision, path_State, path_Parameters;  // path to appropriate results files
    public string fileDir;
    public int collision;
    private bool oncue = false;

    // Use this for initialization
    void Start()
    {
        /*Vector3 a = new Vector3(1, 0, 1);
        Vector3 b = new Vector3(-2, 0, -1);
        Debug.Log("test angle: "+DirectionalAngle(a.normalized, b.normalized));*/
        master = GameObject.Find("Master").GetComponent<Parameters>();

        fileDir = master.fileDir;

        path_Collision = fileDir + "/" + System.DateTime.Now.ToString("dd-MM-yy_hhmmss") + "_Collision" + ".txt";
        if (!File.Exists(path_Collision))
        {
            File.WriteAllText(path_Collision, "Run\tTime\tName\tX_pos\tY_pos\tCollision\tOnCue\n");
        }

        path_State = fileDir + "/" + System.DateTime.Now.ToString("dd-MM-yy_hhmmss") + "_State" + ".txt";
        if (!File.Exists(path_State))
        {
            File.WriteAllText(path_State, "Run\tTime\tName\tX_pos\tY_pos\tState\tDelay\tOnCue\n");
        }

    }

    // Update is called once per frame
    void Update()
    {   
        // adjust time scale in game
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 5;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 10;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Time.timeScale = 30;
        }


        if (master.run)
        {
            RunSensors();
            if (!takeOver)   // if the robot is not in waiting stage
            {
                if (turning)  //if its time to turn. Either because we finished waiting or because we hit a wall
                {
                    
                    Turn();
                }
                else
                {
                    MoveForward();
                    // check the IR proximity sensors
                    // check for collisions. transform.position = robot position; transform.forward = direction of robot. details of hits -> frontSens 
                    if (Physics.Raycast(transform.position, transform.forward, out frontSens, maxSensingRange))
                    {
                        if (!turning)
                        {
                            startBearing = transform.rotation;
                            /*bool boolValue = (Random.Range(0, 2) == 0);   // Random.Range returns int with [minInclusive..maxExclusive). i.e. 0,1
                            if (boolValue)
                            {
                                turnAngle = turnAngle1;
                            }
                            else
                            {
                                turnAngle = -turnAngle1;
                            }*/
                            turnAngle = GetRandomTurnAngle();
                            if (frontSens.transform.gameObject.CompareTag("MONA")) //if the hit is another robot
                            {
                                WaitFunction();   // wait then turning =True and turnAngle = towards hot
                            }
                            else
                            {
                                turning = true;
                            }
                        }
                    }
            
                    if (Physics.Raycast(transform.position, irLeft1, out frontSens, maxSensingRange))
                    {
                        if (!turning)
                        {
                            startBearing = transform.rotation;
                            //turnAngle = turnAngle2;
                            turnAngle = GetRandomTurnAngle();
                            if (frontSens.transform.gameObject.CompareTag("MONA") && !frontSensingOnly)
                            {
                                WaitFunction();
                            }
                            else
                            {
                                turning = true;
                            }
                        }
                    }
                    if (Physics.Raycast(transform.position, irLeft2, out frontSens, maxSensingRange))
                    {
                        if (!turning)
                        {
                            startBearing = transform.rotation;
                            //turnAngle = turnAngle3;
                            turnAngle = GetRandomTurnAngle();
                            if (frontSens.transform.gameObject.CompareTag("MONA") && !frontSensingOnly)
                            {
                                WaitFunction();
                            }
                            else
                            {
                                turning = true;
                            }
                        }
                    }
                    if (Physics.Raycast(transform.position, irRight1, out frontSens, maxSensingRange))
                    {
                        if (!turning)
                        {
                            startBearing = transform.rotation;
                            turnAngle = GetRandomTurnAngle();
                            if (frontSens.transform.gameObject.CompareTag("MONA") && !frontSensingOnly)
                            {
                                WaitFunction();
                            }
                            else
                            {
                                turning = true;
                            }
                        }
                    }
                    if (Physics.Raycast(transform.position, irRight2, out frontSens, maxSensingRange))
                    {
                        if (!turning)
                        {
                            startBearing = transform.rotation;
                             
                            turnAngle = GetRandomTurnAngle();
                            if (frontSens.transform.gameObject.CompareTag("MONA") && !frontSensingOnly)
                            {
                                WaitFunction();
                            }
                            else
                            {
                                turning = true;
                            }
                        }
                    }
                }
            }
        }
    }


    void RunSensors()
    {
        irLeft1 = Quaternion.AngleAxis(-sensor2angle, Vector3.up) * transform.forward;
        irLeft2 = Quaternion.AngleAxis(-sensor3angle, Vector3.up) * transform.forward;
        irRight1 = Quaternion.AngleAxis(sensor2angle, Vector3.up) * transform.forward;
        irRight2 = Quaternion.AngleAxis(sensor3angle, Vector3.up) * transform.forward;

        tempSensorE = Vector3.ClampMagnitude(Quaternion.AngleAxis(90, Vector3.up) * transform.forward, tempSensorLen);
        tempSensorW = Vector3.ClampMagnitude(Quaternion.AngleAxis(-90, Vector3.up) * transform.forward, tempSensorLen);
        tempSensorNW = Vector3.ClampMagnitude(Quaternion.AngleAxis(-45, Vector3.up) * transform.forward, tempSensorLen);
        tempSensorNE = Vector3.ClampMagnitude(Quaternion.AngleAxis(45, Vector3.up) * transform.forward, tempSensorLen);
        tempSensorN = Vector3.ClampMagnitude(transform.forward, tempSensorLen);
        // temp sensors
        Debug.DrawRay(transform.position, tempSensorW, Color.cyan);
        Debug.DrawRay(transform.position, tempSensorE,Color.cyan);
        Debug.DrawRay(transform.position, tempSensorNE, Color.cyan);
        Debug.DrawRay(transform.position, tempSensorNW, Color.cyan);
        Debug.DrawRay(transform.position, tempSensorN, Color.cyan);
        //forward
        Debug.DrawRay(transform.position, transform.forward * maxSensingRange, Color.magenta);
        //ir sensors
        Debug.DrawRay(transform.position, irLeft1 * maxSensingRange);
        Debug.DrawRay(transform.position, irLeft2 * maxSensingRange);
        Debug.DrawRay(transform.position, irRight1 * maxSensingRange);
        Debug.DrawRay(transform.position, irRight2 * maxSensingRange);
    }
    private float DirectionalAngle(Vector3 vec1, Vector3 vec2)
    {
        Vector2 to = new Vector2(vec1.x, vec1.z);
        Vector2 from = new Vector2(vec2.x, vec2.z);
        to = to.normalized;
        from = from.normalized;
        
        float det = (from.x * to.y) - (from.y * to.x);
        float dot = Vector2.Dot(from, to);

        return Mathf.Rad2Deg * (Mathf.Atan2(det, dot));
    }

    float GetTemp(int x, int y) {
        float temp;
        try
        {
            temp =master.tempEntries[x, y]; // gets the temperature at the left antenna
        }
        catch (System.IndexOutOfRangeException)
        {
            temp = 0;
            
        }
        return temp;
    }
    float GetVectorAvgAngle() {
        // get x and y of all antennae tips
        Vector3 tipE = transform.position + tempSensorE;
        int tipE_x = (int)Mathf.Round(tipE.x * 10.0f);
        int tipE_y = (int)Mathf.Round(tipE.z * 10.0f);

        Vector3 tipW = transform.position + tempSensorW;
        int tipW_x = (int)Mathf.Round(tipW.x * 10.0f);
        int tipW_y = (int)Mathf.Round(tipW.z * 10.0f);

        Vector3 tipNW = transform.position + tempSensorNW;
        int tipNW_x = (int)Mathf.Round(tipNW.x * 10.0f);
        int tipNW_y = (int)Mathf.Round(tipNW.z * 10.0f);

        Vector3 tipNE = transform.position + tempSensorNE;
        int tipNE_x = (int)Mathf.Round(tipNE.x * 10.0f);
        int tipNE_y = (int)Mathf.Round(tipNE.z * 10.0f);

        Vector3 tipN = transform.position + tempSensorN;
        int tipN_x = (int)Mathf.Round(tipN.x * 10.0f);
        int tipN_y = (int)Mathf.Round(tipN.z * 10.0f);

        //get temperature at tips
        float tipE_temp = GetTemp(tipE_x, tipE_y);
        float tipW_temp = GetTemp(tipW_x, tipW_y);
        float tipNE_temp = GetTemp(tipNE_x, tipNE_y);
        float tipNW_temp = GetTemp(tipNW_x, tipNW_y);
        float tipN_temp = GetTemp(tipN_x, tipN_y);

        Vector3 wVectorSum = tempSensorE * tipE_temp + tempSensorW * tipW_temp + tempSensorNE * tipNE_temp + tempSensorNW * tipNW_temp + tempSensorN * tipN_temp;
        if (wVectorSum.magnitude == 0) {

            return GetRandomTurnAngle();
        }
        else {
            float angle = DirectionalAngle(transform.forward.normalized, wVectorSum.normalized);
            Debug.Log("Vector Sum: " + wVectorSum.normalized + "  forward: " + transform.forward.normalized + "   angle: " + angle.ToString());
            return angle;
        }
        /*if (angle == 0) {
            angle = GetRandomTurnAngle();
        }*/
        
    }
    float GetSimpleTempAngle() {
        Vector3 rightTip = transform.position + tempSensorE;
        int rightTipX = (int)Mathf.Round(rightTip.x * 10.0f);
        int rightTipY = (int)Mathf.Round(rightTip.z * 10.0f);
        //left antenna

        Vector3 leftTip = transform.position + tempSensorW;
        int leftTipX = (int)Mathf.Round(leftTip.x * 10.0f);
        int leftTipY = (int)Mathf.Round(leftTip.z * 10.0f);
        //get temps at each antenna
        float leftTemp = GetTemp(leftTipX, leftTipY);
        float rightTemp = GetTemp(rightTipX, rightTipY);
        /*Debug.Log("Left temp = " + leftTemp);
        Debug.Log("Right temp = " + rightTemp);
        Debug.Log("********");*/
        float turnAngle;
        //get rotation angle
        if (leftTemp > rightTemp)
        {
            turnAngle = -fixedTempTurnAngle;

        }
        else if (rightTemp > leftTemp)
        {
            turnAngle = fixedTempTurnAngle;

        }
        else
        {
            turnAngle = GetRandomTurnAngle();

        }
        return turnAngle;

    }
    void SetTurnAngle()//get the angle to turn based on the 
    {
        if (simpleFollowTemp)
        {
            turnAngle = GetSimpleTempAngle();

        }
        else if (vectorAvgFollowTemp)
        {
            turnAngle = GetVectorAvgAngle();
        }
        else {
            turnAngle = GetRandomTurnAngle();
        }
    }
    int GetRandomTurnAngle() {
        
        int[] angleOptions = new int[] { -90, -80, -70, -60, -50,  90, 80, 70, 60, 50}; // removed smaller angles because those angles will most likely point towards the wall it collided with
        int index = Random.Range(0,angleOptions.Length);
        return angleOptions[index];
    }
    void Turn()
    {
        
        
        if (turnAngle < 0)
        {
            gameObject.transform.RotateAround(transform.position, Vector3.up, -turnSpeed * Time.deltaTime);
        }
        else
        {
            gameObject.transform.RotateAround(transform.position, Vector3.up, turnSpeed * Time.deltaTime);   //turn a bit
        }
        if (Mathf.Abs(Quaternion.Angle(startBearing, transform.rotation)) >= Mathf.Abs(turnAngle))  // if we've turned enough
        {
            turning = false;
        }
    }

    void MoveForward()
    {
        
        nextPosition = transform.position + transform.forward * forwardSpeed * Time.deltaTime;
        // check for collision with outer walls
        if (nextPosition.x > (0 + master.beeWidth / 2) && nextPosition.x < (master.length - master.beeWidth / 2) && nextPosition.z > (0 + master.beeLength / 2) && nextPosition.z < (master.width - master.beeLength / 2))
        {
            transform.position += transform.forward * forwardSpeed * Time.deltaTime;

        }
        else
        {
            turning = true;

        }

        
        bool newOnCue = isOnCue();

        if (newOnCue != oncue) { // if oncue changes. write to file so we can track. Don't write otherwise because it's redundant
            oncue = newOnCue;
            WriteToCollisionsFile();
            //Debug.Log("On cue changed to: " + oncue.ToString());
        }


    }
    void WaitFunction()
    {
        
        if (master.tempPath != "")
        {

            int xTemp = (int)Mathf.Round(transform.position.x * 10.0f);
            int yTemp = (int)Mathf.Round(transform.position.z * 10.0f);

            s = master.tempEntries[master.width * 10 - 1 - yTemp, xTemp]; // gets the temperature at the robot

            w = wMax * (Mathf.Pow(s, 2.0f) / (Mathf.Pow(s, 2.0f) + theta)); //waiting time 
        }
        else
        {
            w = 0;
        }
        //Collision
        collision++;
        //bool oncue = false;
        if (s != 0)// if its waiting after colliding with another robot.
        {
            oncue = true;
        }
        else
        {
            oncue = false;
        }

        float time = Time.time - master.startTime;
        
        StreamWriter writer = new StreamWriter(path_Collision, true); // write to the collision results file

        writer.WriteLine(master.iteration + "\t" + time.ToString("F2") + "\t" + gameObject.name + "\t" + transform.localPosition.x.ToString("F2") + "\t" + transform.localPosition.z.ToString("F2") + "\t" + collision + "\t" + oncue);
        writer.Close();
        /*StreamWriter writer_state = new StreamWriter(path_State, true);

        writer_state.WriteLine(master.iteration + "\t" + time.ToString("F2") + "\t" + gameObject.name + "\t" + transform.localPosition.x.ToString("F2") + "\t" + transform.localPosition.z.ToString("F2") + "\t" + "0" + "\t" + w + "\t" + oncue);
        writer_state.Close();*/
        SetTurnAngle(); //depending on whether it is set to use simple temp follow, vector avg or none(random)
        //Debug.Log("Intelligent turn angle: " + turnAngle.ToString());
        if (w > 0) { 
            waiting = true;
        }
        StartCoroutine(Delay(w));
    }
    void WriteToCollisionsFile() {
        float time = Time.time - master.startTime;

        StreamWriter writer = new StreamWriter(path_Collision, true); // write to the collision results file

        writer.WriteLine(master.iteration + "\t" + time.ToString("F2") + "\t" + gameObject.name + "\t" + transform.localPosition.x.ToString("F2") + "\t" + transform.localPosition.z.ToString("F2") + "\t" + collision + "\t" + oncue);
        writer.Close();

    }
    bool isOnCue() {
        //get temp at robot
        int xTemp = (int)Mathf.Round(transform.position.x * 10.0f);
        int yTemp = (int)Mathf.Round(transform.position.z * 10.0f);

        s = master.tempEntries[master.width * 10 - 1 - yTemp, xTemp]; // gets the temperature at the robot
        if (s != 0)
        {
            
            return true;
        }
        else
        {
            
            return false;
        }
    }
IEnumerator Delay(float w)
    {
        takeOver = true;
        yield return new WaitForSeconds(w);
        waiting = false;
        takeOver = false;
        turning = true;
    }
}


