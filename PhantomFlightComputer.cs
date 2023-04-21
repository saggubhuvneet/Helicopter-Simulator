using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Oyedoyin;




/// <summary>
/// 
/// Use:		 Handles the processing of inputs and manages the helicopter control state
/// </summary>

public class PhantomFlightComputer : MonoBehaviour
{

    // ------------------------------------------ Selectibles
    public enum AugmentationType { Manual, StabilityAugmentation, CommandAugmentation, Autonomous }
    public AugmentationType operationMode = AugmentationType.Manual;
    public enum ControlType { TwoAxis, ThreeAxis }
    public ControlType controlType = ControlType.TwoAxis;
    public enum ControlState { Active, Off }
    public ControlState autoThrottle = ControlState.Off;
    public ControlState speedHold = ControlState.Off;
    public ControlState headingHold = ControlState.Off;
    public ControlState bankHold = ControlState.Off;
    public ControlState pitchHold = ControlState.Off;
    public ControlState altitudeHold = ControlState.Off;
    public ControlState bankLimiter = ControlState.Off;
    public ControlState pitchLimiter = ControlState.Off;
    public ControlState gLimiter = ControlState.Off;
    public ControlState attenuation = ControlState.Off;
    public enum BankLimitState { Off, Left, Right }
    public BankLimitState bankState = BankLimitState.Off;
    public enum PitchLimitState { Off, Up, Down }
    public PitchLimitState pitchState = PitchLimitState.Off;
    public enum HeadingMethod { BankTurn, FlatTurn }
    public HeadingMethod headingMethod = HeadingMethod.BankTurn;
    public enum PropStae { Balance, Free}
    public PropStae propStae = PropStae.Balance;

    // ------------------------------------------ Base Input
    public float basePitchInput;
    public float baseRollInput;
    public float baseYawInput;
    public float baseThrottleInput;
    public float baseCollectiveInput;
    public float basePropellerInput;


    // ------------------------------------------ Control Output
    public float processedPitch;
    public float processedRoll;
    public float processedYaw;
    public float processedThrottle;
    public float processedCollective;
    public float processedPropeller;


    // ------------------------------------------ Connections
    public PhantomController controller;
    public PhantomControlModule core;
    public Transform homePoint;
    public AnimationCurve pitchInputCurve;
    public AnimationCurve rollInputCurve;
    public AnimationCurve yawInputCurve;
    public PhantomRotor propeller;
    public PhantomActuator gearSolver;



    //--------------------------------------------- Dead Zones
    public float pitchDeadZone = 0.05f;
    public float rollDeadZone = 0.05f;
    public float yawDeadZone = 0.05f;
    [Range(0, 0.5f)] public float rollBreakPoint = 0.4f;
    [Range(0, 0.5f)] public float pitchBreakPoint = 0.4f;
    [Range(1, 3)] public float pitchScale = 1, rollScale = 1, yawScale = 1;
    public bool allOk;



    // ------------------------------------------ Rotation
    public float pitchAngle;
    public float rollAngle;
    public float yawAngle;
    // ------------------------------------------ Rotation
    public float pitchRate;
    public float rollRate;
    public float yawRate;
    public float turnRate;
    // ------------------------------------------ Performance
    public float currentSpeed;
    public float currentAltitude;
    public float currentClimbRate;
    public float currentHeading;
    public float currentDriftSpeed;
    public float timeStep;
    // ------------------------------------------ Command Inputs
    public float commandRollInput = 0f;
    public float commandYawInput = 0f;
    public float commandPitchInput = 0f;
    float presetRollInput = 0f;
    float presetYawInput = 0f;
    float presetPitchInput = 0f;
    public float knotSpeed, ftHeight;


    public bool rollLimitActive;
    public bool pitchLimitActive;
    public bool gLimitActive;



    // ------------------------------------------ Solvers
    //1. Inner Loop
    public PhantomPID rollRateSolver;
    public PhantomPID pitchRateSolver;
    public PhantomPID yawRateSolver;

    //2. Outer Loop A
    public PhantomPID rollAngleSolver;
    public PhantomPID pitchAngleSolver;
    public PhantomPID yawAngleSolver;
    public PhantomPID verticalClimbSolver;
    public PhantomPID forwardClimbSolver;
    public PhantomPID altitudeSolver;
    public PhantomPID turnSolver;
    public PhantomPID propellerSolver;
    public PhantomPID driftSolver;
    public PhantomPID speedSolver;


