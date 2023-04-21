#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Oyedoyin;



/// <summary>
///
/// 
/// Use:		 Handles the movement/rotation of control levers and handles
/// </summary>



public class PhantomLever : MonoBehaviour
{

    // ------------------------------------- Selectibles
    public enum LeverType { Stick, Collective, Pedal, GearIndicator, Throttle }
    public LeverType leverType = LeverType.Stick;
    public enum StickType { Joystick, Yoke }
    public StickType stickType = StickType.Joystick;
    public enum PedalType { Sliding, Hinged }
    public PedalType pedalType = PedalType.Hinged;
    public enum PedalMode { Individual, Combined }
    public PedalMode pedalMode = PedalMode.Combined;

    public enum RotationDirection { CW, CCW }
    public RotationDirection direction = RotationDirection.CW;
    public RotationDirection pitchDirection = RotationDirection.CW;
    public RotationDirection rollDirection = RotationDirection.CW;
    public RotationDirection rightDirection = RotationDirection.CW;
    public RotationDirection leftDirection = RotationDirection.CCW;

    public enum RotationAxis { X, Y, Z }
    public RotationAxis rotationAxis = RotationAxis.X;
    public RotationAxis rudderRotationAxis = RotationAxis.X;
    public RotationAxis rollRotationAxis = RotationAxis.X;
    public RotationAxis pitchRotationAxis = RotationAxis.X;
    public RotationAxis rightRotationAxis = RotationAxis.X;
    public RotationAxis leftRotationAxis = RotationAxis.X;


    // ------------------------------------- Connections
    public PhantomController controller;
    public Transform lever, yoke;
    public Material bulbMaterial;
    public float maximumBulbEmission = 10f;
    private Color baseColor, finalColor;
    public Transform leftPedal, rightPedal;


    // ------------------------------------- Vectors
    Vector3 axisRotation;
    Vector3 pitchAxisRotation;
    Vector3 rollAxisRotation;
    Vector3 rightAxisRotation, leftAxisRotation;
    Vector3 initialRightPosition, initialLeftPosition;


    // ------------------------------------- Quaternions
    Quaternion initialYokeRotation;
    Quaternion InitialRotation = Quaternion.identity;
    Quaternion initialRightRotation;
    Quaternion initialLeftRotation;



