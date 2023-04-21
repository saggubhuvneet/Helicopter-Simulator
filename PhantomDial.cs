using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
#if UNITY_EDITOR
using UnityEditor;
#endif




/// <summary>
///
/// 
/// Use:		 Handles the analogue data display
/// </summary>


public class PhantomDial : MonoBehaviour
{

    // ------------------------------------- Selectibles
    public enum DialType
    {
        Speed, Altitude, Fuel, VerticalSpeed, RPM, BankLevel, Compass, ArtificialHorizon, FuelFlow, EGT, Stabilator
    }
    public DialType dialType = DialType.Speed;
    public enum SpeedType { Knots, MPH, KPH }
    public SpeedType speedType = SpeedType.Knots;
    public enum FuelType { kiloGrams, Pounds, Gallons }
    public FuelType fuelType = FuelType.kiloGrams;
    public enum RotationMode { Free, Clamped }
    public RotationMode rotationMode = RotationMode.Clamped;
    public enum NeedleType { Single, Dual }
    public NeedleType needleType = NeedleType.Single;

    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.X;
    public RotationAxis supportRotationAxis = RotationAxis.X;
    public RotationAxis rollRotationAxis = RotationAxis.X;
    public RotationAxis yawRotationAxis = RotationAxis.X;
    public RotationAxis pitchRotationAxis = RotationAxis.X;
    public enum RotationDirection { CW, CCW }
    public RotationDirection direction = RotationDirection.CW;
    public RotationDirection supportDirection = RotationDirection.CW;
    public RotationDirection pitchDirection = RotationDirection.CW;
    public RotationDirection rollDirection = RotationDirection.CW;
    public RotationDirection yawDirection = RotationDirection.CW;
    public enum EngineType { TurboShaft, ElectricMotor, PistonEngine }
    public EngineType engineType = EngineType.PistonEngine;


    // ------------------------------------- Connections
    public PhantomController controller;
    public Transform Needle, supportNeedle;
    public PhantomControlModule dataLog;
    public PhantomTurboShaft shaft;
    public PhantomPistonEngine piston;
    public PhantomAerofoil stabilator;


    // ------------------------------------- Vectors
    Vector3 axisRotation, supportAxisRotation;
    Vector3 pitchAxisRotation, yawAxisRotation, rollAxisRotation;

    private Quaternion baseNormalRotation = Quaternion.identity;
    private Quaternion baseSupportRotation = Quaternion.identity;





    // ------------------------------------- Variables
    public float currentValue, currentSupportValue;
    public float MinimumValue = 0.0f;
    public float MaximumValue = 100.0f;
    public float MinimumAngleDegrees = 0.0f;
    public float MaximumAngleDegrees = 360.0f;

    public float multiplier = 1f;


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeDial()
    {
        axisRotation = Handler.EstimateModelProperties(direction.ToString(), rotationAxis.ToString());
        pitchAxisRotation = Handler.EstimateModelProperties(pitchDirection.ToString(), pitchRotationAxis.ToString());
        rollAxisRotation = Handler.EstimateModelProperties(rollDirection.ToString(), rollRotationAxis.ToString());
        yawAxisRotation = Handler.EstimateModelProperties(yawDirection.ToString(), yawRotationAxis.ToString());
        supportAxisRotation = Handler.EstimateModelProperties(supportDirection.ToString(), supportRotationAxis.ToString());

        if (Needle != null) { baseNormalRotation = Needle.localRotation; }
        else { Debug.LogError("Needle for Dial " + transform.name + " has not been assigned"); return; }
        if (supportNeedle != null) { baseSupportRotation = supportNeedle.localRotation; }
    }






