using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

[AddComponentMenu("Sonar Simulator/Sonar Renderer")]
[SelectionBase]
public class SonarRenderer : MonoBehaviour
{

	#region Inspector Interface

	[Header("General")]
	[SerializeField]
	string destinationFolder = "C:/SonarSimulator/Dataset1";

	[SerializeField]
	bool savePoses = true;

	[SerializeField]
	bool drawGizmo = true;

	[Range(0, 2)]
	[SerializeField]
	int verbosity = 0;


	[Header("Sonar Configuration")]
	[Range(1f,360f)]
	[SerializeField]
	float horizontalFOV = 60f;

	[Range(0f,90f)]
	[SerializeField]
	float verticalFOV = 12f;

	[SerializeField]
	float minRange = 0.1f;

	[SerializeField]
	float maxRange = 5f;

	[SerializeField]
	int beams = 512;

	[SerializeField]
	int rangeBins = 440;


	[Header("Renderer Configuration")]
	[Range(0.1f, 10f)]
	[SerializeField]
	float verticalSampleMultiplier = 1.0f;

	[Space(10)]
	[SerializeField]
	bool enableNoise = true;

	[Range(0f, 1f)]
	[SerializeField]
	float noiseLevel = 0.2f;

	[Space(10)]

	[SerializeField]
	bool enableAutoSampling = false;

	[Range(0.01f, 10f)]
	[SerializeField]
	float sampleInterval = 0.5f;

	[Space(10)]
	[SerializeField]
	LayerMask cullingMask = ~0;


	[Header("Pose Axis Mapping")]
	[SerializeField]
	AxisOption positionXAxisOut = AxisOption.Z_Axis_Positive;

	[SerializeField]
	AxisOption positionYAxisOut = AxisOption.X_Axis_Negative;

	[SerializeField]
	AxisOption positionZAxisOut = AxisOption.Y_Axis_Positive;

	[Space(10)]
	[SerializeField]
	AxisOption rotationXAxisOut = AxisOption.Z_Axis_Negative;

	[SerializeField]
	AxisOption rotationYAxisOut = AxisOption.X_Axis_Positive;

	[SerializeField]
	AxisOption rotationZAxisOut = AxisOption.Y_Axis_Negative;

	[Header("Screen Capture")]

	[SerializeField]
	bool saveScreenShots = false;

	[SerializeField]
	bool mirrorSonarSettingsWithCamera = true;

	#endregion

	#region Protected Fields

	protected int nextImageIndex = 0;
	protected float lastImageTime = 0f;

	protected Camera sonarCamera;

	#endregion

	#region Unity Callbacks

	void Start()
	{
		nextImageIndex = 0;
		lastImageTime = 0f;
		Directory.CreateDirectory(destinationFolder);

		sonarCamera = GetComponentInChildren<Camera>();
		if (mirrorSonarSettingsWithCamera && sonarCamera != null)
		{
			sonarCamera.fieldOfView = Camera.HorizontalToVerticalFieldOfView(horizontalFOV, sonarCamera.aspect);
			sonarCamera.nearClipPlane = minRange;
			sonarCamera.farClipPlane = maxRange;
		}
	}

	void Update()
	{
		if (enableAutoSampling)
		{
			float currTime = Time.time;
			if (currTime - lastImageTime >= sampleInterval)
			{
				TakeImage(true);
				lastImageTime = currTime;
				nextImageIndex++;
			}
		}
	}

