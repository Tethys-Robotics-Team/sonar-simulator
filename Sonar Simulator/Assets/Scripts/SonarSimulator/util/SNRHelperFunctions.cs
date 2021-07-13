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

    /// <summary>
    /// This function returns the PeriodicTime (time taken to complete one cycle of the transmission of signal from transducer)
    /// the function will be further used to calculate the ActualPhysicalLengthofPulseinWater
    /// http://www.fao.org/3/x5818e/x5818e04.htm#3.1.1%20time%20base
    /// </summary>
    /// <param name="frequency"></param>
    /// <returns></returns>
    public static float PeriodicTime(float frequency)
    {
        float periodicTime = 0.0f;
        periodicTime = 1 / frequency;
        return periodicTime; 
    }

    /// <summary>
    /// This function calculates the actual physical length of pulse in water
    /// this could be a target object or the aqua bed
    /// The transducer updates at 40 Hz every second, I am assuming this is the number of cycles transmitted every second
    /// </summary>
    /// <param name="cycles">40 as per documentation</param>
    /// <param name="frequency">2.1 Mhz </param>
    /// <param name="velocity">1500 m/s </param>
    /// <returns></returns>
    public static float ActualPhysicalLengthofPulseinWater(int cycles, float frequency, float velocity)
    {
        
        float pulseLengthinWater = 0.0f;
        // We know the PeriodicTime 
        // multiply the PeriodicTime times the number of cycles transmitted

        float t = 0.0f; // measure for pulse duration
        // if n cycles are transmitted, the pulse duration is 
        // 40 * .47 10^-6
        t = cycles * PeriodicTime(frequency);

        // assuming acoustic waves travel at 1500 m/s in water, velocity will be 1500 m/s
        // frequency is 2100000 Hz, and 40 cycles are transmitted every second
        // we should have a pulse Length 0.0285714285714286 m
        return pulseLengthinWater = t * velocity;
    }

    /// <summary>
    /// Directivity index can be calculated based on the design of the transducer
    /// Popular design of the face of transducer are 
    /// 1. Omnidirectional, 2. Hemispherical, 3. Circular Face, 4. Rectangular Face
    /// </summary>
    /// <returns></returns>
    public static float DirectivityIndex()
    {

        return 0.0f;
    }

    /// <summary>
    /// This method defines the AcousticPowerOutput for a given electric input
    /// We will need to know the transducer efficiency rpresented by (GreekSymbol eta)
    /// </summary>
    /// <param name="ETA">Transducer Efficiency obtained from manufacturer</param>
    /// <param name="PE">Input electric power in watts to the transducer </param>
    /// <returns></returns>
    public static float AcousticPowerOutput(float ETA, float PE)
    {
        // Acoustic Power Output (WA) for a given 
        // electric power input
        float eta = ETA; // Transducer efficiency obtained from the manufacturer
        float Pe = PE; // Electric input is measured as Watts
        float WA = eta * PE;
        return WA;
    }

    /// <summary>
    /// Source Level is defined as 10 log (intensity of source/reference intensity)
    /// SL = 170.8 + 10 log WA + DI dB/1 m Pa/1 m
    /// </summary>
    /// <returns></returns>
    public static float SourceLevel(float ETA, float PE)
    {
        float SL = 0.0f;
        float WA = AcousticPowerOutput(ETA, PE); // Acoustic Power Output Watt 
        
        // 170.8 is a constant for converting acoustic power to source level ( Urick (1975) p67)
        SL = 170.8f + Mathf.Log10(WA) + DirectivityIndex();
        return 0.0f;
    }

    /// <summary>
    /// 2 way transmission loss for an echosounder is calculated based on target Distance and absorption coefficient
    /// </summary>
    /// <param name="targetDistance"></param>
    /// <param name="absorption"></param> // we need to figure out the absorption in dB/km
    /// <returns></returns>
    public static float TwoWayTransmissionLoss(float targetDistance, float absorption)
    {
        float TL;
        float TL1; // first component of Transmission Loss is 20Log(targetDistance)
        float TL2; // second component of Transmission Loss is (alpha * targetDistance)

        // first component of TransmissionLoss 
        TL1 = 20 * Mathf.Log10(targetDistance);
        TL2 = (absorption * targetDistance)/1000; // converting from dB/km to dB/m

        TL = (2 * TL1) + (2 * TL2); // Add the components to calculate 2way transmission Loss
        return TL;

    }
}
