using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;


public class Parameters : MonoBehaviour
{
    public int width;
    public int length;
    public string obsPath;
    public string tempPath;
    //public string phePath;
    public int[,] tempEntries;
    public bool run = false;
    public int beeCount;
    public bool simpleFollowTemp;  //whether the turn towards the "hotter" temo sensor after waiting
    public bool vectorAvgFollowTemp;  //whether we use vector averaging with more sensors to get the turn angle after waiting
    public float beeWidth;
    public float beeLength;
    public float beeSpeed;
    public float beeTurn;
    public float senseRange;
    public float theta;
    public int minX = 0;
    public int maxX = 0;
    public int minY = 0;
    public int maxY = 0;
    public int digitCount;
    public float simTime;
    public int simCount;
    public string fileDir;
    public Text timerText;
    string path;
    //public string fileName;
    public int toggle = 1;
    public int iteration = 0;
    public float startTime;
    public float interval = 1;
    public int totalInstances;
    public int instance = 0;
    GameObject[] monas;

    private Renderer floorRenderer;
    private Texture tempTex;

    
    // Start is called before the first frame update
    void Start()
    {

        /*init();*/

    }
    public void init()
    {

        maxX = length;
        maxY = width;
        if (tempPath != "" && tempPath != null)
        {

            PopulateTemperatureArray();
        }
        //write parameter config to file
        string path_Parameters = fileDir + "/" + System.DateTime.Now.ToString("dd-MM-yy_hhmmss") + "_Parameters" + ".txt";
        File.WriteAllText(path_Parameters, "Simulation Log:\n");
        StreamWriter writer = new StreamWriter(path_Parameters, true);
        writer.WriteLine("Date and Time: " + System.DateTime.Now.ToString("dd-MM-yy hh:mm:ss") + "\n");

        writer.WriteLine("Simulation:\n" + "Time: " + simTime);
        writer.WriteLine("Repeat count: " + simCount + "\n");

        writer.WriteLine("Arena:\n" + "Length: " + length);
        writer.WriteLine("Width: " + width + "\n");

        writer.WriteLine("Agents:\n" + "Counts: " +beeCount);
        writer.WriteLine("Simple Temp follow: " +simpleFollowTemp);
        writer.WriteLine("Vector Avg temp follow: " + vectorAvgFollowTemp);
        writer.WriteLine("Theta: " + theta);

        writer.WriteLine("Length: " + beeLength);
        writer.WriteLine("Width: " + beeWidth);
        writer.WriteLine("Speed: " + beeSpeed);
        writer.WriteLine("Turning Speed: " + beeTurn);
        writer.WriteLine("Sensor Range: " + senseRange + "\n");

        writer.WriteLine("Initialization Postion:\n" + "X: from " + minX + " to " + maxX);
        writer.WriteLine("Y: from " + minY + " to " + maxY);

        writer.Close();

        path = fileDir + "/" + System.DateTime.Now.ToString("dd-MM-yy_hhmmss") + "_Position" + ".txt";
        if (!File.Exists(path))
        {
            File.WriteAllText(path, "Run\tTime\tName\tX_pos\tY_pos\n");
        }
        monas = GameObject.FindGameObjectsWithTag("MONA");
        run = true;
        totalInstances = (int)(simTime / interval);

        writer = new StreamWriter(path, true);
        writer.WriteLine();
        writer.Close();
        floorRenderer = GameObject.Find("Floor").GetComponent<Renderer>();
        tempTex = Resources.Load<Texture>("Textures/tempTexture");
        floorRenderer.material.mainTexture = tempTex;
        floorRenderer.material.SetColor("_Color", Color.white);
        InvokeRepeating("LogPosition", 0.0f, 1.0f);

        

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (toggle > 0)
            {
                toggle = 0;
            }
            else
            {
                toggle++;
            }
            if(toggle == 0)
            {
                floorRenderer.material.mainTexture = null;
                floorRenderer.material.SetColor("_Color", Color.blue);
            }
            if (toggle == 1)
            {
                
                
                floorRenderer.material.mainTexture = tempTex;
                floorRenderer.material.SetColor("_Color", Color.white);
            }
        }
        if (run)
        {
           
            timerText.text = "Run: " + (iteration + 1) + "\t\t" + "Elapsed Time: " + (Time.time - startTime).ToString("F2")+ "\r\n" + Time.timeScale + "x Speed" + "\r\n" + "\r\n" +"Press UP to show/hide temperature texture" + "\r\n" + "\r\n" + "Press 1 for 1x speed" + "\r\n" + "Press 2 for 5x speed" + "\r\n" + "Press 3 for 10x speed" + "\r\n" + "Press 4 for 30x speed" + "\r\n" + "\r\n" +  "BeeGround Ver.1.0" + "\r\n" + "\r\n" + "01 Obstacle: Examples of Obstacles Array" + "\r\n" + "02 Temperature: Examples of Temperature Array" + "\r\n" + "03 Results: Saving the simulation results";

        }
    }
 
    void PopulateTemperatureArray()
    {
        StreamReader reader = new StreamReader(tempPath);
        string fullData = reader.ReadToEnd();
        int i = 0, j = 0;
        tempEntries = new int[width * 10, length * 10];  // every unit in the arena is split into temperature sub units
        foreach (var row in fullData.Split('\n'))
        {
            j = 0;
            foreach (var col in row.Split(','))
            {
                if (int.TryParse(col, out int result))
                {
                    tempEntries[i, j] = int.Parse(col);
                    j++;
                }
            }
            i++;
        }

        reader.Close();
        
    }

    void LogPosition()
    {
        StreamWriter writer = new StreamWriter(path, true);
        float time = Time.time - startTime;
        foreach (GameObject mona in monas)
        {
            writer.WriteLine(iteration + "\t" + time.ToString("F2") + "\t" + mona.name + "\t" + mona.transform.localPosition.x.ToString("F2") + "\t" + mona.transform.localPosition.z.ToString("F2"));
        }
        writer.Close();
        instance++; //second intervals passed
        if(instance > totalInstances) // 
        {
            CancelInvoke();
            if (iteration < simCount - 1)   // if 
            {
                //resets after a full run if it hasn't completed all the runs
                Debug.Log("Reset");
                foreach (GameObject mona in monas)
                {
                    while (true)
                    {
                        int x = Random.Range(minX, maxX);
                        int y = Random.Range(minY, maxY);
                        Collider[] occupied = Physics.OverlapBox(new Vector3(x + 0.5f, 0.5f, y + 0.5f), new Vector3(0.45f, 0.45f, 0.45f), Quaternion.identity);
                        if (occupied.Length == 0)
                        {
                            mona.transform.position = new Vector3(x + 0.5f, 0.15f, y + 0.5f);
                            mona.transform.eulerAngles = new Vector3(0, Random.Range(0, 360), 0);
                            break;
                        }
                    }
                }
                iteration++;
                instance = 0;
                startTime = Time.time;
                InvokeRepeating("LogPosition", 0.0f, 1.0f);
            }
            else
            {
                Debug.Log("End");
                run = false;
            }
        }
    }
}
