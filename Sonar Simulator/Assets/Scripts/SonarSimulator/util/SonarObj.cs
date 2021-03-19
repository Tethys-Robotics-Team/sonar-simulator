using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a placeholder for GameObjects
/// and its properties.
/// This class establishes a relationship with 
/// GameObjects detected by the Sonar to its properties namely
/// Absorption_coefficients, pose...
/// In future the corordinates of the objects will be implemented
/// </summary>
public class SonarObj
{
    // variable to allocate an absorption_coefficient to a GameObject
    private double abs_coeff;

    // An object of type GameObject
    private GameObject gameObject;

    // variable to allocate target_strength to a GameObject
    private double target_strength;

    // {getter,setter} properties for GameObject
    public GameObject GO
    {
        get { return gameObject;}
        set { gameObject = value;}
    }

    // {getter,setter} properties for Absorption_Coefficient
    public double Abs_Coeff
    {
        get { return abs_coeff; }
        set { abs_coeff = value; }
    }

    // {getter,setter} properties for Target_Strength
    public double Target_Strength
    {
        get { return target_strength; } 
        set { target_strength = value; }
    }

}
