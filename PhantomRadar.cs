using Oyedoyin;
using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif




[RequireComponent(typeof(PhantomTransponder))]
public class PhantomRadar : MonoBehaviour
{


	// ---------------------------------------- Selectibles
	public enum RadarType { Civilian, Military }
	public RadarType radarType = RadarType.Civilian;
	public enum ObjectFilter { All, Ground, Air }
	public ObjectFilter objectFilter = ObjectFilter.All;
	public enum RadarMode { Standalone, Connected }
	public RadarMode radarMode = RadarMode.Standalone;




	// ---------------------------------------- Connections
	public PhantomController controller;
	public PhantomTransponder transponder;
	private GameObject radarCore;
	public PhantomRadar supportRadar;





	[System.Serializable]
	public class FilteredObject
	{
		public string name;
		public GameObject body;
		public string form;
		public float speed;
		public float altitude;
		public float distance;
		public float heading;
		public string trackingID;
		public float ETA;
		public PhantomTransponder transponder;
	}

	public List<FilteredObject> filteredObjects = new List<FilteredObject>();
	public List<FilteredObject> airTargets = new List<FilteredObject>();
	public List<FilteredObject> groundTargets = new List<FilteredObject>();
	public Collider[] visibleObjects;
	public List<GameObject> processedObjects;
	public float baseSpeed;


	// ---------------------------------------- Variables
	public float range = 1000f;
	public float minimumWeaponsRange = 5000;
	public float criticalSignature = 0.5f;
	public float pingRate = 5;
	public float actualPingRate;
	public float pingTime;
	public LayerMask checkLayers = ~0;
	public bool displayBounds;
	public string TrackerID;




	// ---------------------------------------- Display
	public float size = 250f;
	[Range(0, 1f)] public float Transparency = 0.9f;
	public Texture background;
	public Texture compass;
	float scale;
	public bool rotate;
	public Color generalColor = Color.white;
	public Color labelColor = Color.white;
	public GUISkin radarSkin;
	GUIStyle labelStyle = new GUIStyle();
	private Vector2 radarPosition;


	// ---------------------------------------- Target Display
	public Camera currentCamera;
	public Camera lockedTargetCamera;
	public Camera targetCamera;
	public float cameraDistance = 40f;
	public float cameraHeight = 30f;
	Vector3 lockedCameraPosition;


	// ---------------------------------------- Target Variables
	public bool targetLocked, markTargets;
	public FilteredObject lockedTarget;
	public Vector3 lockedPosition;
	public Texture2D selectedTargetTexture, lockedTargetTexture;
	public float objectScale = 1;
	public int targetSelection;
	public FilteredObject currentTarget;
	public Texture2D TargetLockOnTexture;
	public Texture2D TargetLockedTexture;



	private string header;
	private string qualifier;
	private const string headerContainer = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
	private const string qualifierContainer = "1234567890";
	string objectname, objectID, objectForm; float objectSpeed, objectAltitude, objectHeading, objectDistance;



	// --------------------------------------------TARGET SELECTION--------------------------------------------------------------------------------------------------------------
	//--------------------------------------------------------------------------------------------------------------------------------------------------------
	public void SelectedUpperTarget() { targetSelection++; }//SELECT TARGET ABOVE CURRENT TARGET
	public void SelectLowerTarget() { targetSelection--; }//SELECT TAREGT BELOW CURRENT TARGET
	public void SelectTargetAtPosition(int position) { targetSelection = position; }//SELECT TARGET AT A PARTICULAR POSITION




	//-------------------------------------LOCK ONTO A TARGET--------------------------------------------------------------------------------
	public void LockSelectedTarget()
	{
		//SET TARGET PROPERTIES
		if (radarType == RadarType.Military && allOk)
		{
			lockedTarget = currentTarget;
			targetLocked = true;
			lockedTarget.transponder.isLockedOn = true;
			if (controller != null && controller.gunControl != null) { controller.gunControl.lockedTarget = currentTarget.body.transform; }
		}
	}


