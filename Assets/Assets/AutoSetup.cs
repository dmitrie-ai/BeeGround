using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
//Prepares a simulation without the need to go through the Settings menu. Fill in the parameters and just press play. It will automatically set everything up and run the simulation
public class AutoSetup : MonoBehaviour
{

    //Arena Settings
    public int width = 50;
    public int length = 50;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Mesh mesh;
    public string tempPath = "C:/Users/dmitr/Desktop/project/code/Bee-Ground_1.0/Bee-Ground_1.0/Assets/02 Temperature/Test_Temperature_1Cue_50_50.txt";
    Texture2D tempTexture;
    int[,] tempEntries;
    //string phePath = "";
    /*string obsPath = "";
    public int[,] obsEntries;
    
    Texture2D pheTexture;*/

    //Bee Settings
    public int beeCount = 30;
    float beeWidth = 0.8f;
    float beeLength = 0.8f;
    float beeSpeed = 2.0f;
    float beeTurn = 50.0f;
    float senseRange = 1.0f;
    float maxW = 60.0f;
    public float theta = 1000.0f;
    int minX = 0;
    int maxX = 50;
    int minY = 0;
    int maxY = 50;
    int digitCount;
    System.DateTime startDateTime;

    //Sim Settings
    public float simTime = 100;
    public int simCount = 1;
    public string fileDir = "C:/Users/dmitr/Desktop/project/code/Bee-Ground_1.0/Bee-Ground_1.0/Assets/03 Results";

    Parameters master;

    void Start()
    {
        // ===
        if (GameObject.Find("AutoPlayer") != null)
        {
            beeCount = AutoPlayer.startScene.beeNumbers; // needed for when we automate the runs. In AutoPlay Scene, parameter values are set and they are passed to the BeeClust scene one by one.
        } 
        BuildArena();
        GenerateBees();
        SimConfig();
    }


    void BuildArena()
    {
        
        ClearBees();
        ClearTestArea();
        CreateArena();
            



        if (tempPath != "")
        {
            BuildTempTexture();
        }
        else
        {
            BuildTempTextureNULL();
        }
        master = GameObject.Find("Master").GetComponent<Parameters>();
        master.width = width;
        master.length = length;
        //master.obsPath = obsPath;
        master.tempPath = tempPath;
        //master.phePath = phePath;
        GameObject camera = GameObject.Find("Main Camera");
        camera.transform.position = new Vector3((float)length / 2.0f, Mathf.Max(length, width) * 1.10f, (float)width / 2.0f);
       
    }

    void ClearTestArea()
    {
        if (GameObject.Find("Arena") != null)
        {
            DestroyImmediate(GameObject.Find("Arena"));
        }
        if (GameObject.Find("Obstacles") != null)
        {
            DestroyImmediate(GameObject.Find("Obstacles"));
        }
    }

    void ClearBees()

    {
        GameObject[] monas = GameObject.FindGameObjectsWithTag("MONA");
        foreach (GameObject mona in monas)
        {
            DestroyImmediate(mona);
        }
    }

    void CreateArena()
    {
        GameObject arena = new GameObject("Arena");

        GameObject floor = CreateFloor();

        GameObject westWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        westWall.name = "West Wall";
        westWall.transform.position = new Vector3(-0.25f, 1.0f, (float)width / 2.0f);
        westWall.transform.localScale = new Vector3(0.5f, 2.0f, width);
        westWall.AddComponent<Rigidbody>();
        westWall.GetComponent<Rigidbody>().useGravity = false;

        GameObject eastWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        eastWall.name = "East Wall";
        eastWall.transform.position = new Vector3((float)length + 0.25f, 1.0f, (float)width / 2.0f);
        eastWall.transform.localScale = new Vector3(0.5f, 2.0f, width);
        eastWall.AddComponent<Rigidbody>();
        eastWall.GetComponent<Rigidbody>().useGravity = false;

        GameObject northWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        northWall.name = "North Wall";
        northWall.transform.position = new Vector3((float)length / 2.0f, 1.0f, (float)width + 0.25f);
        northWall.transform.localScale = new Vector3(length + 1.0f, 2.0f, 0.5f);
        northWall.AddComponent<Rigidbody>();
        northWall.GetComponent<Rigidbody>().useGravity = false;

        GameObject southWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        southWall.name = "South Wall";
        southWall.transform.position = new Vector3((float)length / 2.0f, 1.0f, -0.25f);
        southWall.transform.localScale = new Vector3((float)length + 1.0f, 2.0f, 0.5f);
        southWall.AddComponent<Rigidbody>();
        southWall.GetComponent<Rigidbody>().useGravity = false;

        floor.transform.SetParent(arena.transform);
        eastWall.transform.SetParent(arena.transform);
        westWall.transform.SetParent(arena.transform);
        northWall.transform.SetParent(arena.transform);
        southWall.transform.SetParent(arena.transform);

    }

