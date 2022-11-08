using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class AutoPlayer : MonoBehaviour
{
    public string beeNumbers;
    public string thetaValues;
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("BeeGround", LoadSceneMode.Additive);
    }

    
}
