using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;



public class BeeClust : MonoBehaviour
{

    public float maxSensingRange;
    public bool followTemp = true;  // using temp sensors to follow the gradient of temp?
    public float antennaAngle = 90.0f;   // arc angle between forward and the antenaes*/
    public float antennaLength = 1.0f; // length of antenna from the center of the robot
    private Vector3 tempSensorRight;
    private Vector3 tempSensorLeft;
    
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
        master = GameObject.Find("Master").GetComponent<Parameters>();

        fileDir = master.fileDir;
        Debug.Log("fileDir in BeeClust.cs: " + fileDir);

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

        path_Parameters = fileDir + "/" + System.DateTime.Now.ToString("dd-MM-yy_hhmmss") + "_Parameters" + ".txt";
        if (!File.Exists(path_Parameters))
        {
            File.WriteAllText(path_Parameters, "Simulation Log:\n");
            StreamWriter writer = new StreamWriter(path_Parameters, true);
            writer.WriteLine("Date and Time: " + System.DateTime.Now.ToString("dd-MM-yy hh:mm:ss") + "\n");

            writer.WriteLine("Simulation:\n" + "Time: " + master.simTime);
            writer.WriteLine("Repeat count: " + master.simCount + "\n");
  
            writer.WriteLine("Arena:\n" + "Length: " + master.length);
            writer.WriteLine("Width: " + master.width+"\n");

            writer.WriteLine("Agents:\n" + "Counts: " + master.beeCount);
            //writer.WriteLine("Follow temp gradient: " + master.followTemp);
            writer.WriteLine("Length: " + master.beeLength);
            writer.WriteLine("Width: " + master.beeWidth);
            writer.WriteLine("Speed: " + master.beeSpeed);
            writer.WriteLine("Turning Speed: " + master.beeTurn);
            writer.WriteLine("Sensor Range: " + master.senseRange + "\n");

            writer.WriteLine("Initialization Postion:\n" + "X: from " + master.minX + " to " + master.maxX);
            writer.WriteLine("Y: from " + master.minY + " to " + master.maxY);

            writer.Close();
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
                            turnAngle = getRandomTurnAngle();
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
                            turnAngle = getRandomTurnAngle();
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
                            turnAngle = getRandomTurnAngle();
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
                            //turnAngle = -turnAngle2;
                            turnAngle = getRandomTurnAngle();
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
                            //turnAngle = -turnAngle3;
                            turnAngle = getRandomTurnAngle();
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

        tempSensorRight = Vector3.ClampMagnitude(Quaternion.AngleAxis(antennaAngle, Vector3.up) * transform.forward, antennaLength);
        tempSensorLeft = Vector3.ClampMagnitude(Quaternion.AngleAxis(-antennaAngle, Vector3.up) * transform.forward, antennaLength);
        // temp sensors
        Debug.DrawRay(transform.position, tempSensorLeft, Color.cyan);
        Debug.DrawRay(transform.position, tempSensorRight,Color.cyan);
        //forward
        Debug.DrawRay(transform.position, transform.forward * maxSensingRange, Color.magenta);
        //ir sensors
        Debug.DrawRay(transform.position, irLeft1 * maxSensingRange);
        Debug.DrawRay(transform.position, irLeft2 * maxSensingRange);
        Debug.DrawRay(transform.position, irRight1 * maxSensingRange);
        Debug.DrawRay(transform.position, irRight2 * maxSensingRange);
    }
    private void OnDrawGizmos() {
        
        
    }

    void GetTurnAngle()//get the angle to turn based on the temperature sensed at the antennaes
    {
        //get x and y of right antennae tip
 
        Vector3 rightTip = transform.position + tempSensorRight;
        int rightTipX = (int)Mathf.Round(rightTip.x * 10.0f);
        int rightTipY = (int)Mathf.Round(rightTip.z * 10.0f);
        //left antenna
 
        Vector3 leftTip = transform.position + tempSensorLeft;
        int leftTipX = (int)Mathf.Round(leftTip.x * 10.0f);
        int leftTipY = (int)Mathf.Round(leftTip.z * 10.0f);
        //get temps at each antenna
        int leftTemp;
        int rightTemp;
        try{
            leftTemp = master.tempEntries[leftTipY, leftTipX]; // gets the temperature at the left antenna
        }
        catch (System.IndexOutOfRangeException) {
            leftTemp = 0;
            //Debug.Log("Left antena is out of range");
        }

        try
        {
            rightTemp = master.tempEntries[rightTipY, rightTipX]; // gets the temperature at the right antenna
        }
        catch (System.IndexOutOfRangeException)
        {
            //Debug.Log("Right antena is out of range");
            rightTemp = 0;
        }
        /*Debug.Log("Left temp = " + leftTemp);
        Debug.Log("Right temp = " + rightTemp);
        Debug.Log("********");*/
        //get rotation angle
        if (leftTemp > rightTemp)
        {
            turnAngle = -fixedTempTurnAngle;
            
        }
        else if (rightTemp > leftTemp)
        {
            turnAngle = fixedTempTurnAngle;
            
        }
        else {
            turnAngle = getRandomTurnAngle();
            
        }
    }
    int getRandomTurnAngle() {
        
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
        StreamWriter writer_state = new StreamWriter(path_State, true);

        writer_state.WriteLine(master.iteration + "\t" + time.ToString("F2") + "\t" + gameObject.name + "\t" + transform.localPosition.x.ToString("F2") + "\t" + transform.localPosition.z.ToString("F2") + "\t" + "0" + "\t" + w + "\t" + oncue);
        writer_state.Close();
        if (followTemp == true) {
            GetTurnAngle();
        }
        
        Debug.Log("Intelligent turn angle: " + turnAngle.ToString());
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


