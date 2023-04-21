using Oyedoyin;
using UnityEngine;



/// <summary>
///
/// 
/// Use:		 Handles the movement and rotation of the aircraft cameras
/// </summary>
/// 


public class PhantomCamera : MonoBehaviour
{
    //---------------------------------------------- Selectibles
    public enum CameraType { Helicopter, Player }
    public CameraType cameraType = CameraType.Helicopter;
    public enum CameraState { Interior, Exterior }
    public CameraState cameraState = CameraState.Exterior;
    public enum CameraStartState { Interior, Exterior }
    public CameraStartState startState = CameraStartState.Exterior;
    public enum CameraFocus { Normal, VR }
    public CameraFocus cameraFocus = CameraFocus.Normal;
    public enum CameraAttachment { None, ExteriorOnly, Dual }
    public CameraAttachment attachment = CameraAttachment.None;
    public GameObject exteriorObject, interiorObject;




    //---------------------------------------------- Connections
    public Rigidbody helicopter;
    public PhantomController controller;
    public Camera normalExterior;
    public Camera normalInterior;
    public Transform vrExterior;
    public Transform vrInterior;
    public Transform focusPoint;
    public Camera currentCamera;
    Camera[] sceneCameras;



    //------------------------------------------ Free Camera
    public float azimuthSensitivity = 3;
    public float elevationSensitivity = 3;
    public float radiusSensitivity = 10;
    private float azimuth, elevation;
    public float radius;
    public float maximumRadius = 20f;
    Vector3 filterPosition; float filerY; Vector3 filteredPosition;
    Vector3 cameraDirection;
    public float maximumInteriorVolume = 0.8f;



    //------------------------------------------ Interior Camera
    public float viewSensitivity = 80f;
    public float maximumViewAngle = 80f;
    float verticalRotation, horizontalRotation;
    Vector3 baseCameraPosition;
    Quaternion baseCameraRotation, currentCameraRotation;
    public GameObject pilotObject;
    public float mouseSensitivity = 100.0f; public float clampAngle = 80.0f;
    public float scrollValue;

    public float zoomSensitivity = 3;
    public bool zoomEnabled = true;
    public float maximumFOV = 20, currentFOV, baseFOV;



