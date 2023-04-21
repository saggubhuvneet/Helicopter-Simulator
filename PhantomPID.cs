using UnityEngine;
using Oyedoyin;

[System.Serializable]
public class PhantomPID
{
	public float Kp = 1;
	public float Ki = 0;
	public float Kd = 0.1f;

	[Header("Computation Values")]
	public float deltaError;
	public float proportional;
	public float derivative;
	public float integral;

	[Header("Limits")]
	public float minimum = -1;
	public float maximum = 1;

	public float output;
	



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float CalculateOutput(float error, float dt)
	{
		//1. PROPORTIONAL
		proportional = error * Kp;
		if (float.IsNaN(proportional) || float.IsInfinity(proportional)) { proportional = 0f; }

		//2. INTEGRAL
		integral += error * dt * Ki;
		if (integral > maximum) { integral = maximum; }
		if (integral < minimum) { integral = minimum; }
		if (float.IsNaN(integral) || float.IsInfinity(integral)) { integral = 0f; }

		//3. DERIVATIVE
		derivative = Kd * ((error - deltaError) / dt);
		if (float.IsNaN(derivative) || float.IsInfinity(derivative)) { derivative = 0f; }
		deltaError = error;

		//OUTPUT
		output = proportional + integral + derivative;
		if (output > maximum) { output = maximum; }
		if (output < minimum) { output = minimum; }
		if (float.IsNaN(output) || float.IsInfinity(output)) { output = 0f; }

		return output;
	}


	public void Reset()
	{
		proportional = 0f;
		integral = 0f;
		derivative = 0f;
	}
}