    // -------------------------------------------- Command Variables
    public float commandSpeed = 50f;//Knots
    public float commandRollRate = 0f;
    public float commandPitchRate = 0f;
    public float commandYawRate = 0f;
    public float commandTurnRate = 0f;
    public float commandBankAngle = 0f;
    public float commandPitchAngle = 0f;
    public float commandAltitude = 1000f;
    public float commandClimbRate = 500f;
    public float commandHeading = 0f;
    public float commandG = 1f, loadCommand = 1f;


    // ------------------------------------------ Errors
    public float altitudeError;
    public float speedError;
    public float climbRateError;
    public float headingError;
    public float gError;
    public float pitchAngleError, rollAngleError, yawAngleError;
    public float pitchRateError, rollRateError, yawRateError;



    // ------------------------------------------ Limits
    public float balanceRollRate = 10f;
    public float balancePitchRate = 10f;
    public float balanceYawRate = 10f;
    public float maximumBankAngle = 30f;
    public float maximumTurnBank = 20f;
    public float maximumClimbRate = 500f;
    public float maximumTurnRate = 4f;
    public float maximumPitchAngle = 30f;//Nose Up
    public float minimumPitchAngle = 15f;//Nose Down
    public float maximumRollRate = 60f;
    public float maximumPitchRate = 30f;
    public float presetRollAngle, presetPitchAngle = 0;
    public float balancePitchAngle = 0, balanceRollAngle = 0f;
    public float maximumLoadFactor = 3.5f;
    public float minimumLoadFactor = 2f;


