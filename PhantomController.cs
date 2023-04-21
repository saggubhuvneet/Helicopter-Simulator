using System.Collections;
using System.Collections.Generic;
using UnityEngine;




/// <summary>
/// Handles the collection and organization of all the connected helicopters components
/// </summary>
/// <remarks>
/// This component will collect the components connected to the aircraft root and set them up with the variables and components they
/// need to function properly. It also runs the core control functions in the dependent child components
/// </remarks>

 

[RequireComponent(typeof(Rigidbody))]
public class PhantomController : MonoBehaviour
{

    public enum CraftMode { Helicopter, Quadcopter}
    public CraftMode craftMode = CraftMode.Helicopter;
    public enum TorqueState { Applied, Isolated }
    public TorqueState torqueState = TorqueState.Isolated;
    public enum StartMode { Cold, Hot }
    /// <summary>
    /// The aircraft start mode. Cold and dark or Hot and running
    /// </summary>
    public StartMode startMode = StartMode.Cold;
    public PhantomFlightComputer.ControlState quickStart = PhantomFlightComputer.ControlState.Active;
    public PhantomCamera.CameraState cameraState = PhantomCamera.CameraState.Exterior;
    public enum GearState { Stowed, Open }
    public GearState gearState = GearState.Open;
    public enum EngineType { Piston, Turboshaft, Electric }
    public EngineType engineType = EngineType.Turboshaft;
    public enum LightState { On, Off }
    public LightState lightState = LightState.Off;
    public enum JetType { JetB, JetA1, JP6, JP8 }
    public JetType jetType = JetType.JetB;
    public enum GasType { AVGas100, AVGas100LL, AVGas82UL }
    public GasType gasType = GasType.AVGas100;
    public enum PlayerType { ThirdPerson, FirstPerson }
    public PlayerType playerType = PlayerType.ThirdPerson;
    public enum InputType { Default, VR, Mobile, Custom }
    public InputType inputType = InputType.Default;
    public enum ControlType { External, Internal }
    [Tooltip("Switch to Internal for 3rd or First person Enter-Exit setup")] public ControlType controlType = ControlType.External;
   



    // -------------------------------- Data
    public float baseAcceleration;
    public bool isControllable, allOk;
    public bool opened = false;
    public bool temp = false;
    public float δD = 1f;
    public bool pilotOnboard;
    public bool isRunning;
    public float powerFactor;
    public int engineCount;
    public float initialDrag;
    public float currentDrag;
    public float actuatorDrag;
    // -------------------------------- Hot Start
    public float startSpeed = 50f;
    public float startAltitude = 500f;


    // -------------------------------- Weight
    public float emptyWeight = 1000f;
    public float currentWeight;
    public float maximumWeight = 5000f;


    // -------------------------------- Fuel
    public float combustionEnergy;
    public float fuelLevel;
    public bool lowFuel, fuelExhausted;
    public float TotalFuelCapacity;
    public float totalConsumptionRate;
    public float batteryLevel = 100f;
    


    // -------------------------------- Components
    public Rigidbody helicopter;
    public PhantomController controller;
    public PhantomInput input;
    public PhantomControlModule core;
    public PhantomFlightComputer flightComputer;
    public PhantomTransmission transmission;


    // ---------------Player
    public Transform getOutPosition;
    public GameObject player;
    public GameObject interiorPilot;
    public PhantomDisplay canvasDisplay;
    public float AGL;


    public Camera mainCamera;
    public PhantomCamera view;
    public GameObject ArmamentsStorage;
    public PhantomGearSystem gearHelper;
    public PhantomFuelSystem fuelSystem;
    public PhantomRadar radar;
    public PhantomArmament hardpoints;
    public PhantomHelper helper;
    public PhantomGunControl gunControl;


    public List<PhantomRotor> forceRotors;
    public PhantomTurboShaft[] shafts;
    public PhantomPistonEngine[] pistons;
    public PhantomElectricMotor[] motors;
    public PhantomFuelTank[] fuelTanks;
    public PhantomCrew[] crew;
    public PhantomCargo[] cargo;
    public PhantomRotor[] rotors;
    public PhantomAerofoil[] foils;
    public PhantomBulb[] lightBulbs;
    public PhantomBody[] dragForms;
    public PhantomESC esc;
    public PhantomDial[] dials;
    public PhantomLever[] levers;


