using UnityEngine;

public class Coordinates
{
    
	public static Vector3 Cartesian2Spherical(Vector3 cartesianCoordinates)
	{
		Vector3 sphericalCoordinates = new Vector3
		{
			x = Vector3.Magnitude(cartesianCoordinates),
			y = Mathf.Atan(cartesianCoordinates.x / cartesianCoordinates.z),
			z = Mathf.Asin(cartesianCoordinates.y / Vector3.Magnitude(cartesianCoordinates))

		};
		return sphericalCoordinates;
	}

	public static Vector3 Spherical2Cartesian(Vector3 sphericalCoordinates)
	{
		Vector3 cartesianCoordinates = new Vector3
		{
			x = sphericalCoordinates.x * Mathf.Sin(sphericalCoordinates.y) * Mathf.Cos(sphericalCoordinates.z),
			y = sphericalCoordinates.x * Mathf.Sin(sphericalCoordinates.z),
			z = sphericalCoordinates.x * Mathf.Cos(sphericalCoordinates.y) * Mathf.Cos(sphericalCoordinates.z)
		};
		return cartesianCoordinates;
	}

	public static Vector3 SphericalRad2Deg(Vector3 sphericalCoordinates)
	{
		Vector3 newCoords = new Vector3
		{
			x = sphericalCoordinates.x,
			y = Mathf.Rad2Deg * sphericalCoordinates.y,
			z = Mathf.Rad2Deg * sphericalCoordinates.z
		};
		return newCoords;
	}

	public static Vector3 SphericalDeg2Rad(Vector3 sphericalCoordinates)
	{
		Vector3 newCoords = new Vector3
		{
			x = sphericalCoordinates.x,
			y = Mathf.Deg2Rad * sphericalCoordinates.y,
			z = Mathf.Deg2Rad * sphericalCoordinates.z
		};
		return newCoords;
	}
}