    public float m_sas_authority = 10f;
    public float m_sas_pitch;
    public float m_sas_roll;
    public float m_sas_yaw;
    public float m_sas_collective;
    public float m_αMax;


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeComputer()
    {
        //----------------------------
        _checkPrerequisites();

        if (allOk)
        {
            //baseHeading = controller.core.headingDirection;
            //basePitchRate = cruisePitchRate;
            //controlCylceRate = 60f;

            // --------------------------------------
            _plotInputCurve();

            plotClimbCurve(safeHeight, out m_climb_curve);


            if (controller.input.inputConfigured) { Debug.Log("Flight computer on " + controller.transform.name + " is starting in " + operationMode.ToString() + " mode"); }
        }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void _checkPrerequisites()
    {
        //CHECK COMPONENTS
        if (controller != null && core != null) { allOk = true; }
        else if (controller == null) { Debug.LogError("Prerequisites not met on FCS " + " " + transform.name + "....operational aircraft not assigned"); allOk = false; }
        else if (core == null) { Debug.LogError("Prerequisites not met on FCS " + " " + transform.name + "....data point not connected"); allOk = false; }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void _plotInputCurve()
    {
        pitchInputCurve = MathBase.PlotControlInputCurve(pitchScale);
        rollInputCurve = MathBase.PlotControlInputCurve(rollScale);
        yawInputCurve = MathBase.PlotControlInputCurve(yawScale);
    }









    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Update()
    {
        if (allOk && controller != null && controller.isControllable)
        {
            timeStep = Time.deltaTime; if (timeStep > Time.fixedDeltaTime) { timeStep = Time.fixedDeltaTime; }//STUTTER PROTECTION
            knotSpeed = currentSpeed * MathBase.toKnots;
            ftHeight = currentAltitude * MathBase.toFt;


            // ----------------------------------------------- Noise Filter
            if (Mathf.Abs(baseRollInput) > rollDeadZone) { presetRollInput = baseRollInput; } else { presetRollInput = 0f; }
            if (Mathf.Abs(basePitchInput) > pitchDeadZone) { presetPitchInput = basePitchInput; } else { presetPitchInput = 0f; }
            if (Mathf.Abs(baseYawInput) > yawDeadZone) { presetYawInput = baseYawInput; } else { presetYawInput = 0f; }


            float filteredPitchInput = ((Mathf.Abs(presetPitchInput) - pitchDeadZone) / (1 - pitchDeadZone)) * presetPitchInput;
            float filteredRollInput = ((Mathf.Abs(presetRollInput) - rollDeadZone) / (1 - rollDeadZone)) * presetRollInput;
            float filteredYawInput = ((Mathf.Abs(presetYawInput) - yawDeadZone) / (1 - yawDeadZone)) * presetYawInput;


            // ----------------------------------------------- Curve Filter
            commandPitchInput = pitchInputCurve.Evaluate(filteredPitchInput);
            commandRollInput = rollInputCurve.Evaluate(filteredRollInput);
            commandYawInput = yawInputCurve.Evaluate(filteredYawInput);


            //--------------------------------------------- Estimate Rotation Angles
            float rawPitchAngle = controller.transform.eulerAngles.x; if (rawPitchAngle > 180) { rawPitchAngle -= 360f; }
            float rawRollAngle = controller.transform.eulerAngles.z; if (rawRollAngle > 180) { rawRollAngle -= 360f; }
            float rawYawAngle = controller.transform.eulerAngles.y; if (rawYawAngle > 180) { rawYawAngle -= 360f; }

            rollAngle = (float)Math.Round(rawRollAngle, 2);
            pitchAngle = (float)Math.Round(rawPitchAngle, 2);
            yawAngle = (float)Math.Round(rawYawAngle, 2);
            m_αMax = -90f; foreach (PhantomRotor rotor in controller.forceRotors) { if (rotor.αMax > m_αMax) { m_αMax = rotor.αMax; } }

            pitchRate = core.pitchRate;
            yawRate = core.yawRate;
            rollRate = core.rollRate;
            turnRate = core.turnRate;


            // --------------------------------------------- Collect Performance Data
            currentAltitude = core.currentAltitude;
            currentClimbRate = core.verticalSpeed;
            currentSpeed = Vector3.Dot(controller.transform.forward, controller.helicopter.velocity);
            currentHeading = core.headingDirection; if (currentHeading > 180) { currentHeading -= 360f; }
            currentDriftSpeed = core.driftSpeed;

        
            //---------------------------------------------OPERATION MODE
            switch (operationMode)
            {

                case AugmentationType.Manual: ManualControl(); break;
                case AugmentationType.StabilityAugmentation: AssitedControl(); break;
                case AugmentationType.CommandAugmentation: CommandControl(); break;
                //case AugmentationType.Autonomous: brain.UpdateBrain(); break;
            }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Cyclic deflection is directly proportional to the control inputs i.e the surfaces are driven
    /// directly from the unfilter inputs
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void ManualControl()
    {
        processedPitch = commandPitchInput;
        processedRoll = commandRollInput;
        processedYaw = commandYawInput;
        processedCollective = baseCollectiveInput;
        processedThrottle = baseThrottleInput;
       

        // ---------------- Propeller Throttle Control
        if (propeller != null && propStae == PropStae.Balance)
        {
            float presetSpeed = commandSpeed / MathBase.toKnots;
            speedError = (presetSpeed - currentSpeed);
            processedPropeller = -propellerSolver.CalculateOutput(speedError, timeStep);
            if (float.IsNaN(processedPropeller) || float.IsInfinity(processedPropeller)) { processedPropeller = 0f; }
        }
        else { processedPropeller = basePropellerInput; }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Cyclic deflection is driven based on the output commands from the flight computer e.g roll rate, pitch rate e.t.c
    /// </summary>
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void CommandControl()
    {
        processedCollective = baseCollectiveInput;
        processedThrottle = baseThrottleInput;
        // ---------------- Propeller Throttle Control
        if (propeller != null && propStae == PropStae.Balance)
        {
            float presetSpeed = commandSpeed / MathBase.toKnots;
            speedError = (presetSpeed - currentSpeed);
            processedPropeller = -propellerSolver.CalculateOutput(speedError, timeStep);
            if (float.IsNaN(processedPropeller) || float.IsInfinity(processedPropeller)) { processedPropeller = 0f; }
        }
        else { processedPropeller = basePropellerInput; }




        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------PITCH AXIS-----------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        if (pitchLimiter == ControlState.Active)
        {
            if (basePitchInput > pitchDeadZone && pitchAngle > minimumPitchAngle) { presetPitchAngle = -minimumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Down; }
            else if (basePitchInput < -pitchDeadZone && pitchAngle < -maximumPitchAngle) { presetPitchAngle = maximumPitchAngle; pitchLimitActive = true; pitchState = PitchLimitState.Up; }

            if (pitchLimitActive)
            {
                if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
                if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { pitchLimitActive = false; pitchState = PitchLimitState.Off; presetPitchAngle = 0f; } }
            }
        }

        // -------------------------------------------------- G Limiter
        if (gLimiter == ControlState.Active)
        {
            if (basePitchInput > pitchDeadZone && core.gForce < -minimumLoadFactor) { commandG = -minimumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Down; }
            else if (basePitchInput < -pitchDeadZone && core.gForce > maximumLoadFactor) { commandG = maximumLoadFactor; gLimitActive = true; pitchState = PitchLimitState.Up; }

            if (gLimitActive)
            {
                if (pitchState == PitchLimitState.Up) { if (basePitchInput > -pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
                if (pitchState == PitchLimitState.Down) { if (basePitchInput < pitchDeadZone) { gLimitActive = false; pitchState = PitchLimitState.Off; commandG = 0f; } }
            }
        }



        if (pitchHold == ControlState.Off)
        {

            if (!gLimitActive)
            {
                if (!pitchLimitActive)
                {

                    float baseCommand = -cruisePitchRate;
                    commandPitchRate = commandPitchInput > 0f ? commandPitchInput * baseCommand : commandPitchInput * (baseCommand);
                    pitchRateError = pitchRate - commandPitchRate;
                    processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                }
                else
                {
                    // -------------------------------------------- Pitch Rate Required
                    pitchAngleError = pitchAngle - (-1f * presetPitchAngle);
                    pitchAngleSolver.maximum = maximumPitchRate; pitchAngleSolver.minimum = 0.5f * -maximumPitchRate;
                    commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

                    pitchRateError = pitchRate - commandPitchRate;
                    processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
                }
            }
            else
            {
                commandPitchRate = (1845f * (commandG)) / (currentSpeed * MathBase.toFt);
                pitchRateError = pitchRate - (commandPitchRate);
                processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
            }
        }
        else
        {
            // -------------------------------------------- Pitch Rate Required
            pitchAngleError = pitchAngle - (-1f * commandPitchAngle);
            pitchAngleSolver.minimum = -balancePitchRate; pitchAngleSolver.maximum = balancePitchRate;
            commandPitchRate = pitchAngleSolver.CalculateOutput(pitchAngleError, timeStep);

            pitchRateError = pitchRate - commandPitchRate;
            processedPitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
        }







        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------ROll AXIS------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        
        if (bankHold == ControlState.Off)
        {
            // -------------------------------------------------- Bank Limiter
            if (bankLimiter == ControlState.Active)
            {
                if (baseRollInput > rollDeadZone && rollAngle < -(maximumBankAngle)) { presetRollAngle = maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Right; }
                else if (baseRollInput < -rollDeadZone && rollAngle > (maximumBankAngle)) { presetRollAngle = -maximumBankAngle; rollLimitActive = true; bankState = BankLimitState.Left; }

                if (rollLimitActive)
                {
                    if (bankState == BankLimitState.Left) { if (baseRollInput > rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                    if (bankState == BankLimitState.Right) { if (baseRollInput < -rollBreakPoint) { rollLimitActive = false; bankState = BankLimitState.Off; presetRollAngle = 0f; } }
                }
            }
            if (!rollLimitActive)
            {
                commandRollRate = commandRollInput * cruiseRollRate;
                rollRateError = commandRollRate - rollRate;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }
            else
            {
                // -------------------------------------------- Roll Rate Required
                rollAngleError = rollAngle - (-1f * presetRollAngle);
                rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
                commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

                rollRateError = commandRollRate - rollRate;
                processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
            }
        }
        else
        {
            // -------------------------------------------- Roll Rate Required
            rollAngleError = rollAngle - (-1f * commandBankAngle);
            rollAngleSolver.minimum = -balanceRollRate; rollAngleSolver.maximum = balanceRollRate;
            commandRollRate = rollAngleSolver.CalculateOutput(rollAngleError, timeStep);

            rollRateError = commandRollRate - rollRate;
            processedRoll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
        }



        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        // -------------------------------------------------------------YAW AXIS------------------------------------------------------------------------------------
        // ----------------------------------------------------------------------------------------------------------------------------------------------------------
        commandYawRate = commandYawInput * cruiseYawRate;
        yawRateError = commandYawRate - yawRate;
        processedYaw = yawRateSolver.CalculateOutput(yawRateError, timeStep);
    }


    public float cruisePitchRate = 20f;
    public float cruiseRollRate = 50f;
    public float cruiseYawRate = 50f;



    public Vector3 m_pitch_gain;
    public Vector3 m_roll_gain;
    public Vector3 m_yaw_gain;
    public float m_pitch_factor = 1;
    public float m_roll_factor = 1;
    public float m_max_speed = 160f;
    public AnimationCurve m_climb_curve;
    public float safeHeight = 50f, m_climb_factor;

    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void AssitedControl()
    {
        // ----------------------- Stabilization Bias
        float m_pitch_bias = 0.9f - (0.8f * Mathf.Abs(commandPitchInput));
        float m_roll_bias = 0.9f - (0.8f * Mathf.Abs(commandRollInput));
        float m_yaw_bias = m_sas_authority / 100f;
        float baseKnot = core.forwardSpeed * MathBase.toKnots;
        m_climb_factor = m_climb_curve.Evaluate(currentAltitude * MathBase.toFt);

        if (altitudeHold == ControlState.Active)
        {
            float m_target_altitude = commandAltitude / MathBase.toFt;
            altitudeError = m_target_altitude - currentAltitude;
            float m_limit = commandClimbRate / MathBase.toFtMin;
            altitudeSolver.minimum = -(Mathf.Clamp(m_limit, 2.536f, (maximumClimbRate/MathBase.toFtMin))) * m_climb_factor; 
            altitudeSolver.maximum = m_limit * m_climb_factor;
            float m_climb_rate = altitudeSolver.CalculateOutput(altitudeError, timeStep);

            climbRateError = m_climb_rate - currentClimbRate;
            processedCollective = verticalClimbSolver.CalculateOutput(climbRateError, timeStep);
        }
        else { processedCollective = baseCollectiveInput; }
        processedThrottle = baseThrottleInput;

        float m_factor_p = 1 + (((m_pitch_factor - 1) * baseKnot) / m_max_speed);
        pitchRateSolver.Kp = m_pitch_gain.x / m_factor_p;
        pitchRateSolver.Ki = m_pitch_gain.y / m_factor_p;
        pitchRateSolver.Kd = m_pitch_gain.z / m_factor_p;


        pitchRateError = pitchRate;
        pitchRateSolver.maximum = m_pitch_bias;
        pitchRateSolver.minimum = -m_pitch_bias;
        m_sas_pitch = pitchRateSolver.CalculateOutput(pitchRateError, timeStep);
        processedPitch = commandPitchInput + m_sas_pitch;


        float m_factor_r = 1 + (((m_roll_factor - 1) * baseKnot) / m_max_speed);
        rollRateSolver.Kp = m_roll_gain.x / m_factor_r;
        rollRateSolver.Ki = m_roll_gain.y / m_factor_r;
        rollRateSolver.Kd = m_roll_gain.z / m_factor_r;

        rollRateError = -rollRate;
        rollRateSolver.maximum = m_roll_bias;
        rollRateSolver.minimum = -m_roll_bias;
        m_sas_roll = rollRateSolver.CalculateOutput(rollRateError, timeStep);
        processedRoll = commandRollInput - driftSolver.CalculateOutput(currentDriftSpeed, timeStep);


        yawRateError = -yawRate;
        //yawRateSolver.maximum = m_yaw_bias;
        //yawRateSolver.minimum = -m_yaw_bias;
        //m_sas_yaw = yawRateSolver.CalculateOutput(yawRateError, timeStep);
        processedYaw = commandYawInput;// + m_sas_yaw;
    }



    public void plotClimbCurve(float safeHeight, out AnimationCurve curve)
    {
        curve = new AnimationCurve();
        curve.AddKey(new Keyframe(0.0f * safeHeight, 0.0f));
        curve.AddKey(new Keyframe(0.1f * safeHeight, 0.1f));
        curve.AddKey(new Keyframe(0.2f * safeHeight, 0.2f));
        curve.AddKey(new Keyframe(0.4f * safeHeight, 0.4f));
        curve.AddKey(new Keyframe(0.6f * safeHeight, 0.6f));
        curve.AddKey(new Keyframe(0.8f * safeHeight, 0.8f));
        curve.AddKey(new Keyframe(1.0f * safeHeight, 1.0f));

#if UNITY_EDITOR
        for (int i = 0; i < curve.keys.Length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Auto);
        }
#endif
    }
}