	//---------------------------- RELEASE TARGET LOCK
	public void ReleaseLockedTarget()
	{
		if (radarType == RadarType.Military && allOk)
		{
			if (lockedTarget != null && lockedTarget.transponder != null) { lockedTarget.transponder.isLockedOn = false; }
			targetLocked = false;
			lockedTarget = null;
			if(controller != null && controller.gunControl != null) { controller.gunControl.lockedTarget = null; }
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//GENERATE TRACKING ID
	public string GenerateTrackID()
	{
		header = string.Empty; qualifier = string.Empty;
		for (int i = 0; i < 3; i++) { header += "ABCDEFGHIJKLMNOPQRSTUVWXYZ"[UnityEngine.Random.Range(0, "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Length)]; }
		for (int j = 0; j < 2; j++) { qualifier += "1234567890"[UnityEngine.Random.Range(0, "1234567890".Length)]; }
		return header + qualifier;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	bool allOk;
	protected void _checkPrerequisites()
	{
		//CHECK COMPONENTS
		if (controller != null && transponder != null)
		{
			allOk = true;
		}
		else if (transponder == null)
		{
			Debug.LogError("Prerequisites not met on Radar " + transform.name + "....Aircraft transponder not assigned");
			allOk = false;
			return;
		}
		else if (controller == null)
		{
			Debug.LogError("Prerequisites not met on Radar " + transform.name + "....Aircraft controller not assigned");
			allOk = false;
			return;
		}
	}






	//-------------------------------------------------------------------------//SETUP RADAR---------------------------------------------------------------------------------------------
	public void InitializeRadar()
	{
		transponder = GetComponent<PhantomTransponder>();


		_checkPrerequisites();


		if (allOk)
		{
			radarPosition = Vector2.zero;
			if (pingRate < 0.1f) { pingRate = 1f; }
			actualPingRate = 1f / pingRate; pingTime = 0f;

			radarCore = new GameObject("Radar Core");
			radarCore.transform.parent = this.transform;
			radarCore.transform.localPosition = Vector3.zero;

			//GENERATE ID
			TrackerID = GenerateTrackID();
			//SEND DATA TO AIRCRAFT
			if (controller != null)
			{
				if (transponder != null) { transponder.TrackingID = TrackerID; }
				else { Debug.Log("Please attach a transponder to radar gameObject"); }
			}
		}
	}






	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//REFRESH
	void Update()
	{
		if (allOk && controller != null && controller.isControllable)
		{
			//-------------------------------------SPIN CORE
			radarCore.transform.Rotate(new Vector3(0f, this.pingRate * 100f * Time.deltaTime, 0f));
			scale = range / 100f; if (controller != null && controller.view != null)
			{ currentCamera = controller.view.currentCamera; }


			//-------------------------------------SEND PING 
			pingTime += Time.deltaTime;
			if (pingTime >= actualPingRate) { Ping(); }

			//-------------------------------------SEND MILITARY RADAR DATA
			if (radarType == RadarType.Military)
			{
				CombatRadarSystem();
			}

			//--------------------------------------POSITION TARGET CAMERA
			PositionCamera();

			baseSpeed = controller.core.forwardSpeed;
		}
	}







	Collider[] filterPool;
	Collider filterCollider;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//PING
	private void Ping()
	{
		pingTime = 0f;

		// ------------------------- Search
		visibleObjects = Physics.OverlapSphere(transform.position, range, checkLayers);

		// ----------------------- Clear Containers
		processedObjects.Clear();
		filteredObjects.Clear();
		groundTargets.Clear();
		airTargets.Clear();


		if (filterPool != visibleObjects)
		{

			// ------------------------- Filter
			for (int i = 0; i < visibleObjects.Length; i++)
			{
				filterCollider = visibleObjects[i];
				//Avoid Self detection
				if (!filterCollider.transform.IsChildOf(controller.transform))
				{
					//SEPARATE OBJECTS
					PhantomTransponder transponder;
					//CHECK PARENT BODY
					transponder = filterCollider.gameObject.GetComponent<PhantomTransponder>();
					if (transponder == null) { transponder = filterCollider.gameObject.GetComponentInChildren<PhantomTransponder>(); }


					// ----------------------------- Process Object
					if (transponder != null)
					{
						// Check Range
						float objectDistance = Vector3.Distance(filterCollider.gameObject.transform.position, transform.position);

						if (transponder.radarSignature > criticalSignature && objectDistance < range)
						{
							processedObjects.Add(filterCollider.gameObject);

							//SET VARIABLES
							if (!transponder.isTracked)
							{
								transponder.isTracked = true;
								transponder.TrackingID = TrackerID;
								transponder.AssignedID = GenerateTrackID();
							}

							//FILTER
							Rigidbody filterbody = filterCollider.gameObject.GetComponent<Rigidbody>();
							//COLLECT VARIABLES
							if (filterbody != null)
							{
								objectSpeed = filterbody.velocity.magnitude;
							}
							else { objectSpeed = 0f; }

							objectname = filterCollider.transform.name;
							objectForm = transponder.silantroTag.ToString();
							if (transponder.AssignedID != null) { objectID = transponder.AssignedID; } else { objectID = "Undefined"; }
							objectAltitude = filterCollider.gameObject.transform.position.y;
							objectHeading = filterCollider.gameObject.transform.eulerAngles.y;
							objectDistance = Vector3.Distance(filterCollider.gameObject.transform.position, this.transform.position);
							//STORE VARIABLES
							FilteredObject currentObject = new FilteredObject();
							currentObject.body = filterCollider.gameObject;
							currentObject.name = objectname;
							currentObject.altitude = objectAltitude;
							currentObject.form = objectForm;
							currentObject.speed = objectSpeed;
							currentObject.trackingID = objectID;
							currentObject.heading = objectHeading;
							currentObject.distance = objectDistance;
							currentObject.transponder = transponder;
							currentObject.ETA = currentObject.distance / (baseSpeed + 1f);
							//ADD TO BUCKET
							filteredObjects.Add(currentObject);

							//FILTER INTO SELECTIBLES
							if (currentObject.form == "Aircraft") { airTargets.Add(currentObject); }
							if (currentObject.form == "SAM Battery" || currentObject.form == "Truck" || currentObject.form == "Tank") { groundTargets.Add(currentObject); }
						}
					}
				}
			}

			filterPool = visibleObjects;
		}

		//REFRESH LOCKED TARGET
		if (lockedTarget != null && targetLocked)
		{
			//FILTER
			Rigidbody filterbody = lockedTarget.body.gameObject.GetComponent<Rigidbody>();
			//COLLECT VARIABLES
			if (filterbody != null) { lockedTarget.speed = filterbody.velocity.magnitude; }
			lockedTarget.altitude = lockedTarget.body.transform.position.y;
			lockedTarget.distance = Vector3.Distance(lockedTarget.body.transform.position, this.transform.position);
			lockedPosition = lockedTarget.body.transform.position;
			lockedTarget.ETA = lockedTarget.distance / (baseSpeed + 1f);

			//UNLOCK TARGET IF OUT OF RANGE
			if (lockedTarget.distance > range) { ReleaseLockedTarget(); }
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void CombatRadarSystem()
	{
		//SELECT CLOSEST TARGET
		if (lockedTarget == null && currentTarget == null) { targetSelection++; }

		//1. ALL TARGETS
		if (objectFilter == ObjectFilter.All)
		{
			if (targetSelection < 0) { targetSelection = filteredObjects.Count - 1; }
			if (targetSelection > filteredObjects.Count - 1) { targetSelection = 0; }
			//SELECT CURRENT TARGET
			if (filteredObjects.Count > 0 && filteredObjects != null)
			{
				currentTarget = filteredObjects[targetSelection];
				if (currentTarget == null)
				{
					filteredObjects.Remove(currentTarget);
				}
			}
		}

		//2. GROUND TARGETS
		if (objectFilter == ObjectFilter.Ground)
		{
			if (targetSelection < 0) { targetSelection = groundTargets.Count - 1; }
			if (targetSelection > groundTargets.Count - 1) { targetSelection = 0; }
			//SELECT CURRENT TARGET
			if (groundTargets.Count > 0 && groundTargets != null)
			{
				currentTarget = groundTargets[targetSelection];
				if (currentTarget == null)
				{
					filteredObjects.Remove(currentTarget);
					groundTargets.Remove(currentTarget);
				}
			}
		}

		//2. GROUND TARGETS
		if (objectFilter == ObjectFilter.Air)
		{
			if (targetSelection < 0) { targetSelection = airTargets.Count - 1; }
			if (targetSelection > airTargets.Count - 1) { targetSelection = 0; }
			//SELECT CURRENT TARGET
			if (airTargets.Count > 0 && airTargets != null)
			{
				currentTarget = airTargets[targetSelection];
				if (currentTarget == null)
				{
					filteredObjects.Remove(currentTarget);
					airTargets.Remove(currentTarget);
				}
			}
		}



		//RELEASE TARGET IF DESTROYED
		if (targetLocked && lockedTarget.transponder == null)
		{
			ReleaseLockedTarget();
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private void OnGUI()
	{
		if (controller == null || !controller.isControllable) { return; }


		//SET SKIN
		if (radarSkin != null) { GUI.skin = radarSkin; }


		//DRAW LOCKED TARGET
		if (lockedTarget != null && lockedTarget.body != null)
		{
			//DRAW CAMERA LOCK INDICATOR
			if (currentCamera)
			{
				Vector3 dir = (lockedTarget.body.transform.position - currentCamera.transform.position).normalized;
				float direction = Vector3.Dot(dir, currentCamera.transform.forward);
				if (direction > 0.5f)
				{
					Vector3 screenPos = currentCamera.WorldToScreenPoint(lockedTarget.body.transform.position);
					if (TargetLockedTexture) GUI.DrawTexture(new Rect(screenPos.x - TargetLockedTexture.width / 2, Screen.height - screenPos.y - TargetLockedTexture.height / 2, TargetLockedTexture.width, TargetLockedTexture.height), TargetLockedTexture);
					//DISPLAY TARGET PROPERTIES
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y - 20, 250, 50), lockedTarget.name);
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y, 200, 50), lockedTarget.trackingID);
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 20, 200, 50), "SPD:" + lockedTarget.speed.ToString("0.0") + " knts");
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 40, 200, 50), "ALT: " + (lockedTarget.altitude * MathBase.toFt).ToString("0.0") + " ft");
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 60, 200, 50), "DST: " + (lockedTarget.distance * MathBase.toFt).ToString("0.0") + " ft");
					GUI.Label(new Rect(screenPos.x + 40, Screen.height - screenPos.y + 80, 200, 50), "ETA: " + lockedTarget.ETA.ToString("0.0") + " s");
				}
			}
		}


		//DRAW NORMAL RADAR
		GUI.color = new Color(generalColor.r, generalColor.g, generalColor.b, Transparency);
		if (rotate)
		{
			GUIUtility.RotateAroundPivot(-base.transform.eulerAngles.y, radarPosition + new Vector2(size / 2f, size / 2f));
		}


		//DRAW IDENTIFIED OBJECTS
		foreach (FilteredObject selectedObject in filteredObjects)
		{
			if (selectedObject != null && selectedObject.body != null && selectedObject.transponder != null)
			{
				//COLLECT TARGET DATA
				Vector2 position = GetPosition(selectedObject.body.transform.position);
				Texture2D radarTexture = selectedObject.transponder.silantroTexture;
				string targetID = selectedObject.transponder.AssignedID;
				float superScale = objectScale / selectedObject.transponder.radarSignature;

				//DRAW ON SCREEN
				if (radarTexture != null)
				{
					GUI.DrawTexture(new Rect(position.x - (float)radarTexture.width / superScale / 2f, position.y + (float)radarTexture.height / superScale / 3f, (float)radarTexture.width / superScale,
						(float)radarTexture.height / superScale), radarTexture);
				}

				//CHOOSE LABEL COLOR
				labelStyle.normal.textColor = new Color(labelColor.r, labelColor.g, labelColor.b, Transparency);

				//DRAW LABEL
				if (radarTexture)
				{
					GUI.Label(new Rect(position.x - (float)radarTexture.width / objectScale / 2f, position.y - (float)radarTexture.height / superScale / 2f, 50f / superScale, 40f / superScale), targetID, labelStyle);
				}
				//DRAW CAMERA INDICATOR
				if (currentCamera)
				{
					Vector3 dir = (selectedObject.body.transform.position - currentCamera.transform.position).normalized;
					float direction = Vector3.Dot(dir, currentCamera.transform.forward);
					if (direction > 0.5f)
					{
						Vector3 screenPos = currentCamera.WorldToScreenPoint(selectedObject.body.transform.position);
						//float distance = Vector3.Distance (transform.position, selectedObject.body.transform.position);
						if (TargetLockOnTexture)
							GUI.DrawTexture(new Rect(screenPos.x - TargetLockOnTexture.width / 2, Screen.height - screenPos.y - TargetLockOnTexture.height / 2, TargetLockOnTexture.width, TargetLockOnTexture.height), TargetLockOnTexture);
					}
				}
			}
		}

		//DRAW LOCKED TARGET
		if (filteredObjects.Count > 0 && lockedTarget != null && lockedTarget.body != null)
		{
			//COLLECT DATA
			Vector2 currentposition = GetPosition(lockedTarget.body.transform.position);
			GUI.DrawTexture(new Rect(currentposition.x - (float)lockedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)lockedTargetTexture.height / 2.5f / 3f,
				(float)lockedTargetTexture.width / 2.5f, (float)lockedTargetTexture.height / 2.5f), lockedTargetTexture);
		}