    //------------------------------------------Orbit Camera
    public float orbitDistance = 20.0f;
    public float orbitHeight = 2.0f;
    //private bool FirstClick = false;
    private float orbitAngle = 180.0f;
    Vector3 cameraRange, cameraPosition;
    //Vector3 startPosition;
  



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ToggleCamera()
    {
        if (cameraState == CameraState.Exterior) { ActivateInteriorCamera(); }
        else { ActivateExteriorCamera(); }
    }


    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ActivateExteriorCamera()
    {

        if (cameraFocus == CameraFocus.Normal)
        {
            // ------------------ Normal Interior Camera
            if (normalInterior != null)
            {
                normalInterior.enabled = false;
                AudioListener interiorListener = normalInterior.GetComponent<AudioListener>();
                if (interiorListener != null) { interiorListener.enabled = false; }
            }


            // ------------------ Normal Exterior Camera
            normalExterior.enabled = true; currentCamera = normalExterior;
            AudioListener exteriorListener = normalExterior.GetComponent<AudioListener>();
            if (exteriorListener != null) { exteriorListener.enabled = true; }
        }

        if (attachment != CameraAttachment.None)
        {
            if (exteriorObject != null && !exteriorObject.activeSelf) { exteriorObject.SetActive(true); }
            if (interiorObject != null && interiorObject.activeSelf) { interiorObject.SetActive(false); }
        }


        // ------------------ Pilot
        if (pilotObject != null && !pilotObject.activeSelf) { pilotObject.SetActive(true); }
        cameraState = CameraState.Exterior;
        if (controller != null) { controller.cameraState = CameraState.Exterior; }
    }

    
    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ActivateInteriorCamera()
    {
        if (cameraFocus == CameraFocus.Normal)
        {
            // ------------------ Normal Interior Camera
            if (normalInterior != null)
            {
                normalInterior.enabled = true;
                AudioListener interiorListener = normalInterior.GetComponent<AudioListener>();
                if (interiorListener != null) { interiorListener.enabled = true; }
            }
            else { Debug.Log("Interior Camera has not been setup"); return; }


            // ------------------ Normal Exterior Camera
            normalExterior.enabled = false; currentCamera = normalInterior;
            AudioListener exteriorListener = normalExterior.GetComponent<AudioListener>();
            if (exteriorListener != null) { exteriorListener.enabled = false; }
        }


        if (attachment != CameraAttachment.None)
        {
            if (exteriorObject != null && exteriorObject.activeSelf) { exteriorObject.SetActive(false); }
            if (interiorObject != null && !interiorObject.activeSelf) { interiorObject.SetActive(true); }
        }


        // ------------------ Pilot
        if (pilotObject != null && pilotObject.activeSelf) { pilotObject.SetActive(false); }
        cameraState = CameraState.Interior;
        if (controller != null) { controller.cameraState = CameraState.Interior; }
    }



    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        if (cameraType == CameraType.Player)
        {
            sceneCameras = Camera.allCameras;
            foreach (Camera cam in sceneCameras)
            {
                cam.enabled = false;
                AudioListener Listener = cam.GetComponent<AudioListener>(); if (Listener != null) { Listener.enabled = false; }
            }

            normalExterior.enabled = true; AudioListener exteriorListener = normalExterior.GetComponent<AudioListener>(); if (exteriorListener != null) { exteriorListener.enabled = true; }
            if (normalExterior == null) { Debug.LogError("Prerequisites not met on Camera " + transform.name + "....Exterior camera not assigned"); allOk = false; return; }
            else { allOk = true; }
            if (focusPoint == null) { focusPoint = this.transform; }
        }
    }




    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public bool allOk;
    protected void _checkPrerequisites()
    {

        if (focusPoint == null) { focusPoint = this.transform; }

        if (cameraType == CameraType.Helicopter)
        {
            if (cameraFocus == CameraFocus.Normal)
            {
                if (normalExterior == null) { Debug.LogError("Prerequisites not met on Camera " + transform.name + "....Exterior camera not assigned"); allOk = false; return; }
                else { allOk = true; }
            }
            if (cameraFocus == CameraFocus.VR)
            {
                if (vrExterior == null) { Debug.LogError("Prerequisites not met on Camera " + transform.name + "....Exterior VR camera holder not assigned"); allOk = false; return; }
                else { allOk = true; }
            }
        }
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void InitializeCamera()
    {

        //CHECK COMPONENTS
        _checkPrerequisites();


        if (allOk)
        {


            if (cameraType == CameraType.Helicopter)
            {
                if (normalExterior != null && normalExterior.GetComponent<AudioListener>() == null) { normalExterior.gameObject.AddComponent<AudioListener>(); }
                if (normalInterior != null && normalInterior.GetComponent<AudioListener>() == null) { normalInterior.gameObject.AddComponent<AudioListener>(); }

                //ResetCameras();
                if (normalInterior != null)
                {
                    baseCameraPosition = normalInterior.transform.localPosition;
                    baseCameraRotation = normalInterior.transform.localRotation;
                    baseFOV = normalInterior.fieldOfView;
                }

                if (controller != null && controller.controlType == PhantomController.ControlType.External)
                {
                    // ------------------------ Start Mode
                    if (startState == CameraStartState.Exterior) { ActivateExteriorCamera(); }
                    else { ActivateInteriorCamera(); }
                }
            }
        }
    }





    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    public void ResetCameras()
    {
        if (normalExterior != null)
        {
            normalExterior.enabled = false;
            if (normalExterior.GetComponent<AudioListener>() != null) { normalExterior.GetComponent<AudioListener>().enabled = false; }
        }

        if (normalInterior != null)
        {
            normalInterior.enabled = false;
            if (normalInterior.GetComponent<AudioListener>() != null) { normalInterior.GetComponent<AudioListener>().enabled = false; }
        }
    }





    //---------------------------------------------------------------------------------
    private void Update()
    {
        if (allOk)
        {
            if(cameraType == CameraType.Helicopter)
            {
                if(controller != null && controller.isControllable)
                {
                    if(cameraState == CameraState.Exterior) { FreeCamera(); }
                    else { InteriorSystem(); }
                }
            }

            if (cameraType == CameraType.Player)
            {
                //SEND PLAYER ORBIT DATA
                PlayerSystem();
            }
        }
    }







    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void InteriorSystem()
    {

        if (cameraFocus == CameraFocus.Normal)
        {
            if (normalInterior != null && Input.GetMouseButton(0))
            {
                if (Application.isFocused)
                {
                    verticalRotation += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
                    horizontalRotation += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
                }

                //CLAMP ANGLES (You can make them independent to have a different maximum for each)
                horizontalRotation = Mathf.Clamp(horizontalRotation, -clampAngle, clampAngle);
                verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);
                //ASSIGN ROTATION
                currentCameraRotation = Quaternion.Euler(horizontalRotation, verticalRotation, 0.0f);
                normalInterior.transform.localRotation = currentCameraRotation;
            }

            //ZOOM
            if (zoomEnabled)
            {
                currentFOV = normalInterior.fieldOfView;
                currentFOV += Input.GetAxis("Mouse ScrollWheel") * zoomSensitivity;
                currentFOV = Mathf.Clamp(currentFOV, maximumFOV, baseFOV);
                normalInterior.fieldOfView = currentFOV;
            }
        }
    }






    // ----------------------------------------------------------------------------------------------------------------------------------------------------------
    void PlayerSystem()
    {
        //CALCULATE CAMERA ANGLE
        cameraRange = focusPoint.transform.forward;
        cameraRange.y = 0f; cameraRange.Normalize();
        cameraRange = Quaternion.Euler(0, orbitAngle, 0) * cameraRange;

        //CALCULATE CAMERA POSITION
        cameraPosition = focusPoint.transform.position;
        cameraPosition += cameraRange * orbitDistance;
        cameraPosition += focusPoint.up * orbitHeight;

        //APPLY TO CAMERA
        normalExterior.transform.position = cameraPosition;
        normalExterior.transform.LookAt(focusPoint.transform.position);
    }






    //------------------------------------------------------------------------------------------
    void FreeCamera()
    {
        if (Input.GetMouseButton(0) && Application.isFocused)
        {
            azimuth -= Input.GetAxis("Mouse X") * azimuthSensitivity * Time.deltaTime;
            elevation -= Input.GetAxis("Mouse Y") * elevationSensitivity * Time.deltaTime;
        }


        //CALCULATE DIRECTION AND POSITION
        MathBase.SphericalToCartesian(radius, azimuth, elevation, out cameraDirection);
        //CLAMP ROTATION IF AIRCRAFT IS ON THE GROUND//LESS THAN radius meters
        if (focusPoint.position.y < maximumRadius)
        {
            filterPosition = focusPoint.position + cameraDirection;
            filerY = filterPosition.y;
            if (filerY < 2) filerY = 2;
            filteredPosition = new Vector3(filterPosition.x, filerY, filterPosition.z);
            normalExterior.transform.position = filteredPosition;
        }
        else
        {
            normalExterior.transform.position = focusPoint.position + cameraDirection; ;
        }


        //POSITION CAMERA
        normalExterior.transform.LookAt(focusPoint);
        radius = maximumRadius;//use this to control the distance from the aircraft
    }









    //---------------------------------------------------------------------------------
    public float CalculateCameraAngle(Transform referencePoint)
    {
        //--------------ESTIMATE SECTOR
        float angle = normalExterior.transform.localEulerAngles.y - 90;//(Mathf.Atan2(targetDirection.z, targetDirection.x) * Mathf.Rad2Deg);
        if (angle < 0) { angle += 360f; }

        return angle;
    }
}
