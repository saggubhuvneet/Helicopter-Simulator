using UnityEngine;
using UnityEngine.UI;
using Oyedoyin.Rotary.LowFidelity;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class PhantomDisplay : MonoBehaviour {

	public enum ModelType { Realistic, Arcade }
	public ModelType m_type = ModelType.Realistic;
	public PhantomController helicopterController;
	public Controller m_controller;

	[HideInInspector]public SilantroSapphire weatherSystem;
	[HideInInspector]public bool displayPoints;
	//DATA
	[HideInInspector]public Text speed;
	[HideInInspector]public Text altitude;
	[HideInInspector]public Text fuel;
	[HideInInspector]public Text weight;
	[HideInInspector]public Text brake;
	[HideInInspector]public Text density;
	[HideInInspector]public Text temperature;
	[HideInInspector]public Text pressure;
	[HideInInspector]public Text enginePower;
	[HideInInspector] public Text propellerPower;
	[HideInInspector]public Text gearState;
	[HideInInspector]public Text Time;
	[HideInInspector]public Text collective;
	[HideInInspector]public Text engineName;
	[HideInInspector]public Text currentWeapon;
	[HideInInspector]public Text weaponCount;
	[HideInInspector]public Text ammoCount;

	public Text gload;
	public Text pitchRate;
	public Text rollRate;
	public Text yawRate;
	public Text turnRate;
	public Text climb;

	public enum UnitsSetup
	{
		Metric,
		Imperial,
		Custom
	}
	[HideInInspector]public UnitsSetup units = UnitsSetup.Metric;
	public enum SpeedUnit
	{
		MeterPerSecond,
		Knots,
		FeetPerSecond,
		MilesPerHour,
		KilometerPerHour,
	}
	[HideInInspector]public SpeedUnit speedUnit = SpeedUnit.MeterPerSecond;
	//
	public enum AltitudeUnit
	{
		Meter,
		Feet,
		NauticalMiles,
		Kilometer
	}
	[HideInInspector]public AltitudeUnit altitudeUnit = AltitudeUnit.Meter;
	//
	public enum TemperatureUnit
	{
		Celsius,
		Fahrenheit
	}
	[HideInInspector]public TemperatureUnit temperatureUnit = TemperatureUnit.Celsius;
	//
	public enum WeightUnit
	{
		Tonne,
		Pound,
		Ounce,
		Stone,
		Kilogram
	}
	[HideInInspector]public WeightUnit weightUnit = WeightUnit.Kilogram;
	//
	public enum ForceUnit
	{
		Newton,
		KilogramForce,
		PoundForce
	}
	[HideInInspector]public ForceUnit forceUnit = ForceUnit.Newton;
	//
	public enum TorqueUnit
	{
		NewtonMeter,
		PoundForceFeet
	}
	[HideInInspector]public TorqueUnit torqueUnit = TorqueUnit.NewtonMeter;
	Text[] children;







	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (m_type == ModelType.Realistic)
		{
			if (helicopterController != null && helicopterController.isActiveAndEnabled && speed != null)
			{
				if (helicopterController != null)
				{
					if (engineName != null)
					{
						engineName.text = helicopterController.transform.name;
					}
				}
				//PARKING BRAKE
				if (helicopterController.gearHelper != null && helicopterController.gearHelper.brakeState == PhantomGearSystem.BrakeState.Engaged)
				{
					brake.text = "Parking Brake = On";
				}
				else
				{
					brake.text = "Parking Brake = Off";
				}
				if (weatherSystem != null)
				{
					Time.text = weatherSystem.CurrentTime;
				}

				float gfore = helicopterController.core.gForce;
				if (gload != null)
				{
					if (gfore > 4f) { gload.color = Color.red; } else if (gfore < -1f) { gload.color = Color.yellow; } else { gload.color = Color.white; }
					gload.text = "G-Load = " + helicopterController.core.gForce.ToString("0.00");
				}
				climb.text = "Climb = " + (helicopterController.core.verticalSpeed * Oyedoyin.MathBase.toFtMin).ToString("0.0") + " ft/min";

				//WEIGHT SETTINGS
				float Weight = helicopterController.currentWeight;
				if (weightUnit == WeightUnit.Kilogram)
				{
					weight.text = "Weight = " + Weight.ToString("0.0") + " kg";
				}
				if (weightUnit == WeightUnit.Tonne)
				{
					float tonneWeight = Weight * 0.001f;
					weight.text = "Weight = " + tonneWeight.ToString("0.00") + " T";
				}
				if (weightUnit == WeightUnit.Pound)
				{
					float poundWeight = Weight * 2.20462f;
					weight.text = "Weight = " + poundWeight.ToString("0.0") + " lb";
				}
				if (weightUnit == WeightUnit.Ounce)
				{
					float ounceWeight = Weight * 35.274f;
					weight.text = "Weight = " + ounceWeight.ToString("0.0") + " Oz";
				}
				if (weightUnit == WeightUnit.Stone)
				{
					float stonneWeight = Weight * 0.15747f;
					weight.text = "Weight = " + stonneWeight.ToString("0.0") + " St";
				}
				//FUEL
				float Fuel = helicopterController.fuelLevel;
				if (weightUnit == WeightUnit.Kilogram)
				{
					fuel.text = "Fuel = " + Fuel.ToString("0.0") + " kg";
				}
				if (weightUnit == WeightUnit.Tonne)
				{
					float tonneWeight = Fuel * 0.001f;
					fuel.text = "Fuel = " + tonneWeight.ToString("0.00") + " T";
				}
				if (weightUnit == WeightUnit.Pound)
				{
					float poundWeight = Fuel * 2.20462f;
					fuel.text = "Fuel = " + poundWeight.ToString("0.0") + " lb";
				}
				if (weightUnit == WeightUnit.Ounce)
				{
					float ounceWeight = Fuel * 35.274f;
					fuel.text = "Fuel = " + ounceWeight.ToString("0.0") + " Oz";
				}
				if (weightUnit == WeightUnit.Stone)
				{
					float stonneWeight = Fuel * 0.15747f;
					fuel.text = "Fuel = " + stonneWeight.ToString("0.0") + " St";
				}
				//SPEED
				if (helicopterController.core != null)
				{
					float Speed = helicopterController.core.forwardSpeed;

					if (speedUnit == SpeedUnit.Knots)
					{
						float speedly = Speed * 1.944f;
						speed.text = "Airspeed = " + speedly.ToString("0.0") + " knots";
					}
					if (speedUnit == SpeedUnit.MeterPerSecond)
					{
						float speedly = Speed;
						speed.text = "Airspeed = " + speedly.ToString("0.0") + " m/s";
					}
					if (speedUnit == SpeedUnit.FeetPerSecond)
					{
						float speedly = Speed * 3.2808f;
						speed.text = "Airspeed = " + speedly.ToString("0.0") + " ft/s";
					}
					if (speedUnit == SpeedUnit.MilesPerHour)
					{
						float speedly = Speed * 2.237f;
						speed.text = "Airspeed = " + speedly.ToString("0.0") + " mph";
					}
					if (speedUnit == SpeedUnit.KilometerPerHour)
					{
						float speedly = Speed * 3.6f;
						speed.text = "Airspeed = " + speedly.ToString("0.0") + " kmh";
					}
				}
				//ENGINE POWER
				//if (propellerPower != null) { propellerPower.text = "Propeller Throttle = " + (helicopterController.actualThrustInput * 100f).ToString("0.0") + " %"; }
				enginePower.text = "Engine Throttle = " + (helicopterController.flightComputer.processedThrottle * 100f).ToString("0.0") + " %";
				collective.text = "Collective = " + (helicopterController.flightComputer.processedCollective * 100f).ToString("0.0") + " %";
				//ALTITUDE
				if (helicopterController.core != null)
				{
					float Altitude = helicopterController.core.currentAltitude * Oyedoyin.MathBase.toFt;
					if (altitudeUnit == AltitudeUnit.Feet)
					{
						float distance = Altitude;
						altitude.text = "Altitude = " + distance.ToString("0.0") + " ft";
					}
					if (altitudeUnit == AltitudeUnit.NauticalMiles)
					{
						float distance = Altitude * 0.00054f;
						altitude.text = "Altitude = " + distance.ToString("0.0") + " NM";
					}
					if (altitudeUnit == AltitudeUnit.Kilometer)
					{
						float distance = Altitude / 3280.8f;
						altitude.text = "Altitude = " + distance.ToString("0.0") + " km";
					}
					if (altitudeUnit == AltitudeUnit.Meter)
					{
						float distance = Altitude / 3.2808f;
						altitude.text = "Altitude = " + distance.ToString("0.0") + " m";
					}
					//

					//AMBIENT
					pressure.text = "Pressure = " + helicopterController.core.pressure_hpa.ToString("0.0") + " hpa";
					density.text = "Air Density = " + helicopterController.core.airDensity.ToString("0.000") + " kg/m3";
					//
					float Temperature = helicopterController.core.ambientTemperature;
					if (temperatureUnit == TemperatureUnit.Celsius)
					{
						temperature.text = "Temperature = " + Temperature.ToString("0.0") + " °C";
					}
					if (temperatureUnit == TemperatureUnit.Fahrenheit)
					{
						float temp = (Temperature * (9 / 5)) + 32f;
						temperature.text = "Temperature = " + temp.ToString("0.0") + " °F";
					}
				}
				if (helicopterController.gearHelper != null)
				{
					if (gearState.gameObject.activeSelf == true)
					{
						gearState.gameObject.SetActive(false);
						brake.gameObject.SetActive(false);
					}
				}

				if (pitchRate != null && pitchRate.gameObject.activeSelf)
				{
					turnRate.text = helicopterController.core.turnRate.ToString("0.0") + " °/s";
					pitchRate.text = helicopterController.core.pitchRate.ToString("0.0") + " °/s";
					rollRate.text = helicopterController.core.rollRate.ToString("0.0") + " °/s";
					yawRate.text = helicopterController.core.yawRate.ToString("0.0") + " °/s";
				}

				//WEAPON
				if (helicopterController.hardpoints != null)
				{
					//ACTIVATE
					if (!weaponCount.gameObject.activeSelf)
					{
						weaponCount.gameObject.SetActive(false);
						currentWeapon.gameObject.SetActive(false);
						ammoCount.gameObject.SetActive(false);
					}
					//SET VALUES
					weaponCount.text = "Weapon Count: " + helicopterController.hardpoints.availableWeapons.Count.ToString();
					currentWeapon.text = "Current Weapon: " + helicopterController.hardpoints.currentWeapon;
					if (helicopterController.hardpoints.currentWeapon == "Gun")
					{
						int ammoTotal = 0;
						foreach (PhantomMachineGun gun in helicopterController.hardpoints.attachedGuns)
						{
							ammoTotal += gun.currentAmmo;
						}
						ammoCount.text = "Ammo Count: " + ammoTotal.ToString();
					}
					//
					if (helicopterController.hardpoints.currentWeapon == "Missile")
					{

						int count = 0;
           				if (helicopterController.hardpoints.missiles != null) { count = helicopterController.hardpoints.missiles.Count; }
            			if (helicopterController.hardpoints.lowMissiles != null) { count = helicopterController.hardpoints.lowMissiles.Count; }
						ammoCount.text = "Ammo Count: " + count.ToString();
					}
					//
					if (helicopterController.hardpoints.currentWeapon == "Rocket")
					{
						ammoCount.text = "Ammo Count: " + helicopterController.hardpoints.rockets.Count.ToString();
					}

				}
				else
				{
					if (weaponCount != null && weaponCount.gameObject.activeSelf)
					{
						weaponCount.gameObject.SetActive(false);
						currentWeapon.gameObject.SetActive(false);
						ammoCount.gameObject.SetActive(false);
					}
				}
			}
		}
		else
        {
			if(m_controller != null && m_controller.gameObject.activeSelf)
            {
				if (engineName != null)
				{
					engineName.text = m_controller.transform.name;
				}
				if (gearState.gameObject.activeSelf == true)
				{
					gearState.gameObject.SetActive(false);
					brake.gameObject.SetActive(false);
				}

				if (weaponCount != null && weaponCount.gameObject.activeSelf)
				{
					weaponCount.gameObject.SetActive(false);
					currentWeapon.gameObject.SetActive(false);
					ammoCount.gameObject.SetActive(false);
				}


				if (pitchRate != null && pitchRate.gameObject.activeSelf)
				{
					turnRate.text = m_controller.m_turnrate.ToString("0.0") + " °/s";
					pitchRate.text = m_controller.m_pitchrate.ToString("0.0") + " °/s";
					rollRate.text = m_controller.m_rollrate.ToString("0.0") + " °/s";
					yawRate.text = m_controller.m_yawrate.ToString("0.0") + " °/s";
				}


				//AMBIENT
				pressure.text = "Pressure = " + m_controller.m_air_pressure.ToString("0.0") + " kpa";
				density.text = "Air Density = " + m_controller.m_air_density.ToString("0.000") + " kg/m3";
				//
				float Temperature = m_controller.m_air_temperature;
				if (temperatureUnit == TemperatureUnit.Celsius)
				{
					temperature.text = "Temperature = " + Temperature.ToString("0.0") + " °C";
				}
				if (temperatureUnit == TemperatureUnit.Fahrenheit)
				{
					float temp = (Temperature * (9 / 5)) + 32f;
					temperature.text = "Temperature = " + temp.ToString("0.0") + " °F";
				}

				float gfore = m_controller.m_gforce;
				if (gload != null)
				{
					if (gfore > 4f) { gload.color = Color.red; } else if (gfore < -1f) { gload.color = Color.yellow; } else { gload.color = Color.white; }
					gload.text = "G-Load = " + gfore.ToString("0.00");
				}

				climb.text = "Climb = " + (m_controller.helicopter.velocity.y * Oyedoyin.MathBase.toFtMin).ToString("0.0") + " ft/min";

				//WEIGHT SETTINGS
				float Weight = m_controller.currentWeight;
				if (weightUnit == WeightUnit.Kilogram)
				{
					weight.text = "Weight = " + Weight.ToString("0.0") + " kg";
				}
				if (weightUnit == WeightUnit.Tonne)
				{
					float tonneWeight = Weight * 0.001f;
					weight.text = "Weight = " + tonneWeight.ToString("0.00") + " T";
				}
				if (weightUnit == WeightUnit.Pound)
				{
					float poundWeight = Weight * 2.20462f;
					weight.text = "Weight = " + poundWeight.ToString("0.0") + " lb";
				}
				if (weightUnit == WeightUnit.Ounce)
				{
					float ounceWeight = Weight * 35.274f;
					weight.text = "Weight = " + ounceWeight.ToString("0.0") + " Oz";
				}
				if (weightUnit == WeightUnit.Stone)
				{
					float stonneWeight = Weight * 0.15747f;
					weight.text = "Weight = " + stonneWeight.ToString("0.0") + " St";
				}
				//FUEL
				float Fuel = m_controller.fuelLevel;
				if (weightUnit == WeightUnit.Kilogram)
				{
					fuel.text = "Fuel = " + Fuel.ToString("0.0") + " kg";
				}
				if (weightUnit == WeightUnit.Tonne)
				{
					float tonneWeight = Fuel * 0.001f;
					fuel.text = "Fuel = " + tonneWeight.ToString("0.00") + " T";
				}
				if (weightUnit == WeightUnit.Pound)
				{
					float poundWeight = Fuel * 2.20462f;
					fuel.text = "Fuel = " + poundWeight.ToString("0.0") + " lb";
				}
				if (weightUnit == WeightUnit.Ounce)
				{
					float ounceWeight = Fuel * 35.274f;
					fuel.text = "Fuel = " + ounceWeight.ToString("0.0") + " Oz";
				}
				if (weightUnit == WeightUnit.Stone)
				{
					float stonneWeight = Fuel * 0.15747f;
					fuel.text = "Fuel = " + stonneWeight.ToString("0.0") + " St";
				}
				//SPEED
				float Speed = m_controller.vz;

				if (speedUnit == SpeedUnit.Knots)
				{
					float speedly = Speed * 1.944f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " knots";
				}
				if (speedUnit == SpeedUnit.MeterPerSecond)
				{
					float speedly = Speed;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " m/s";
				}
				if (speedUnit == SpeedUnit.FeetPerSecond)
				{
					float speedly = Speed * 3.2808f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " ft/s";
				}
				if (speedUnit == SpeedUnit.MilesPerHour)
				{
					float speedly = Speed * 2.237f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " mph";
				}
				if (speedUnit == SpeedUnit.KilometerPerHour)
				{
					float speedly = Speed * 3.6f;
					speed.text = "Airspeed = " + speedly.ToString("0.0") + " kmh";
				}



				float Altitude = m_controller.transform.position.y * Oyedoyin.MathBase.toFt;
				if (altitudeUnit == AltitudeUnit.Feet)
				{
					float distance = Altitude;
					altitude.text = "Altitude = " + distance.ToString("0.0") + " ft";
				}
				if (altitudeUnit == AltitudeUnit.NauticalMiles)
				{
					float distance = Altitude * 0.00054f;
					altitude.text = "Altitude = " + distance.ToString("0.0") + " NM";
				}
				if (altitudeUnit == AltitudeUnit.Kilometer)
				{
					float distance = Altitude / 3280.8f;
					altitude.text = "Altitude = " + distance.ToString("0.0") + " km";
				}
				if (altitudeUnit == AltitudeUnit.Meter)
				{
					float distance = Altitude / 3.2808f;
					altitude.text = "Altitude = " + distance.ToString("0.0") + " m";
				}


				enginePower.text = "Engine Throttle = " + (m_controller.corePower * 100f).ToString("0.0") + " %";
				collective.text = "Collective = " + (m_controller.m_collective_power * 100f).ToString("0.0") + " %";
			}
        }
	}
}