		//DRAW SELECTED TARGET
		if (filteredObjects.Count > 0 && currentTarget != null && currentTarget.body != null && currentTarget.transponder != null)
		{
			//COLLECT DATA
			Vector2 currentposition = GetPosition(currentTarget.body.transform.position);
			GUI.DrawTexture(new Rect(currentposition.x - (float)selectedTargetTexture.width / 2.5f / 2f, currentposition.y + (float)selectedTargetTexture.height / 2.5f / 3f,
			(float)selectedTargetTexture.width / 2.5f, (float)selectedTargetTexture.height / 2.5f), selectedTargetTexture);
		}


		//DRAW BACKGROUND
		if (background) { GUI.DrawTexture(new Rect(radarPosition.x, radarPosition.y, size, size), background); }
		GUIUtility.RotateAroundPivot(base.transform.eulerAngles.y, radarPosition + new Vector2(size / 2f, size / 2f));
		//DRAW RADAR COMPASS
		if (compass)
		{ GUI.DrawTexture(new Rect(radarPosition.x + size / 2f - (float)compass.width / 2f, radarPosition.y + size / 2f - (float)compass.height / 2f, (float)compass.width, (float)compass.height), compass); }
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//DRAW EXTENTS
#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Handles.color = new Color(1, 0, 0, 0.1f);
		if (radarCore != null && displayBounds)
		{
			Handles.DrawSolidArc(radarCore.transform.position, radarCore.transform.up, -radarCore.transform.right, 120f, range);//Horizontal View
			Handles.DrawSolidArc(radarCore.transform.position, radarCore.transform.right, radarCore.transform.forward, 60f, range);//VERTICAL VIEW TOP
			Handles.DrawSolidArc(radarCore.transform.position, -radarCore.transform.right, radarCore.transform.forward, 60f, range);//VERTICAL VIEW BOTTOM
		}


