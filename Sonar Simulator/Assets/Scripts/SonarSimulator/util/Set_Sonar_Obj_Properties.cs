using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will set the properties that are 
/// observed by the Sonar. The associated properties 
/// will be used to calculate the intensity for displaying
/// in the game
/// </summary>
public class Set_Sonar_Obj_Properties : MonoBehaviour
{
    // Declare SonarRenderer here;

    SonarRenderer sr;

    // Declare SonarObj 's here
    public SonarObj T; // Short for Terrain;
    //public GameObject S;
    //public GameObject C;
    //public GameObject Cil;
    public SonarObj G; // Short for Grate

    // Start is called before the first frame update
    void Start()
    {
        // Object Instantiation and 
        // Initialization in one step
        // 
        G = new SonarObj
        {
            GO = GameObject.Find("Grate"),
            Abs_Coeff = 0.1
        };
        //Debug.Log(G.GO);
        //Debug.Log(G.Abs_Coeff);

        // Object Instantiation and 
        // Initialization in one step
        T = new SonarObj
        {
            GO = GameObject.Find("Terrain"),
            Abs_Coeff = 0.6
        };
        //Debug.Log(T.GO);
        //Debug.Log(T.Abs_Coeff);
        
    }

    // Update is called once per frame
    void Update()
    {
        // Press the 'I' or 'i' key to take image
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I key was pressed");
            sr.TakeImage();
        }
        
    }
}