#if UNITY_EDITOR
[CustomEditor(typeof(PhantomDisplay))]
public class PhantomDisplayEditor: Editor
{
	Color backgroundColor;
	Color silantroColor = new Color(1f,0.4f,0);
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;
		//DrawDefaultInspector ();
		PhantomDisplay control = (PhantomDisplay)target;
		serializedObject.UpdateIfRequiredOrScript();

		GUILayout.Space(10f);
		EditorGUILayout.PropertyField(serializedObject.FindProperty("m_type"), new GUIContent("System Type"));
		EditorGUILayout.HelpBox("Connected Helicoper", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		if (control.m_type == PhantomDisplay.ModelType.Arcade)
        {
			EditorGUILayout.PropertyField(serializedObject.FindProperty("m_controller"), new GUIContent(" "));
		}
		else
        {
			control.helicopterController = EditorGUILayout.ObjectField(" ", control.helicopterController, typeof(PhantomController), true) as PhantomController;
		}
		
		GUILayout.Space(15f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Unit Display Setup", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.units = (PhantomDisplay.UnitsSetup)EditorGUILayout.EnumPopup("Unit System",control.units);
		GUILayout.Space(3f);
		if (control.units == PhantomDisplay.UnitsSetup.Custom) {
			//
			EditorGUI.indentLevel++;
			GUILayout.Space(3f);
			control.speedUnit = (PhantomDisplay.SpeedUnit)EditorGUILayout.EnumPopup("Speed Unit",control.speedUnit);
			GUILayout.Space(3f);
			control.altitudeUnit = (PhantomDisplay.AltitudeUnit)EditorGUILayout.EnumPopup("Altitude Unit",control.altitudeUnit);
			GUILayout.Space(3f);
			control.temperatureUnit = (PhantomDisplay.TemperatureUnit)EditorGUILayout.EnumPopup("Temperature Unit",control.temperatureUnit);
			GUILayout.Space(3f);
			control.forceUnit = (PhantomDisplay.ForceUnit)EditorGUILayout.EnumPopup("Force Unit",control.forceUnit);
			GUILayout.Space(3f);
			control.weightUnit = (PhantomDisplay.WeightUnit)EditorGUILayout.EnumPopup("Weight Unit",control.weightUnit);
			GUILayout.Space(3f);
			control.torqueUnit = (PhantomDisplay.TorqueUnit)EditorGUILayout.EnumPopup("Torque Unit",control.torqueUnit);
			EditorGUI.indentLevel--;
		} else if (control.units == PhantomDisplay.UnitsSetup.Metric) {
			//
			control.speedUnit = PhantomDisplay.SpeedUnit.MeterPerSecond;
			control.altitudeUnit = PhantomDisplay.AltitudeUnit.Meter;
			control.temperatureUnit = PhantomDisplay.TemperatureUnit.Celsius;
			control.forceUnit = PhantomDisplay.ForceUnit.Newton;
			control.weightUnit = PhantomDisplay.WeightUnit.Kilogram;
			control.torqueUnit = PhantomDisplay.TorqueUnit.NewtonMeter;
			//
		} else if (control.units == PhantomDisplay.UnitsSetup.Imperial) {
			//
			//
			control.speedUnit = PhantomDisplay.SpeedUnit.Knots;
			control.altitudeUnit = PhantomDisplay.AltitudeUnit.Feet;
			control.temperatureUnit = PhantomDisplay.TemperatureUnit.Fahrenheit;
			control.forceUnit = PhantomDisplay.ForceUnit.PoundForce;
			control.weightUnit = PhantomDisplay.WeightUnit.Pound;
			control.torqueUnit = PhantomDisplay.TorqueUnit.PoundForceFeet;
			//
		}
		//
		GUILayout.Space(5f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output Ports", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(3f);
		control.displayPoints = EditorGUILayout.Toggle ("Show", control.displayPoints);
		if (control.displayPoints) {
			GUILayout.Space(5f);
			control.speed = EditorGUILayout.ObjectField ("Speed Text", control.speed, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.altitude = EditorGUILayout.ObjectField ("Altitude Text", control.altitude, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.climb = EditorGUILayout.ObjectField("Climb Text", control.climb, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.fuel = EditorGUILayout.ObjectField ("Fuel Text", control.fuel, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.weight = EditorGUILayout.ObjectField ("Weight Text", control.weight, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.engineName = EditorGUILayout.ObjectField ("Engine Name Text", control.engineName, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.enginePower = EditorGUILayout.ObjectField ("Engine Power Text", control.enginePower, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.propellerPower = EditorGUILayout.ObjectField("Propeller Power Text", control.propellerPower, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.collective = EditorGUILayout.ObjectField ("Rotor Collective", control.collective, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.density = EditorGUILayout.ObjectField ("Density Text", control.density, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.pressure = EditorGUILayout.ObjectField ("Pressure Text", control.pressure, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.temperature = EditorGUILayout.ObjectField ("Temperature Text", control.temperature, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.Time = EditorGUILayout.ObjectField ("Time Text", control.Time, typeof(Text), true) as Text;
			//
			GUILayout.Space(5f);
			control.brake = EditorGUILayout.ObjectField ("Parking Brake Text", control.brake, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.gearState = EditorGUILayout.ObjectField ("Gear Text", control.gearState, typeof(Text), true) as Text;


			GUILayout.Space(5f);
			control.gload = EditorGUILayout.ObjectField("G Load", control.gload, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.pitchRate = EditorGUILayout.ObjectField("Pitch Rate", control.pitchRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.rollRate = EditorGUILayout.ObjectField("Roll Rate", control.rollRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.yawRate = EditorGUILayout.ObjectField("Yaw Rate", control.yawRate, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.turnRate = EditorGUILayout.ObjectField("Turn Rate", control.turnRate, typeof(Text), true) as Text;

			GUILayout.Space(5f);
			control.weaponCount = EditorGUILayout.ObjectField ("Weapon Count", control.weaponCount, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.currentWeapon = EditorGUILayout.ObjectField ("Current Weapon", control.currentWeapon, typeof(Text), true) as Text;
			GUILayout.Space(3f);
			control.ammoCount = EditorGUILayout.ObjectField ("Ammo Count", control.ammoCount, typeof(Text), true) as Text;
		}
		//
		if (GUI.changed) {
			EditorUtility.SetDirty (control);
			EditorSceneManager.MarkSceneDirty (control.gameObject.scene);
		}
		serializedObject.ApplyModifiedProperties();
	}
}
#endif