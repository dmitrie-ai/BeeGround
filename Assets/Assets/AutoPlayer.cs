using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AutoPlayer : MonoBehaviour
{
    public static AutoPlayer startScene; 
    public string beeCountsString;
    public string thetaValuesString;
    public int simTime = 100;
    public int simCount = 1;
    public static int numberDone= 0;
    public int beeCount;
    public float theta;
    // Start is called before the first frame update
    private void Awake()
    {
        if (startScene == null)
        {
            startScene = this;
            DontDestroyOnLoad(gameObject);

        }
        else {
            Destroy(gameObject);
        }
        
    }
    void Start()
    {
        string[] beeCounts = beeCountsString.Split(',');
        string[] thetaValues = thetaValuesString.Split(',');
        if (numberDone < beeCounts.Length) {
            theta = float.Parse(thetaValues[numberDone]);
            beeCount = int.Parse(beeCounts[numberDone]);
            numberDone = numberDone + 1;
            SceneManager.LoadScene("BeeGround", LoadSceneMode.Additive);

        }

    }

    
}