		if (markTargets)
		{
			//-------------------------------------------DRAW LINE TO TARGETS
			Handles.color = Color.white;
			foreach (PhantomRadar.FilteredObject filteredObject in filteredObjects)
			{
				if (filteredObject != null && filteredObject != lockedTarget && filteredObject != currentTarget && filteredObject.body != null)
				{
					Handles.DrawLine(filteredObject.body.transform.position, this.transform.position);
				}
			}
			//------------------------------------------- DRAW LINE TO CURRENT TARGET
			if (currentTarget != null && currentTarget.body != null && currentTarget.transponder != null)
			{
				Handles.color = Color.green;
				Handles.DrawLine(currentTarget.body.transform.position, this.transform.position);
			}
			//--------------------------------------------DRAW LINE TO LOCKED TARGET
			if (lockedTarget != null && lockedTarget.body != null)
			{
				Handles.color = Color.red;
				Handles.DrawLine(lockedTarget.body.transform.position, this.transform.position);
			}
		}
	}
#endif




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//GET 2D POSITION
	private Vector2 GetPosition(Vector3 position)
	{
		Vector2 cronus = Vector2.zero;
		if (controller)
		{
			cronus.x = radarPosition.x + (position.x - transform.position.x + size * scale / 2f) / scale;
			cronus.y = radarPosition.y + (-(position.z - transform.position.z) + size * scale / 2f) / scale;
		}
		return cronus;
	}





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void PositionCamera()
	{
		//TARGET CAMERA
		if (filteredObjects.Count > 0 && currentTarget != null && currentTarget.body != null)
		{
			float x = currentTarget.body.transform.position.x;
			float y = currentTarget.body.transform.position.y + cameraHeight;
			float z = currentTarget.body.transform.position.z + cameraDistance;
			Vector3 cameraPosition = new Vector3(x, y, z);
			//
			if (targetCamera != null)
			{
				targetCamera.transform.position = cameraPosition;
				targetCamera.transform.LookAt(currentTarget.body.transform.position);
			}
		}
		else
		{
			if (targetCamera != null && controller.view != null)
			{
				targetCamera.transform.position = controller.view.currentCamera.transform.position;
				targetCamera.transform.rotation = controller.view.currentCamera.transform.rotation;
			}
		}
		//
		if (lockedTarget != null && lockedTarget.body != null)
		{
			float xy = lockedTarget.body.transform.position.x;
			float yy = lockedTarget.body.transform.position.y + cameraHeight;
			float zy = lockedTarget.body.transform.position.z + cameraDistance;

			lockedCameraPosition = new Vector3(xy, yy, zy);
			if (lockedTargetCamera != null)
			{
				lockedTargetCamera.transform.position = lockedCameraPosition;
				if (lockedTarget != null && lockedTarget.body != null)
				{
					lockedTargetCamera.transform.LookAt(lockedTarget.body.transform.position);
				}
			}
		}
	}
}