	void OnDrawGizmos()
	{
		if (drawGizmo)
		{
			float halfHorzFOV = horizontalFOV / 2f;
			float halfVertFOV = verticalFOV / 2f;
			float effectiveRange = maxRange - minRange;

			Vector3 originLowerLeft = GetRayOrigin(0, 0, horizontalFOV, verticalFOV, halfHorzFOV, halfVertFOV);
			Vector3 directionLowerLeft = Vector3.Normalize(originLowerLeft - transform.position) * effectiveRange;

			Vector3 originUpperLeft = GetRayOrigin(0, 1, horizontalFOV, verticalFOV, halfHorzFOV, halfVertFOV);
			Vector3 directionUpperLeft = Vector3.Normalize(originUpperLeft - transform.position) * effectiveRange;

			Vector3 originUpperRight = GetRayOrigin(1, 1, horizontalFOV, verticalFOV, halfHorzFOV, halfVertFOV);
			Vector3 directionUpperRight = Vector3.Normalize(originUpperRight - transform.position) * effectiveRange;

			Vector3 originLowerRight = GetRayOrigin(1, 0, horizontalFOV, verticalFOV, halfHorzFOV, halfVertFOV);
			Vector3 directionLowerRight = Vector3.Normalize(originLowerRight - transform.position) * effectiveRange;

			Gizmos.DrawRay(originLowerLeft, directionLowerLeft);
			Gizmos.DrawRay(originUpperLeft, directionUpperLeft);
			Gizmos.DrawRay(originUpperRight, directionUpperRight);
			Gizmos.DrawRay(originLowerRight, directionLowerRight);
		}
	}

	#endregion

	#region Public Methods

	public void TakeImage()
	{
		Directory.CreateDirectory(destinationFolder);
		TakeImage(false);
	}

	public void TakeImage(bool autoSample)
	{
		LogVerbose("Taking image...", 1);

		// Create black base sonar image
		Texture2D sonarImage = new Texture2D(beams, rangeBins);
		for (int x = 0; x < beams; x++)
		{
			for (int y = 0; y < rangeBins; y++)
			{
				sonarImage.SetPixel(x, y, Color.black);
			}
		}

		int samples = Mathf.Max((int)(verticalSampleMultiplier * rangeBins), 1);
		float degsPerBeam = horizontalFOV / beams;
		float degsPerSample = verticalFOV / samples;

		float horzFOVHalf = horizontalFOV / 2f;
		float vertFOVHalf = verticalFOV / 2f;

		float effectiveRange = maxRange - minRange;
		float rangePerBin = effectiveRange / rangeBins;

		float intensity; // similar to the intensity from the sensor
		float absorption_coefficient = 0.0f; // The absorption coefficient of material
		float dist = 0.0f; // Distance from sonar to the hit point

		for (int b = 0; b < beams; b++)
		{
			for (int sample = 0; sample < samples; sample++)
			{

				Vector3 origin = GetRayOrigin(b, sample, degsPerBeam, degsPerSample, horzFOVHalf, vertFOVHalf);
				Vector3 direction = origin - transform.position;


				if (Physics.Raycast(origin, direction, out RaycastHit hit, effectiveRange, cullingMask))
				{
					// Sample has echo -> get range bin and store echo
					int bin = (int)(hit.distance / rangePerBin);

					// Calculate the distance of the object from Sonar
					//
					//dist = Vector3.Magnitude(transform.position - hit.point);
					dist = hit.distance;
					dist = (dist / effectiveRange); // Need to add the comment

					Vector3 rayDirection = Vector3.Normalize(transform.position - hit.point);
					intensity = Vector3.Dot(rayDirection, hit.normal);

					// Get the Absorption_Coefficient of the material from the raycast

					if (hit.transform.GetComponent<Renderer>() && hit.transform.GetComponent<Renderer>().sharedMaterial) // only if the object has a material and custom shader
					{
						// Absorption_Coefficient from the material custom shader
						absorption_coefficient = hit.transform.GetComponent<Renderer>().sharedMaterial.GetFloat("_Absorption_Coefficient");
					}
					else
					{
						// Need to write the else condition
					}

					//intensity = intensity - (absorption_coefficient * dist) - (0.459 * Random.Range(-noiseLevel,noiseLevel));
					//intensity = Mathf.Clamp(intensity - (absorption_coefficient * dist) +  Random.Range(-.2f,.2f),0f,1f);
					intensity = intensity - (absorption_coefficient * dist) + Random.Range(-.2f, .2f);

					// This will introduce noise to the image. 
					if (enableNoise)
					{   /// check with walter the correct formula
						intensity = Mathf.Clamp(intensity + Random.Range(-noiseLevel, noiseLevel), 0f, 1f);
					}

					// Correct orientation (flip both axes)
					int yCoord = rangeBins - bin - 1;
					int xCoord = beams - b - 1;

					sonarImage.SetPixel(xCoord, yCoord, new Color(intensity, intensity, intensity));
				}
				else
				{
					// No echoes for this sample -> ignore
				}
			}
		}

		string fileName;
		if (autoSample)
		{
			fileName = "frame_" + nextImageIndex.ToString("D6");
		}
		else
		{
			fileName = System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss");
		}


		File.WriteAllBytes(destinationFolder + "/" + fileName + ".jpg", sonarImage.EncodeToJPG());

		if (savePoses)
		{
			string poseString = GetPoseString(',');
			LogVerbose(poseString, 2);

			using (StreamWriter writer = new StreamWriter(destinationFolder + "/" + fileName + ".csv"))
			{
				writer.WriteLine(poseString);
			}
		}

		if (saveScreenShots)
		{
			ScreenCapture.CaptureScreenshot(destinationFolder + "/" + fileName + ".png");
		}

		LogVerbose("Image taken!", 1);
	}

