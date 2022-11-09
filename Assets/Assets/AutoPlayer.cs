using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AutoPlayer : MonoBehaviour
{
    public static AutoPlayer startScene; 
    private static int simCount = 0;
    public int beeNumbers;
    public string thetaValues;
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
        SceneManager.LoadScene("BeeGround", LoadSceneMode.Additive);
        simCount = simCount + 1;

    }

    
}