    private void Update()
    {
        if (dataLog != null) { CollectData(); }
        if (Needle != null) { MoveNeedle(); }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void CollectData()
    {
        // --------------------------------- Speed
        if (dialType == DialType.Speed)
        {
            float baseSpeed = dataLog.forwardSpeed;

            if (speedType == SpeedType.Knots) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KTS"); }
            if (speedType == SpeedType.MPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "MPH"); }
            if (speedType == SpeedType.KPH) { currentValue = MathBase.ConvertSpeed(baseSpeed, "KPH"); }
        }

        // -------------------------------- EGT
        if (dialType == DialType.EGT)
        {
            float baseTemp = 0f;

            if (engineType == EngineType.TurboShaft) { baseTemp = shaft.Te; }
            if (engineType == EngineType.PistonEngine) { baseTemp = piston.Te; }
            currentValue = baseTemp / 100f; if (currentValue < 0) { currentValue = 0f; }
            currentSupportValue = Mathf.Floor(baseTemp % 100.0f) / 10f; if (currentSupportValue < 0) { currentSupportValue = 0f; }
        }

        // -------------------------------- Fuel
        if (dialType == DialType.Fuel)
        {
            float baseFuel = controller.fuelLevel / multiplier;
            if (fuelType == FuelType.kiloGrams) { currentValue = baseFuel; }
            if (fuelType == FuelType.Pounds) { currentValue = baseFuel * 2.20462f; }
            if (fuelType == FuelType.Gallons) { currentValue = baseFuel * 0.264172053f; }
        }

        // -------------------------------- RPM
        if (dialType == DialType.RPM)
        {
            if (engineType == EngineType.TurboShaft && shaft != null) { currentValue = shaft.core.coreFactor * 100; }
            if (engineType == EngineType.PistonEngine && piston != null) { currentValue = piston.core.coreFactor * 100; }
            currentSupportValue = Mathf.Floor(currentValue % 10.0f);
        }


        // -------------------------------- Bank
        if (dialType == DialType.BankLevel)
        {
            currentValue = -1 * dataLog.bankAngle;
        }


        // -------------------------------- Stab
        if (dialType == DialType.Stabilator)
        {
            currentValue = stabilator.controlDeflection;
        }


        // -------------------------------- Altitude
        if (dialType == DialType.Altitude)
        {
            float baseAltitude = MathBase.ConvertDistance(dataLog.currentAltitude, "FT");
            currentValue = baseAltitude / 1000f;
            currentSupportValue = Mathf.Floor(baseAltitude / 10000f);
        }


        // -------------------------------- Fuel Flow
        if (dialType == DialType.FuelFlow)
        {
            float baseFuel = controller.fuelSystem.engineConsumption / multiplier;
            if (fuelType == FuelType.kiloGrams) { currentValue = baseFuel; }
            if (fuelType == FuelType.Pounds) { currentValue = baseFuel * 2.20462f; }
            if (fuelType == FuelType.Gallons) { currentValue = baseFuel * 0.264172053f; }
        }


        // -------------------------------- Compass
        if (dialType == DialType.Compass) { currentValue = dataLog.headingDirection; }

        // -------------------------------- Climb Rate
        if (dialType == DialType.VerticalSpeed) { currentValue = (dataLog.verticalSpeed * 196.85f)/1000f; }

        // --------------------- Clamp
        if (rotationMode == RotationMode.Clamped) { currentValue = Mathf.Clamp(currentValue, MinimumValue, MaximumValue); }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void MoveNeedle()
    {
        if (dialType == DialType.EGT || dialType == DialType.RPM)
        {
            float baseDelta = (currentValue - MinimumValue) / (MaximumValue - MinimumValue);
            float supportDelta = (currentSupportValue) / 10;

            float baseDeltaRotation = MinimumAngleDegrees + ((MaximumAngleDegrees - MinimumAngleDegrees) * baseDelta);
            float supportDeltaRotation = (360 * supportDelta);

            // -----------------  Set
            Needle.transform.localRotation = baseNormalRotation; Needle.transform.Rotate(axisRotation, baseDeltaRotation);
            if (supportNeedle != null) { supportNeedle.transform.localRotation = baseSupportRotation; supportNeedle.transform.Rotate(supportAxisRotation, supportDeltaRotation); }
        }

        else if (dialType == DialType.Compass)
        {
            float baseDelta = currentValue / 360f; float baseDeltaRotation = (360 * baseDelta);

            // -----------------  Set
            Needle.transform.localRotation = baseNormalRotation; Needle.transform.Rotate(axisRotation, baseDeltaRotation);
        }

        else if (dialType == DialType.ArtificialHorizon)
        {
            var yaw = controller.transform.eulerAngles.y;
            var pitch = controller.transform.eulerAngles.x;
            var roll = controller.transform.eulerAngles.z;
            if (yawDirection == RotationDirection.CCW) { yaw *= -1f; }
            if (pitchDirection == RotationDirection.CCW) { pitch *= -1f; }
            if (rollDirection == RotationDirection.CCW) { roll *= -1f; }

            var yawEffect = Quaternion.AngleAxis(yaw, yawAxisRotation);
            var pitchEffect = Quaternion.AngleAxis(pitch, pitchAxisRotation);
            var rollEffect = Quaternion.AngleAxis(roll, rollAxisRotation);

            var angleEffect = yawEffect * pitchEffect * rollEffect;
            Needle.localRotation = baseNormalRotation * angleEffect;
        }

        else if (dialType == DialType.Altitude)
        {
            float baseDelta = currentValue / 10;
            float supportDelta = (currentSupportValue) / 10;

            float baseDeltaRotation = (360 * baseDelta);
            float supportDeltaRotation = (360 * supportDelta);

            // -----------------  Set
            Needle.transform.localRotation = baseNormalRotation; Needle.transform.Rotate(axisRotation, baseDeltaRotation);
            if (supportNeedle != null)
            {
                supportNeedle.transform.localRotation = baseSupportRotation; supportNeedle.transform.Rotate(supportAxisRotation, supportDeltaRotation);
            }
        }

        else
        {
            float valueDelta = (currentValue - MinimumValue) / (MaximumValue - MinimumValue);
            float angleDeltaDegrees = MinimumAngleDegrees + ((MaximumAngleDegrees - MinimumAngleDegrees) * valueDelta);

            // ------------------------ Set
            Needle.transform.localRotation = baseNormalRotation;
            Needle.transform.Rotate(axisRotation, angleDeltaDegrees);
        }
    }
}