	#endregion

	#region Protected Methods

	protected void LogVerbose(string message, int minVerbosity)
	{
		if (verbosity >= minVerbosity)
		{
			Debug.Log(message);
		}
	}

	protected float GetPositionValue(AxisOption option)
	{
		switch (option)
		{
			case AxisOption.X_Axis_Positive:
				return transform.position.x;
			case AxisOption.X_Axis_Negative:
				return -transform.position.x;
			case AxisOption.Y_Axis_Positive:
				return transform.position.y;
			case AxisOption.Y_Axis_Negative:
				return -transform.position.y;
			case AxisOption.Z_Axis_Positive:
				return transform.position.z;
			case AxisOption.Z_Axis_Negative:
				return -transform.position.z;
			default:
				return float.NaN;
		}
	}

	protected float GetRotationValue(AxisOption option)
	{
		switch (option)
		{
			case AxisOption.X_Axis_Positive:
				return transform.rotation.eulerAngles.x;
			case AxisOption.X_Axis_Negative:
				return -transform.rotation.eulerAngles.x;
			case AxisOption.Y_Axis_Positive:
				return transform.rotation.eulerAngles.y;
			case AxisOption.Y_Axis_Negative:
				return -transform.rotation.eulerAngles.y;
			case AxisOption.Z_Axis_Positive:
				return transform.rotation.eulerAngles.z;
			case AxisOption.Z_Axis_Negative:
				return -transform.rotation.eulerAngles.z;
			default:
				return float.NaN;
		}
	}

	protected string GetPoseString(char seperator)
	{
		StringBuilder sb = new StringBuilder(128);
		sb.Append(GetPositionValue(positionXAxisOut));
		sb.Append(seperator);
		sb.Append(GetPositionValue(positionYAxisOut));
		sb.Append(seperator);
		sb.Append(GetPositionValue(positionZAxisOut));
		sb.Append(seperator);

		sb.Append(GetRotationValue(rotationXAxisOut));
		sb.Append(seperator);
		sb.Append(GetRotationValue(rotationYAxisOut));
		sb.Append(seperator);
		sb.Append(GetRotationValue(rotationZAxisOut));

		return sb.ToString();
	}

	protected Vector3 GetRayOrigin(int beam, int sample, float degsPerBeam, float degsPerSample, float halfHorzFOV, float halfVertFOV)
	{
		Vector3 originSpherical = new Vector3
		{
			x = minRange,
			y = beam * degsPerBeam - halfHorzFOV,
			z = sample * degsPerSample - halfVertFOV
		};
		return transform.TransformPoint(Coordinates.Spherical2Cartesian(Coordinates.SphericalDeg2Rad(originSpherical)));
	}

	#endregion

	#region Protected Enums

	protected enum AxisOption
	{
		X_Axis_Positive,
		X_Axis_Negative,
		Y_Axis_Positive,
		Y_Axis_Negative,
		Z_Axis_Positive,
		Z_Axis_Negative
	};

	#endregion
}
