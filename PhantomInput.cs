using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oyedoyin;
using System;
//using UnityStandardAssets.CrossPlatformInput;
#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.InputSystem.Controls;
#endif





/// <summary>
/// Handles the input variable and state collection plus processing	 
/// </summary>
/// /// <remarks>
/// This component will collect the inputs from various sources e.g Keyboard, Joystick, VR or custom
/// and process them into control variables for the flight computer. It also contains all the control and
/// command functions for the aircraft operation
/// </remarks>



[Serializable]
public class PhantomInput
{
    public PhantomController controller;
    // ----------------------------------- States
    public enum LightState { On, Off }
    public LightState lightState = LightState.Off;



    // --------------------------------- Variables
    public Vector2 scrollInput;
    public float rollInput, yawInput, pitchInput, throttleInput, collectiveInput, propellerInput;
    public float rawPitchInput, rawRollInput, rawYawInput, rawThrottleInput, rawCollectiveInput;
    public bool inputConfigured = true;
    public bool triggerHeld;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void CollectInputs()
    {
        if (controller.inputType == PhantomController.InputType.Default)
        {

            if((Emergency.istailRoterCableRubture == false))
                rawYawInput = Input.GetAxis("Rudder");


            // ----------------------------------------- Base
            rawPitchInput = Input.GetAxis("Pitch");
            rawRollInput = Input.GetAxis("Roll");
            rawThrottleInput = (Input.GetAxis("Throttle"));
            throttleInput = (rawThrottleInput + 1) / 2;
            rawCollectiveInput = (Input.GetAxis("Collective"));
            collectiveInput = (rawCollectiveInput + 1) / 2;
            propellerInput = (Input.GetAxis("Propeller"));
           

            // ----------------------------------------- Toggles
            if (Input.GetButtonDown("Stop Engine")) { TurnOffEngines(); }
            if (Input.GetButtonDown("Parking Brake")) { ToggleBrakeState(); }
            if (Input.GetButtonDown("Actuate Gear")) { ToggleGearState(); }
            if (Input.GetButtonDown("LightSwitch")) { ToggleLightState(); }
            if (Input.GetButtonDown("Fire")) { FireWeapons(); }
            if (Input.GetButtonDown("Target Up")) { CycleTargetUpwards(); }
            if (Input.GetButtonDown("Target Down")) { CycleTargetDownwards(); }
            if (Input.GetButtonDown("Target Lock")) { LockTarget(); }
            if (Input.GetKeyDown(KeyCode.Backspace)) { ReleaseTarget(); }
            if (Input.GetButtonDown("Weapon Select")) { SwitchWeapons(); }
            if (Input.GetKeyDown(KeyCode.C)) { ToggleCameraState(); }
            if (Input.GetButton("Fire")) { if (!triggerHeld) { triggerHeld = true; } } else { if (triggerHeld) { triggerHeld = false; } }
            if (Input.GetKeyDown(KeyCode.F)) { ExitAircraftSwitch(); }
            if (Input.GetKeyDown(KeyCode.R)) { controller.ResetScene(); }


            if(Emergency.isEngienFailedTosart == true) { 
                return; }
            if (Input.GetButtonDown("Start Engine")) { TurnOnEngines(); }

        }
        else if (controller.inputType == PhantomController.InputType.VR)
        {
            if (controller.controlStick != null)
            {
                rollInput = controller.controlStick.rollOutput;
                pitchInput = controller.controlStick.pitchOutput;
            }
            if (controller.throttleLever != null) { throttleInput = controller.throttleLever.leverOutput; }
            if (controller.collectiveLever != null) { collectiveInput = controller.collectiveLever.leverOutput; }
        }

        else
        {
             //---------------------------- Your Custom Input Code
        }



        // ---------------------------------------- Base Input
        controller.flightComputer.basePitchInput = rawPitchInput;
        controller.flightComputer.baseRollInput = rawRollInput;
        controller.flightComputer.baseYawInput = rawYawInput;
        controller.flightComputer.baseCollectiveInput = collectiveInput;
        controller.flightComputer.baseThrottleInput = throttleInput;
        controller.flightComputer.basePropellerInput = propellerInput;
    }







    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateInput()
    {

#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
        // -------------------------------------- Re-initialize Inputs
        if (controlSource == null)
        {
            controlSource = new SilantroControl();
            controlSource.Enable();
        }
#endif


        //if (Application.isFocused)
        //{

            // ---------------------------------------- Collect Input
            CollectInputs();


            if (controller != null && controller.allOk)
            {

                // ------------------------------ Send Engine Data
                if (controller.engineType == PhantomController.EngineType.Turboshaft) { foreach (PhantomTurboShaft engine in controller.shafts) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
                if (controller.engineType == PhantomController.EngineType.Piston) { foreach (PhantomPistonEngine engine in controller.pistons) { engine.core.controlInput = controller.flightComputer.processedThrottle; } }
                if (controller.engineType == PhantomController.EngineType.Electric && controller.craftMode != PhantomController.CraftMode.Quadcopter) { foreach (PhantomElectricMotor engine in controller.motors) { engine.controlInput = controller.flightComputer.processedThrottle; } }


                // ---------------------------------------- Rotors
                foreach (PhantomRotor rotor in controller.rotors)
                {
                    if (rotor != null)
                    {
                        if (rotor.rotorType == PhantomRotor.RotorType.TailRotor) { rotor.collectiveInput = controller.flightComputer.processedYaw; }
                        else
                        {
                            rotor.collectiveInput = controller.flightComputer.processedCollective;
                            rotor.pitchInput = controller.flightComputer.processedPitch;
                            rotor.rollInput = controller.flightComputer.processedRoll;
                            rotor.yawInput = controller.flightComputer.processedYaw;
                            rotor.propellerInput = controller.flightComputer.processedPropeller;
                        }
                    }
                }


                // ---------------------------------------- Wings
                foreach (PhantomAerofoil foil in controller.foils)
                {
                    foil.pitchInput = -controller.flightComputer.processedPitch;
                    foil.rollInput = controller.flightComputer.processedRoll;
                    foil.yawInput = controller.flightComputer.processedYaw;
                }

                // ------------------------------------------ Gear
                if (controller.gearHelper != null) { controller.gearHelper.rudderInput = controller.flightComputer.processedYaw; }



                // --------------------------------------- Weapons
                if (controller.hardpoints != null)
                {

                    if (controller.hardpoints.attachedGuns != null && controller.hardpoints.attachedGuns.Length > 0 && controller.hardpoints.currentWeapon == "Gun")
                    {
                        if (triggerHeld)
                        {
                            controller.hardpoints.FireGuns();

                            foreach (PhantomMachineGun gun in controller.hardpoints.attachedGuns)
                            { if (gun.currentAmmo > 0 && gun.canFire) { gun.running = true; } }
                        }
                        else
                        {
                            foreach (PhantomMachineGun gun in controller.hardpoints.attachedGuns)
                            { if (gun.running) { gun.running = false; } }
                        }
                    }
                }
            }
        //}
    }













    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // ---------------------------------------------------CONTROL FUNCTIONS--------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOnEngines()
    {
        if (controller.isControllable)
        {
            Debug.Log("engine is strated");
            if (controller.engineType == PhantomController.EngineType.Piston) { foreach (PhantomPistonEngine engine in controller.pistons) { if (!engine.core.active) { engine.core.StartEngine(); } } }
            if (controller.engineType == PhantomController.EngineType.Turboshaft) { foreach (PhantomTurboShaft engine in controller.shafts) { if (!engine.core.active) { engine.core.StartEngine(); } } }
            if (controller.engineType == PhantomController.EngineType.Electric) { foreach (PhantomElectricMotor engine in controller.motors) { if (!engine.active) { engine.start = true; } } }
        }
    }


    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void TurnOffEngines()
    {
        if (controller.engineType == PhantomController.EngineType.Piston) { foreach (PhantomPistonEngine engine in controller.pistons) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
        if (controller.engineType == PhantomController.EngineType.Turboshaft) { foreach (PhantomTurboShaft engine in controller.shafts) { if (engine.core.active) { engine.core.ShutDownEngine(); } } }
        if (controller.engineType == PhantomController.EngineType.Electric) { foreach (PhantomElectricMotor engine in controller.motors) { if (engine.active) { engine.stop = true; } } }
    }

    /// <summary>
    /// Fire the rockets or missiles connected to the aircraft weapons system
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void FireWeapons()
    {
        if (controller.hardpoints != null)
        {
            if (controller.hardpoints.currentWeapon == "Missile") { controller.hardpoints.FireMissile(); }
            else if (controller.hardpoints.currentWeapon == "Rocket") { controller.hardpoints.FireRocket(); }
        }
    }



    public void SwitchWeapons() { if (controller.hardpoints != null) { controller.hardpoints.ChangeWeapon(); } }
    /// <summary>
    /// Scroll up through the discovered/tracked object list
    /// </summary>
    public void CycleTargetUpwards() { if (controller.radar != null) { controller.radar.SelectedUpperTarget(); } }
    /// <summary>
    /// Scroll down through the discovered/tracked object list
    /// </summary>
    public void CycleTargetDownwards() { if (controller.radar != null) { controller.radar.SelectLowerTarget(); } }
    /// <summary>
    /// Lock the selected target object
    /// </summary>
    public void LockTarget() { if (controller.radar != null) { controller.radar.LockSelectedTarget(); } }
    /// <summary>
    /// Unlock the selected/locked target object
    /// </summary>
    public void ReleaseTarget() { if (controller.radar != null) { controller.radar.ReleaseLockedTarget(); } }

    /// <summary>
    /// Engage or disengage the gear actuator
    /// </summary>
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleGearState()
    {
        if (controller.isControllable)
        {
            if (controller != null && controller.gearActuator != null)
            {
                if (controller.gearActuator.actuatorState == PhantomActuator.ActuatorState.Disengaged) { controller.gearActuator.EngageActuator(); }
                else { controller.gearActuator.DisengageActuator(); }
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleBrakeState() { if (controller.isControllable) { if (controller != null && controller.gearHelper != null) { controller.gearHelper.ToggleBrakes(); } } }
    public void ToggleLightState() { if (controller != null && controller.isControllable) { foreach (PhantomBulb light in controller.lightBulbs) { if (light.state == PhantomBulb.CurrentState.On) { light.SwitchOff(); } else { light.SwitchOn(); } } } }
    public void TurnOffLights() { if (controller != null && controller.isControllable) { foreach (PhantomBulb light in controller.lightBulbs) { if (light.state == PhantomBulb.CurrentState.On) { light.SwitchOff(); } } } }
    public void ToggleCameraState() { if (controller != null && controller.isControllable && controller.view != null) { controller.view.ToggleCamera(); } }
    
    
    
    // -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ExitAircraftSwitch()
    {
        if (controller.controlType == PhantomController.ControlType.Internal && !controller.temp)
        {
            if (controller.AGL < 10f && controller.core.forwardSpeed < 1) { controller.ExitAircraft(); controller.opened = false; controller.temp = false; }
        }
    }
}