    // ------------------------------------- Variables
    public float maximumDeflection;
    public float MaximumPitchDeflection;
    public float MaximumRollDeflection;
    public float rudderInput;
    public float maximumPedalDeflection = 20f;
    public float maximumSlidingDistance = 10f;
    public float currentPedalDeflection;
    public float currentDistance;
    public float collectiveAmount, throttleAmount;
    public float currentGearRotation;





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeLever()
    {
        // ---------------------------------------- Setup Rotation Variables
        axisRotation = Handler.EstimateModelProperties(direction.ToString(), rotationAxis.ToString());
        if (lever != null) {InitialRotation = lever.localRotation; }
        if (stickType == StickType.Yoke && yoke != null) { initialYokeRotation = yoke.localRotation; }

        pitchAxisRotation = Handler.EstimateModelProperties(pitchDirection.ToString(), pitchRotationAxis.ToString());
        rollAxisRotation = Handler.EstimateModelProperties(rollDirection.ToString(), rollRotationAxis.ToString());

        if (leverType == LeverType.Pedal)
        {
            initialLeftRotation = leftPedal.localRotation; initialRightRotation = rightPedal.localRotation;
            initialLeftPosition = leftPedal.localPosition; initialRightPosition = rightPedal.localPosition;
            rightAxisRotation = Handler.EstimateModelProperties(rightDirection.ToString(), rightRotationAxis.ToString());
            leftAxisRotation = Handler.EstimateModelProperties(leftDirection.ToString(), leftRotationAxis.ToString());
        }
    }








    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void Update()
    {
        if (controller != null)
        {

            // ---------------------------------------- Control Stick
            if (leverType == LeverType.Stick)
            {
                float pitch = controller.flightComputer.processedPitch * MaximumPitchDeflection;
                float roll = controller.flightComputer.processedRoll * MaximumRollDeflection;
                var rollEffect = Quaternion.AngleAxis(roll, rollAxisRotation);
                var pitchEffect = Quaternion.AngleAxis(pitch, pitchAxisRotation);

                // ----------------- Apply
                if (stickType == StickType.Joystick) { var angleEffect = rollEffect * pitchEffect; lever.localRotation = InitialRotation * angleEffect; }
                else { lever.localRotation = InitialRotation * pitchEffect; yoke.localRotation = initialYokeRotation * rollEffect; }
            }



            // ---------------------------------------- Throttle
            if (leverType == LeverType.Throttle)
            {
                throttleAmount = controller.flightComputer.processedThrottle;
                throttleAmount = Mathf.Clamp(throttleAmount, 0, 1.0f);
                throttleAmount *= maximumDeflection;
                lever.localRotation = InitialRotation;
                lever.Rotate(axisRotation, throttleAmount);
            }



            // ---------------------------------------- Collective
            if (leverType == LeverType.Collective)
            {
                //COLELCT THROTTLE INPUT
                collectiveAmount = controller.flightComputer.processedCollective;
                collectiveAmount = Mathf.Clamp(collectiveAmount, 0, 1.0f);
                collectiveAmount *= maximumDeflection; lever.localRotation = InitialRotation;
                lever.Rotate(axisRotation, collectiveAmount);
            }




            // ---------------------------------------- Gear
            if (leverType == LeverType.GearIndicator)
            {
                if (controller.gearState == PhantomController.GearState.Open) { currentGearRotation = Mathf.Lerp(currentGearRotation, 0, Time.deltaTime * 2f); }
                else { currentGearRotation = Mathf.Lerp(currentGearRotation, maximumDeflection, Time.deltaTime * 2f); }

                if (lever != null)
                { lever.transform.localRotation = InitialRotation; lever.transform.Rotate(axisRotation, currentGearRotation); }

                if (bulbMaterial != null)
                {
                    if (controller.gearState == PhantomController.GearState.Open) { bulbMaterial.color = Color.green; baseColor = Color.green; }
                    else { bulbMaterial.color = Color.red; baseColor = Color.red; }
                    // ------------------- Set
                    finalColor = baseColor * Mathf.LinearToGammaSpace(maximumBulbEmission);
                    if (bulbMaterial != null) { bulbMaterial.SetColor("_EmissionColor", finalColor); }
                }
            }





            // ---------------------------------------- Pedals
            if (leverType == LeverType.Pedal)
            {
                rudderInput = controller.flightComputer.processedYaw;

                // ---------------------- ROTATE
                if (pedalType == PedalType.Hinged)
                {
                    currentPedalDeflection = rudderInput * maximumDeflection;

                    // ---------------------- ROTATE PEDALS
                    rightPedal.localRotation = initialRightRotation;
                    rightPedal.Rotate(rightAxisRotation, currentPedalDeflection);
                    if (pedalMode == PedalMode.Combined)
                    {
                        leftPedal.localRotation = initialLeftRotation;
                        leftPedal.Rotate(leftAxisRotation, currentPedalDeflection);
                    }
                    else
                    {
                        leftPedal.localRotation = initialLeftRotation;
                        leftPedal.Rotate(leftAxisRotation, -currentPedalDeflection);
                    }
                }

                // ---------------------- MOVE
                if (pedalType == PedalType.Sliding)
                {
                    currentDistance = rudderInput * (maximumSlidingDistance / 100);
                    //MOVE PEDALS
                    rightPedal.localPosition = initialRightPosition;
                    rightPedal.localPosition += rightAxisRotation * currentDistance;
                    leftPedal.localPosition = initialLeftPosition;
                    leftPedal.localPosition += leftAxisRotation * currentDistance;
                }
            }
        }
    }
}