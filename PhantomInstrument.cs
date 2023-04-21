using UnityEngine.UI;
using UnityEngine;
using Oyedoyin;






public class PhantomInstrument : MonoBehaviour
{


	// --------------------------------------------------------- Selectibles
	public enum DisplayType { Speedometer, Altimeter, Variometer, Compass, Horizon, FuelQuantity, EnginePower, Temperature, Tachometer }
	public DisplayType displayType = DisplayType.Speedometer;
	public enum SpeedType { Knots, MPH, KPH }
	public SpeedType speedType = SpeedType.Knots;
	public enum FuelType { kiloGrams, Pounds, Gallons }
	public FuelType fuelType = FuelType.kiloGrams;
	public enum EngineType { TurboShaft, ElectricMotor, PistonEngine }
	public EngineType engineType = EngineType.PistonEngine;



	// --------------------------------------------------------- Connections
	public PhantomController controller;
	public PhantomTurboShaft shaft;
	public PhantomPistonEngine piston;
	public RectTransform needle;
	public RectTransform pitchTape;
	public RectTransform rollAnchor;
	public RectTransform TemperatureNeedle;
	public Text valueOutput;




	// --------------------------------------------------------- Variables
	public float currentValue;
	public float maximumValue;
	public float maximumRotation = 360f;
	public float minimumRotation = 0f;
	float needleRotation;
	float smoothRotation;

	// --------------------------------------------------------- Variables
	public float inputFactor = 1f;
	public float rotationFactor = 1f;
	public float rollValue;
	public float pitchValue;
	public float minimumPosition;
	public float movementFactor;
	public float maximumPitch;
	public float minimumPitch;

	public float minimumRoll;
	public float maximumRoll;
	public float minimumValue;
	private float TemperatureNeedlePosition = 0.0f;
	private float smoothedTemperatureNeedlePosition = 0.0f;
	public float minimumTemperaturePosition = 20.0f;
	public float maximumTemperaturePosition = 160.0f;



	void FixedUpdate()
	{

		if (controller != null && controller.gameObject.activeSelf)
		{
			// --------------------------------- Speed
			if (displayType == DisplayType.Speedometer)
			{
				float baseSpeed = controller.core.forwardSpeed;

				if (speedType == SpeedType.Knots) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KTS"); }
				if (speedType == SpeedType.MPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "MPH"); }
				if (speedType == SpeedType.KPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KPH"); }
			}


			// -------------------------------- Altitude
			if (displayType == DisplayType.Altimeter)
			{
				float baseAltitude = MathBase.ConvertDistance(controller.core.currentAltitude, "FT");
				currentValue = baseAltitude / 1000f;
			}

			// -------------------------------- Compass
			if (displayType == DisplayType.Compass) { currentValue = controller.core.headingDirection; }


			// -------------------------------- RPM
			if (displayType == DisplayType.Tachometer)
			{
				if (engineType == EngineType.TurboShaft && shaft != null) { currentValue = shaft.core.coreFactor * 100; }
				if (engineType == EngineType.PistonEngine && piston != null) { currentValue = piston.core.coreFactor * 100; }
			}


			// -------------------------------- Variometer
			if (displayType == DisplayType.Variometer)
			{
				currentValue = controller.core.verticalSpeed * MathBase.toFtMin;
				float baseDelta = (currentValue - minimumValue) / (maximumValue - minimumValue);
				float baseDeltaRotation = minimumRotation + ((maximumRotation - minimumRotation) * baseDelta);

				// ------------------------ Set
				needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -baseDeltaRotation);
			}


			// -------------------------------- Fuel
			if (displayType == DisplayType.FuelQuantity)
			{
				currentValue = controller.fuelLevel;
			}


			// -------------------------------- Power
			if (displayType == DisplayType.EnginePower)
			{
				currentValue = controller.powerFactor * 100f;
			}


			// -------------------------------- Temperature
			if (displayType == DisplayType.Temperature)
			{
				currentValue = controller.powerFactor * maximumValue;
				TemperatureNeedlePosition = Mathf.Lerp(minimumTemperaturePosition, maximumTemperaturePosition, currentValue / maximumValue);
				smoothedTemperatureNeedlePosition = Mathf.Lerp(smoothedTemperatureNeedlePosition, TemperatureNeedlePosition, Time.deltaTime * 5);
				TemperatureNeedle.transform.localPosition = new Vector3(TemperatureNeedle.transform.eulerAngles.x, smoothedTemperatureNeedlePosition, TemperatureNeedle.transform.eulerAngles.z);
				valueOutput.text = currentValue.ToString("0.0") + " °C";
			}


			// -------------------------------- Horizon
			if (displayType == DisplayType.Horizon && controller.helicopter != null)
			{
				if (pitchTape != null)
				{
					pitchValue = Mathf.DeltaAngle(0, -controller.helicopter.transform.rotation.eulerAngles.x);
					float extension = minimumPosition + movementFactor * Mathf.Clamp(pitchValue, minimumPitch, maximumPitch) / 10f;
					pitchTape.anchoredPosition3D = new Vector3(pitchTape.anchoredPosition3D.x, extension, pitchTape.anchoredPosition3D.z);
				}

				if (rollAnchor != null)
				{
					rollValue = Mathf.DeltaAngle(0, -controller.helicopter.transform.eulerAngles.z);
					float rotation = Mathf.Clamp(rollValue, minimumRoll, maximumRoll);
					rollAnchor.localEulerAngles = new Vector3(rollAnchor.localEulerAngles.x, rollAnchor.localEulerAngles.y, rotation);
				}
			}


			// -------------------------------- Rotate Needle
			if (displayType != DisplayType.Temperature)
			{
				//CONVERT
				float dataValue = currentValue * inputFactor;
				//ROTATE
				if (needle != null)
				{
					needleRotation = Mathf.Lerp(minimumRotation, (maximumRotation * rotationFactor), dataValue / (maximumValue * rotationFactor));
					smoothRotation = Mathf.Lerp(smoothRotation, needleRotation, Time.deltaTime * 5);
					needle.transform.eulerAngles = new Vector3(needle.transform.eulerAngles.x, needle.transform.eulerAngles.y, -smoothRotation);
				}
			}


			// -------------------------------- Display Text
			if (valueOutput != null)
			{
				float dataValue = currentValue * inputFactor;
				valueOutput.text = dataValue.ToString("0.0");
			}
		}
	}
}