    static bool CheckArrayDimensions(string address, int length, int width)
    {
        StreamReader reader = new StreamReader(address);
        string fullData = reader.ReadToEnd();
        int i = 0, j = 0;
        int maxJ = 0;
        int[,] entries = new int[width, length];
        foreach (var row in fullData.Split('\n'))
        {
            j = 0;
            foreach (var col in row.Split(','))
            {
                if (int.TryParse(col, out int result))
                {
                    if (j >= length || i >= width)
                    {
                        reader.Close();
                        return false;
                    }
                    entries[i, j] = int.Parse(col);
                    j++;
                }
            }
            i++;
            if (maxJ < j)
            {
                maxJ = j;
            }
        }
        if (i - 1 != width || maxJ != length)
        {
            return false;
        }
        reader.Close();
        return true;
    }


    GameObject CreateFloor()
    {
        GameObject go = new GameObject("Floor");
        MeshFilter mf = go.AddComponent(typeof(MeshFilter)) as MeshFilter;
        MeshRenderer mr = go.AddComponent(typeof(MeshRenderer)) as MeshRenderer;
        mesh = new Mesh();
        go.GetComponent<MeshFilter>().mesh = mesh;
        (go.AddComponent(typeof(MeshCollider)) as MeshCollider).sharedMesh = mesh;
        CreateShape();
        UpdateMesh();
        go.GetComponent<Renderer>().material = Resources.Load<Material>("Materials/Default");
        return go;
    }

    void BuildTempTexture()
    {
        StreamReader reader = new StreamReader(tempPath);
        string fullData = reader.ReadToEnd();
        int i = 0, j = 0;
        tempEntries = new int[width * 10, length * 10];
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

        tempTexture = new Texture2D(length * 10, width * 10, TextureFormat.ARGB32, false);

        for (int z = 0; z < width * 10; z++)
        {
            for (int x = 0; x < length * 10; x++)
            {
                tempTexture.SetPixel(x, z, Color.Lerp(Color.blue, Color.red, (float)tempEntries[width * 10 - z - 1, x] / 255.0f));
            }
        }
        tempTexture.Apply();
        AssetDatabase.CreateAsset(tempTexture, "Assets/Resources/Textures/tempTexture.asset");

    }

    void BuildTempTextureNULL()
    {


        tempEntries = new int[width * 10, length * 10];


        tempTexture = new Texture2D(length * 10, width * 10, TextureFormat.ARGB32, false);

        for (int z = 0; z < width * 10; z++)
        {
            for (int x = 0; x < length * 10; x++)
            {
                tempTexture.SetPixel(x, z, Color.Lerp(Color.blue, Color.red, 0));
            }
        }
        tempTexture.Apply();
        AssetDatabase.CreateAsset(tempTexture, "Assets/Resources/Textures/tempTexture.asset");

    }

    void CreateShape()
    {
        vertices = new Vector3[4];

        vertices[0] = new Vector3(0, 0, 0);
        vertices[1] = new Vector3(0, 0, width);
        vertices[2] = new Vector3(length, 0, 0);
        vertices[3] = new Vector3(length, 0, width);

        triangles = new int[] { 0, 1, 2, 2, 1, 3 };

        uvs = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 0),
            new Vector2(1, 1)
        };
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateNormals();
    }

    void GenerateBees()
    {
        master = GameObject.Find("Master").GetComponent<Parameters>();
        GameObject prefab = Resources.Load<GameObject>("Prefabs/BEE");

        

        digitCount = (int)Mathf.Floor(Mathf.Log10(beeCount) + 1);


        for (int i = 0; i < beeCount; i++)
        {
            string id = i.ToString().PadLeft(digitCount, '0');
            string name = "Bee" + id;
            int x = Random.Range(minX, maxX);
            int y = Random.Range(minY, maxY);
            Collider[] occupied = Physics.OverlapBox(new Vector3(x + 0.5f, 0.5f, y + 0.5f), new Vector3(0.45f, 0.45f, 0.45f), Quaternion.identity);
            if (occupied.Length == 0)
            {
                GameObject bee = (GameObject)Instantiate(prefab, new Vector3(x + 0.5f, 0.15f, y + 0.5f), Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0)));
                bee.transform.localScale = new Vector3(beeWidth, 0.15f, beeLength);
                bee.name = name;
                bee.GetComponent<BeeClust>().forwardSpeed = beeSpeed;
                bee.GetComponent<BeeClust>().turnSpeed = beeTurn;
                bee.GetComponent<BeeClust>().maxSensingRange = senseRange;
                bee.GetComponent<BeeClust>().wMax = maxW;
                bee.GetComponent<BeeClust>().theta = theta;

            }
            else
            {
                i--;
            }
                
            GameObject[] monas = GameObject.FindGameObjectsWithTag("MONA");
            Debug.Log("Number of Bees: " + monas.Length);
            master = GameObject.Find("Master").GetComponent<Parameters>();
            master.beeCount = beeCount;
            master.beeWidth = beeWidth;
            master.beeLength = beeLength;
            master.beeSpeed = beeSpeed;
            master.beeTurn = beeTurn;
            master.senseRange = senseRange;
            master.minX = minX;
            master.minY = minY;
            master.maxX = maxX;
            master.maxY = maxY;
            master.digitCount = digitCount; 

        }
    }

    void SimConfig()
    {

        master = GameObject.Find("Master").GetComponent<Parameters>();
        master.simTime = simTime;
        master.simCount = simCount;
        master.fileDir = fileDir;
  
    }
    

}

