using UnityEngine;
using System.Collections;

public class Accelerometer : MonoBehaviour
{
	public static Quaternion calibration = Quaternion.identity;

	public static void Calibrate()
	{
#if UNITY_EDITOR || UNITY_STANDALONE
		Vector3 accel = Vector3.zero;
		
		if (Input.GetKey(KeyCode.A)) accel.x -= 1.0f;
	    if (Input.GetKey(KeyCode.D)) accel.x += 1.0f;
		if (Input.GetKey(KeyCode.W)) accel.z -= 1.0f;
		if (Input.GetKey(KeyCode.S)) accel.z += 1.0f;

		accel.Normalize();

#elif UNITY_IOS || UNITY_ANDROID
		Vector3 accel = Input.acceleration;
		accel.x = 0;
		accel.Normalize();
#endif
        if(accel.magnitude > 0.0001f)
		    calibration = Quaternion.FromToRotation(accel, Vector3.down);
        else
            calibration = Quaternion.identity;
	}

	public static Vector3 value
	{
		get
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			Vector3 dir = Vector3.zero;
			
			if(Input.GetKey(KeyCode.A)) dir.x -= 1.0f;
	        if(Input.GetKey(KeyCode.D)) dir.x += 1.0f;
			if(Input.GetKey(KeyCode.W)) dir.z -= 1.0f;
			if(Input.GetKey(KeyCode.S)) dir.z += 1.0f;
			
			if(dir == Vector3.zero)
                dir = Vector3.down;
			
			dir.Normalize();
			
			return calibration * dir;
#elif UNITY_IOS || UNITY_ANDROID
			Vector3 accel = Input.acceleration;
			return calibration * accel;
#endif
		}
	}
}
