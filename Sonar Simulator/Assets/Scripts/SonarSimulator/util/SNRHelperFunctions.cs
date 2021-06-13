using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SNRHelperFunctions is a class that will provide the functions to calculate the 
/// signaltonoise ratio of the Sonar
/// </summary>
public class SNRHelperFunctions : MonoBehaviour
{
    /// <summary>
    /// This function calculates the attenuation coefficient for transmission loss using the following parameters
    /// Assuming the salinity is 1 ppt for freshwater
    /// Assuming pH  is 8 for freshwater
    /// </summary>
    /// <param name="targetDistance">"The target Distance"</param>
    /// <param name="frequency">"The operating frequency in kHz of the Sonar"</param>
    /// <param name="depth">"The measured depth of the Sonar to the waterbed" , We are currently assuming it to be less than 100 meters</param>
    /// <param name="freshWater">"Freshwater or seaWater</param>
    /// <returns>TransmissionLoss</returns>
    public static float Range2TransmissionLoss(float targetDistance, float frequency, float depth, bool freshWater)
    {
        float temperature = 4.0f; // degree celsius, temperature is dependent on the location
        float salinity = 0.0f; // 1 ppt for freshwater and 35 ppt for seaWater
        if(freshWater)
        {
            salinity = 1.0f;
        }
        else
        {
            salinity = 35.0f;
        }
        float pH = 8.0f; // Assuming an average pH for lakes and sewater, this can be later modified 

        // Relaxation frequency
        float boricAcidRelaxFreq = 0.78f * Mathf.Sqrt(salinity / 35) * Mathf.Exp(temperature / 26);
        float f1 = boricAcidRelaxFreq;
        float f1Sqr = Mathf.Pow(f1, 2);

        float magnesiumSulphateRelaxFreq = 42 * Mathf.Exp(temperature / 17);
        float f2 = magnesiumSulphateRelaxFreq;
        float f2Sqr = Mathf.Pow(f2,2); // Square of the f2

        float freqSqr = Mathf.Pow(frequency,2); // Frequency Square
        
        depth = depth / (2 * 1000); // in km
        float a,b,c = 0.0f; // relaxation value of boricacid , magnesiumsulphate and viscocity of water
        a = 0.106f *( (f1 * freqSqr) / (f1Sqr + freqSqr) )* Mathf.Exp((pH - 8) / 0.56f);
        b = 0.52f * (1 + (temperature / 43)) * (salinity / 35) * (f2 * freqSqr / (f2Sqr + freqSqr)) * Mathf.Exp(-depth / 6);
        c = 0.00049f * freqSqr * Mathf.Exp(-((temperature / 27) + (depth / 17))) ;

        float transmissionLoss = 0.0f;
        float transitionDepth = 0.0f; // Transition depth after which spherical spreading becomes cylindrical
        transitionDepth = depth / 2;


        if (targetDistance <= transitionDepth) // for spherical spreading loss
        {
            transmissionLoss = Power2Decibel(Mathf.Pow(targetDistance, 2)) + ((a + b + c) * targetDistance * 0.001f);
        }  
        else if(targetDistance > transitionDepth)
        {
            transmissionLoss = Power2Decibel(targetDistance) + Power2Decibel(transitionDepth) + ((a + b + c) * targetDistance * 0.001f);
        }

        return transmissionLoss;
    }

    /// <summary>
    /// Voltage to dB conversion, voltage deals with amplitude 
    /// use this conversion for Sonar
    /// </summary>
    /// <param name="volt"></param>
    /// <returns></returns>
    public static float Voltage2Decibel(float volt)
    {
        float decibel;
        return decibel = (20 * Mathf.Log10(volt) + 300) - 300;
        
    }

    /// <summary>
    /// dB to Voltage conversion, voltage deals with amplitude 
    /// use this conversion for Sonar
    /// </summary>
    /// <param name="decibel">Input in decibels</param>
    /// <returns></returns>
    public static float Decibel2Voltage(float decibel)
    {
        float volt;
        return volt =  Mathf.Pow(20,(decibel / 10));
    }

    /// <summary>
    /// Power to dB conversion
    /// </summary>
    /// <param name="power"></param>
    /// <returns></returns>
    public static float Power2Decibel(float power)
    {
        float decibel;
        return decibel = (10 * Mathf.Log10(power) + 300) - 300;

    }

    /// <summary>
    /// dB to Power conversion
    /// </summary>
    /// <param name="decibel">Input in decibels</param>
    /// <returns></returns>
    public static float Decibel2Power(float decibel)
    {
        float power;
        return power = Mathf.Pow(10, (decibel / 10));
    }

    /// <summary>
    /// Signal to noise ratio of Sonar Equation
    /// The returned SNR is in decibel
    /// </summary>
    /// <param name="sourceLevel"> sourceLevel is in dB/1uPa </param>
    /// <param name="noiseLevel"> noiseLevel in dB/1uPa </param>
    /// <param name="directivityIndex"> in dB</param>
    /// <param name="transmissionLoss"> in dB</param>
    /// <param name="targetStrength"> in dB</param>
    /// <returns></returns>
    public static float SonarEqnSNR(float sourceLevel, float noiseLevel, float directivityIndex, float transmissionLoss, float targetStrength)
    {
        // this equation is for active Sonar
        float SNR;
        return SNR = sourceLevel + targetStrength - (2 * transmissionLoss) - (noiseLevel - directivityIndex);
    }

    public static float WavelengthCalculator(float temperature)
    {
        float waveLength = 0.0f; // in meters
        float frequency = 2100000.0f; // in hertz
        float velocity = 0.0f; // in m/sec
        switch (temperature)
        {
            case 0.0f: // temperature 0 deg celcius
                {
                    velocity = 1403.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 5.0f: // temperature is 5 deg celcius
                {
                    velocity = 1427.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 10.0f: // temperature is 10 deg celcius
                {
                    velocity = 1447.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 20.0f: // temperature is 20 deg celcius
                {
                    velocity = 1481.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 30.0f: // temperature is 30 deg celcius
                {
                    velocity = 1507.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 40.0f: // temperature is 40 deg celcius
                {
                    velocity = 1526.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            case 50.0f: // temperature is 50 deg celcius
                {
                    velocity = 1541.0f;
                    waveLength = velocity / frequency;
                    break;
                }
            default:
                return float.NaN;
        }
        return waveLength;
    }
}