    // ---------- Actuators
    public PhantomActuator[] actuators;
    public PhantomActuator gearActuator;
    public PhantomActuator canopyActuator;


    // -------------------------------- Variables
    public Vector3 baseWind = Vector3.zero;
    public Vector3 basePosition;
    public Quaternion baseRotation;
    public List<string> inputList;


    // ---------------VR Controls
    public SilantroVirtualLever controlStick;
    public SilantroVirtualLever throttleLever;
    public SilantroVirtualLever collectiveLever;



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start() { InitializeHelicopter(); }
    



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    //ACTIVATE AIRCRAFT CONTROLS
    /// <summary>
    /// Sets the state of the aircraft control.
    /// </summary>
    /// <param name="state">If set to <c>true</c> aircraft is controllable.</param>
    public void SetControlState(bool state) { isControllable = state; }
    public void TurnOnEngines() { input.TurnOnEngines(); }
    public void TurnOffEngines() { input.TurnOffEngines(); }
    public void CleanupGameobject(GameObject trash) { Destroy(trash); }
    public void ToggleGearState() { input.ToggleGearState(); }
    public void ToggleBrakeState() { input.ToggleBrakeState(); }
    public void ToggleLightState() { input.ToggleLightState(); }
    public void RestoreAircraft() { helper.RestoreFunction(helicopter, controller); }
    public void ResetScene() { UnityEngine.SceneManagement.SceneManager.LoadScene(this.gameObject.scene.name); }
    public void PositionAircraft() { helper.PositionAircraftFunction(helicopter, controller); }
    public void StartAircraft() { helper.StartAircraftFunction(helicopter, controller); }
    public void RefreshWeapons() { helper.RefreshWeaponsFunction(controller); }
    public void EnterAircraft() { helper.EnterAircraftFunction(controller); }
    public void ExitAircraft() { helper.ExitAircraftFunction(controller); }
    public void ThirdPersonCall() { StartCoroutine(helper.EnableTPControls(controller)); }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    protected void _checkPrerequisites()
    {
        if (core != null && helicopter != null && rotors.Length > 1 && flightComputer != null) { allOk = true; }
        else if (helicopter == null) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... rigidbody not assigned"); allOk = false; return; }
        else if (core == null) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... control module not assigned"); allOk = false; return; }
        else if (rotors.Length <= 1) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... rotor System not properly configured"); allOk = false; return; }
        else if (flightComputer == null) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... flight computer not connected"); allOk = false; return; }
        else if (transmission == null) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... transmission not connected"); allOk = false; return; }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void InitializeHelicopter()
    {

        // --------------------- Collect Components
        helicopter = GetComponent<Rigidbody>();
        controller = GetComponent<PhantomController>();
        core = GetComponentInChildren<PhantomControlModule>();
        rotors = GetComponentsInChildren<PhantomRotor>();
        shafts = GetComponentsInChildren<PhantomTurboShaft>();
        motors = GetComponentsInChildren<PhantomElectricMotor>();
        pistons = GetComponentsInChildren<PhantomPistonEngine>();
        fuelTanks = GetComponentsInChildren<PhantomFuelTank>();
        foils = GetComponentsInChildren<PhantomAerofoil>();
        gearHelper = GetComponentInChildren<PhantomGearSystem>();
        lightBulbs = GetComponentsInChildren<PhantomBulb>();
        view = GetComponentInChildren<PhantomCamera>();
        crew = GetComponentsInChildren<PhantomCrew>();
        cargo = GetComponentsInChildren<PhantomCargo>();
        actuators = GetComponentsInChildren<PhantomActuator>();
        dragForms = GetComponentsInChildren<PhantomBody>();
        flightComputer = GetComponentInChildren<PhantomFlightComputer>();
        transmission = GetComponentInChildren<PhantomTransmission>();
        radar = GetComponentInChildren<PhantomRadar>();
        hardpoints = GetComponentInChildren<PhantomArmament>();
        esc = GetComponentInChildren<PhantomESC>();
        dials = GetComponentsInChildren<PhantomDial>();
        levers = GetComponentsInChildren<PhantomLever>();
        gunControl = GetComponentInChildren<PhantomGunControl>();


        mainCamera = Camera.main;
        basePosition = helicopter.transform.position;
        baseRotation = helicopter.transform.rotation;



        // --------------------- Confirm Components
        _checkPrerequisites();



        if (allOk)
        {

            // ------------------------- Setup Camera
            if (view != null)
            {
                view.helicopter = helicopter;
                view.controller = controller;
                view.InitializeCamera();
                if (view.exteriorObject != null) { interiorPilot = view.exteriorObject; }
            }
#if UNITY_EDITOR
            if (input != null) { helper.CheckInputCofig(false, controller); }
#endif

            // ------------------------- Setup Core
            if (core != null)
            {
                core.helicopter = helicopter;
                core.controller = controller;
                core.InitializeCore();
                initialDrag = helicopter.drag;
            }



            // ------------------------- Setup Gear
            if (gearHelper != null)
            {
                gearHelper.helicopter = helicopter;
                gearHelper.controller = controller;
                gearHelper.InitializeStruct();
            }



            // ------------------------- Setup Drag Body
            foreach (PhantomBody body in dragForms)
            {
                body.controller = controller;
                body.helicopter = helicopter;
                body.InitializeBody();
            }



            // --------------------- Setup Rotor System
            foreach (PhantomRotor rotor in rotors)
            {
                rotor.controller = controller;
                rotor.InitializeRotor();
                if (rotor.rotorType == PhantomRotor.RotorType.MainRotor) { forceRotors.Add(rotor); }
                if (rotor.rotorConfiguration == PhantomRotor.RotorConfiguration.Propeller && rotor.propellerMode == PhantomRotor.PropellerMode.Horizontal && flightComputer != null) { flightComputer.propeller = rotor; }
            }



            // ------------------------- Setup Actuators
            if (actuators != null)
            {
                foreach (PhantomActuator actuator in actuators)
                {
                    if (actuator.initialized) { Debug.LogWarning("Actuator for " + actuator.transform.name + " is still in evaluation mode."); }
                    else { actuator.InitializeActuator(); }

                    // ------------- Filter
                    if (actuator.actuatorType == PhantomActuator.ActuatorType.Canopy) { canopyActuator = actuator; }
                    if (actuator.actuatorType == PhantomActuator.ActuatorType.LandingGear) { gearActuator = actuator; }

                }
            }



            // ------------------------- Setup Transmission
            if (transmission != null)
            {
                transmission.helicopter = controller;
                foreach (PhantomRotor rotor in rotors)
                {
                    if (rotor.rotorType == PhantomRotor.RotorType.MainRotor && rotor.rotorConfiguration != PhantomRotor.RotorConfiguration.Propeller)
                    {
                        if (transmission.primaryRotor == null && !rotor.assigned) { transmission.primaryRotor = rotor; rotor.assigned = true; }
                        if (transmission.secondaryRotor == null && !rotor.assigned) { transmission.secondaryRotor = rotor; rotor.assigned = true; }
                    }
                    if (rotor.rotorType == PhantomRotor.RotorType.TailRotor && rotor.rotorConfiguration != PhantomRotor.RotorConfiguration.Propeller)
                    {
                        if (transmission.secondaryRotor == null && !rotor.assigned) { transmission.secondaryRotor = rotor; rotor.assigned = true; }
                    }
                    if(transmission.helicopterType == PhantomTransmission.HelicopterType.Compound)
                    {
                        if(rotor.rotorConfiguration == PhantomRotor.RotorConfiguration.Propeller && !rotor.assigned && transmission.appendageRotor == null) { transmission.appendageRotor = rotor;rotor.assigned = true; }
                    }
                }
                transmission.InitializeTransmission();
            }



            // --------------------------- Bulbs
            foreach (PhantomBulb bulb in lightBulbs) { bulb.InitializeBulb(); if (bulb.lightType == PhantomBulb.LightType.Landing && gearActuator != null) { gearActuator.landingBulbs.Add(bulb); } }




            // ------------------------- Setup Fuel
            if (engineType != EngineType.Electric)
            {
                if (fuelTanks.Length < 1)
                {
                    Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... fuel tank(s) not attached"); return;
                }
                foreach (PhantomFuelTank tank in fuelTanks) { tank.controller = controller; }
                fuelSystem.fuelTanks = fuelTanks;
                string tankFuelType = fuelTanks[0].fuelType.ToString();

                if (engineType == EngineType.Piston)
                {
                    if (tankFuelType != gasType.ToString()) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... fuel selected on controller (" + gasType.ToString() + ") not a match with tank fuel (" + tankFuelType + ")"); allOk = false; return; }
                }
                if (engineType == EngineType.Turboshaft)
                {
                    if (tankFuelType != jetType.ToString()) { Debug.LogError("Prerequisites not met on Helicopter " + transform.name + ".... fuel selected on controller (" + jetType.ToString() + ") not a match with loaded tank fuel (" + tankFuelType + ")"); allOk = false; return; }
                }

                fuelSystem.controller = controller;
                fuelSystem.InitializeDistributor();
            }




            // ------------------------- Setup Flight Computer
            if (flightComputer != null)
            {
                flightComputer.core = core;
                flightComputer.controller = controller;
                if (gearActuator != null) { flightComputer.gearSolver = gearActuator; }
                flightComputer.InitializeComputer();
            }



            // ------------------------- Radar
            if (radar != null)
            {
                radar.controller = controller;
                radar.InitializeRadar();
                if(gunControl != null)
                {
                    gunControl.connectedRadar = radar;
                    gunControl.InitializeGunControl();
                }
            }



            // ------------------------- Setup Aerofoils
            foreach (PhantomAerofoil foil in foils)
            {
                foil.controller = controller;
                foil.helicopter = helicopter;
                foil.coreSystem = core;
                foil.InitializeFoil();
            }




            // ------------------------- Setup Dials
            foreach (PhantomDial dial in dials)
            {
                dial.controller = controller;
                dial.dataLog = core;
                dial.InitializeDial();
            }



            // ------------------------- Setup Levers
            foreach (PhantomLever lever in levers)
            {
                lever.controller = controller;
                lever.InitializeLever();
            }



            // ------------------------- Setup Engines
            if (engineType == EngineType.Turboshaft && shafts != null) { foreach (PhantomTurboShaft engine in shafts) { engine.controller = controller; engine.computer = core; engine.helicopter = helicopter; engine.InitializeEngine(); } }
            if (engineType == EngineType.Piston && pistons != null) { foreach (PhantomPistonEngine engine in pistons) { engine.controller = controller; engine.computer = core; engine.helicopter = helicopter; engine.InitializeEngine(); } }
            if (engineType == EngineType.Electric && motors != null) { foreach (PhantomElectricMotor engine in motors) { engine.controller = controller; engine.InitializeMotor(); } }



            // --------------------------- Weapons
            if (hardpoints != null)
            {
                //STORE FOR REARMING
                GameObject armamentBox = hardpoints.gameObject;
                ArmamentsStorage = Instantiate(armamentBox, hardpoints.transform.position, hardpoints.transform.rotation, this.transform);
                ArmamentsStorage.SetActive(false); ArmamentsStorage.name = "Hardpoints(Storage)";
                if (radar != null)
                {
                    hardpoints.connectedRadar = radar;
                }
                hardpoints.controller = controller;
                hardpoints.InitializeWeapons();
            }




            // --------------------------------------------------------------------------------------- Fuel System
            if (engineType == EngineType.Piston)
            {
                if (gasType == GasType.AVGas100) { combustionEnergy = 42.8f; }
                if (gasType == GasType.AVGas100LL) { combustionEnergy = 43.5f; }
                if (gasType == GasType.AVGas82UL) { combustionEnergy = 43.6f; }
            }
            else
            {
                if (jetType == JetType.JetB) { combustionEnergy = 42.8f; }
                if (jetType == JetType.JetA1) { combustionEnergy = 43.5f; }
                if (jetType == JetType.JP6) { combustionEnergy = 43.02f; }
                if (jetType == JetType.JP8) { combustionEnergy = 43.28f; }
            }
            combustionEnergy *= 1000f;
        }



        // --------------------------- VR Levers
        if (throttleLever != null) { throttleLever.vehicle = helicopter.transform; throttleLever.InitializeLever(); }
        if (controlStick != null) { controlStick.vehicle = helicopter.transform; controlStick.InitializeLever(); }
        if (collectiveLever != null) { collectiveLever.vehicle = helicopter.transform; collectiveLever.InitializeLever(); }



        // --------------------------------------------------------------------------------------- Control State
        if (controlType == ControlType.Internal)
        {
            helper.InternalControlSetup(controller);
            SetControlState(false);

            // ------------------------ Disable Canvas
            if (canvasDisplay != null) { canvasDisplay.gameObject.SetActive(false); }
        }
        else
        {
            SetControlState(true);
            // ------------------------ Disable Main Camera
            if (mainCamera != null) { mainCamera.gameObject.SetActive(false); }
        }



        // ------------------------- Setup Quadcopter
        if (craftMode == CraftMode.Quadcopter)
        {
            if (esc != null)
            {
                esc.controller = controller;
                esc.InitializeESC();
            }
            input.controller = controller;
        }




        // ------------------------- Setup Input
        input.controller = controller;
#if UNITY_EDITOR
        helper.CheckInputCofig(true, controller);
#endif
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void FixedUpdate()
    {
        if(allOk && isControllable)
        {
            // ---------------------------- Safety NEt
            if (helicopter.velocity == Vector3.positiveInfinity) { helicopter.velocity = Vector3.zero; }
            if (helicopter.velocity == Vector3.negativeInfinity) { helicopter.velocity = Vector3.zero; }
            if (helicopter.angularVelocity == Vector3.positiveInfinity) { helicopter.angularVelocity = Vector3.zero; }
            if (helicopter.angularVelocity == Vector3.positiveInfinity) { helicopter.angularVelocity = Vector3.zero; }
            if (helicopter.velocity.magnitude > 500) { helicopter.velocity = Vector3.zero; helicopter.angularVelocity = Vector3.zero; }


            // ---------------------------- Fuel Usage
            if (engineType != EngineType.Electric) { fuelSystem.UpdateFuel(); }

            // ---------------------------- Force Update
            UpdateModel();
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Update()
    {
        if (allOk && isControllable)
        {
            input.UpdateInput();
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------FORCE MODEL-----------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------

    // ----------------------------------------------------- Velocities
    public Vector3 υw; public Vector3 ww;
    public Vector3 υl, wl;
    public Vector3 wind;

    // ----------------------------------------------------- Output
    public Vector3 centralForce;
    public Vector3 centralMoment;
    public Vector3 balanceMoment;


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void GetLocalVelocity(Transform component, out Vector3 υl, out Vector3 wl)
    {
        υw = helicopter.GetPointVelocity(component.position) + wind;
        ww = helicopter.angularVelocity;
        υl = helicopter.transform.InverseTransformDirection(υw);
        wl = -helicopter.transform.InverseTransformDirection(ww);
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void UpdateModel()
    {
        centralForce = new Vector3(); centralMoment = new Vector3(); balanceMoment = new Vector3();
        foreach (PhantomRotor rotor in controller.rotors) { if (rotor != null) { rotor.UpdateRotor(); } }
        foreach (PhantomRotor rotor in forceRotors)
        {
            if (rotor != null && rotor.gameObject.activeSelf)
            {
                centralForce += rotor.ThrustForce;
                if (controller.craftMode == PhantomController.CraftMode.Helicopter) { centralForce.y = 0f; }
                centralMoment += rotor.TorqueForce;
            }
        }
        helicopter.AddRelativeForce(centralForce, ForceMode.Force);
        helicopter.AddRelativeTorque(centralMoment, ForceMode.Force);

        // -------------------------------------------- Drag
        actuatorDrag = 0f;
        foreach (PhantomActuator actuator in actuators) { actuatorDrag += actuator.currentDragFactor; }
        currentDrag = (initialDrag + actuatorDrag); helicopter.drag = currentDrag;
    }
}
