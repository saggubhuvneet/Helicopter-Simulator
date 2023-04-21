using System.Collections.Generic;
using UnityEngine;
using System;using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PhantomBody : MonoBehaviour {
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float maximumDiameter = 20f;
	public int resolution = 10;
	GameObject point;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	[Serializable]
	public class SectionPoint
	{
		public Transform sectionTransform;
		public float sectionDiameterPercentage = 10;
		public float sectionHeightPercentage = 10;
		public List<Vector3> sectionPointList;
		public float height, width;
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public List<SectionPoint> sectionPoints;
	public float totalArea,aircraftLength;
	public float frontalArea, rearArea, topArea, bottomArea, leftArea, rightArea;
	public float skinDragCoefficient;public enum SurfaceFinish{SmoothPaint,PolishedMetal,ProductionSheetMetal,MoldedComposite,PaintedAluminium}
	public SurfaceFinish surfaceFinish = SurfaceFinish.PaintedAluminium;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public PhantomController controller;
	public Rigidbody helicopter;
	public float airspeed =10f,k,knotSpeed,totalDrag,RE;


    private void Start()
    {
		InitializeBody();
    }

    bool allOk;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void InitializeBody()
	{

		//----------------------------
		_checkPrerequisites ();


		if(allOk){
			//SET FINISH FACTOR
			if (surfaceFinish == SurfaceFinish.MoldedComposite) {k = 0.17f;}
			if (surfaceFinish == SurfaceFinish.PaintedAluminium) {k = 3.33f;}
			if (surfaceFinish == SurfaceFinish.PolishedMetal) {k = 0.50f;}
			if (surfaceFinish == SurfaceFinish.ProductionSheetMetal) {k = 1.33f;}
			if (surfaceFinish == SurfaceFinish.SmoothPaint) {k = 2.08f;}
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	protected void _checkPrerequisites() {
		//CHECK COMPONENTS
		if (controller != null && helicopter != null) {
			allOk = true;
		} else if (helicopter == null) {
			Debug.LogError("Prerequisites not met on " + transform.name + "....Aircraft rigidbody not assigned");
			allOk = false;
		}
		else if (controller == null) {
			Debug.LogError("Prerequisites not met on " + transform.name + "....controller not assigned");
			allOk = false;
		}
	}

	public float verticalDrag;
	public float lateralDrag;
	public float longitudinalDrag;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void FixedUpdate()
	{
		if (controller != null)
		{
			airspeed = helicopter.velocity.magnitude; knotSpeed = airspeed * 1.944f;
			Vector3 dragForce = -helicopter.velocity; dragForce.Normalize();

			if (airspeed > 0)
			{
				skinDragCoefficient = EstimateSkinDragCoefficient(airspeed);
				totalDrag = 0.5f * controller.core.airDensity * airspeed * airspeed * totalArea * skinDragCoefficient;
			}

			dragForce *= totalDrag; 
			if (totalDrag > 0) { helicopter.AddForce(dragForce, ForceMode.Force); }


			Vector3 relativeVelocity = transform.InverseTransformDirection(helicopter.velocity + controller.baseWind);

			longitudinalDrag = 0.5f * 0.04f * 1.225f * frontalArea * relativeVelocity.z * relativeVelocity.z;
			lateralDrag = 0.5f * 0.42f * 1.225f * rightArea * relativeVelocity.x * relativeVelocity.z;
			verticalDrag = 0.5f * 0.42f * 1.225f * topArea * relativeVelocity.y * relativeVelocity.z;

			helicopter.AddForce(-transform.forward * longitudinalDrag);
			helicopter.AddForce(-transform.right * lateralDrag);
			helicopter.AddForce(-transform.up * verticalDrag);

		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateRe(float inputSpeed)
	{
		float Re1 = (controller.core.airDensity * inputSpeed * aircraftLength) / controller.core.viscocity;float Re2;
		if (controller.core.machSpeed < 0.9f) {Re2 = 38.21f * Mathf.Pow (((aircraftLength *3.28f)/ (k/100000)), 1.053f);} 
		else {Re2 = 44.62f * Mathf.Pow (((aircraftLength*3.28f) / (k/100000)), 1.053f) * Mathf.Pow (controller.core.machSpeed, 1.16f);}
		float superRe = Mathf.Min (Re1, Re2);RE = superRe;return superRe;
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public float EstimateSkinDragCoefficient(float velocity)
	{
		float Recr = EstimateRe (velocity);
		float baseCf = frictionDragCurve.Evaluate (Recr) / 1000f;
		
		//WRAPPING CORRECTION
		float Cfa = baseCf*(0.0025f*(aircraftLength/maximumDiameter)*Mathf.Pow(Recr,-0.2f));
		//SUPERVELOCITY CORRECTION
		float Cfb = baseCf * Mathf.Pow ((maximumDiameter / aircraftLength), 1.5f);
		//PRESSURE CORRECTION
		float Cfc = baseCf*7*Mathf.Pow((maximumDiameter/aircraftLength),3f);
		float actualCf = 1.03f * (baseCf + Cfa + Cfb + Cfc);
		return actualCf;
	}


	public AnimationCurve frictionDragCurve;
	public List<float> sectionAreas = new List<float>();
	public float maximumCrossArea,maximumSectionDiameter,finenessRatio;public int layer;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void ProcessBodyShape()
    {
		frictionDragCurve = new AnimationCurve();
		frictionDragCurve.AddKey(new Keyframe(1000000000, 1.5f));
		frictionDragCurve.AddKey(new Keyframe(100000000, 2.0f));
		frictionDragCurve.AddKey(new Keyframe(10000000, 2.85f));
		frictionDragCurve.AddKey(new Keyframe(1000000, 4.1f));
		frictionDragCurve.AddKey(new Keyframe(100000, 7.0f));


		if (sectionPoints != null && sectionPoints.Count > 0)
		{
			for (int a = 0; a < sectionPoints.Count; a++)
			{
				if (sectionPoints[a].sectionTransform == null)
				{
					sectionPoints.Remove(sectionPoints[a]);
				}
			}


			sectionAreas = new List<float>();
			for (int i = 0; i < sectionPoints.Count - 1; i++)
			{
				//AREA
				float sectionWidth = sectionPoints[i].width; float sectionHeight = sectionPoints[i].height;
				float area = 3.142f * sectionHeight * sectionWidth; sectionAreas.Add(area); maximumCrossArea = sectionAreas.Max();
				layer = sectionAreas.IndexOf(maximumCrossArea);
			}

			//EQUIVALENT DIAMETER
			float sectionWidthM = sectionPoints[layer].width;
			float sectionHeightM = sectionPoints[layer].height;
			float perimeter = 6.284f * Mathf.Pow((0.5f * (Mathf.Pow(sectionHeightM, 2) + Mathf.Pow(sectionWidthM, 2))), 0.5f);
			maximumSectionDiameter = (1.55f * Mathf.Pow(maximumCrossArea, 0.625f)) / Mathf.Pow(perimeter, 0.25f);


			// ----------------------- Reset Area
			topArea = 0f;
			frontalArea = 0f;
			rearArea = 0f;
			rightArea = 0f;
			leftArea = 0f;
			bottomArea = 0f;



			foreach (SectionPoint position in sectionPoints)
			{

				if (position.sectionTransform != null)
				{
					position.height = (maximumDiameter * position.sectionHeightPercentage * 0.01f) / 2;
					position.width = (maximumDiameter * position.sectionDiameterPercentage * 0.01f) / 2;
					//DrawEllipse(position.sectionTransform, position.width, position.height, out position.sectionPointList);
				}
			}

#if UNITY_EDITOR
			if (sectionPoints.Count > 0)
			{
				for (int a = 0; a < sectionPoints.Count - 1; a++)
				{
					float FA = 0, BA = 0, TA = 0, BBA = 0, LA = 0, RA = 0;float UA;

					//Display
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out UA, bodyFace, true);
					//1. Front
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out FA, BodyFace.Front, false); frontalArea += FA;
					//2. Back
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out BA, BodyFace.Back, false); rearArea += BA;
					//3. Left
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out LA, BodyFace.Left, false); leftArea += LA;
					//4. Right
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out RA, BodyFace.Right, false); rightArea += RA;
					//5. Top
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out TA, BodyFace.Top, false); topArea += TA;
					//6. Bottom
					DrawConnection(sectionPoints[a].sectionPointList, sectionPoints[a + 1].sectionPointList, out BBA, BodyFace.Bottom, false); bottomArea += BBA;
				}
				aircraftLength = Vector3.Distance(sectionPoints[0].sectionTransform.position, sectionPoints[sectionPoints.Count - 1].sectionTransform.position);
				float noseArea = 3.142f * sectionPoints[0].height * sectionPoints[0].width;
				frontalArea += noseArea; bottomArea += noseArea;
			}
			finenessRatio = aircraftLength / maximumSectionDiameter;
#endif
		}
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
	void OnDrawGizmosSelected()
	{
        if (!Application.isPlaying) { ProcessBodyShape(); }
	}




	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawEllipse(Transform positionTransform,float a,float c,out List<Vector3> outList)
	{
		outList = new List<Vector3> ();
		Quaternion q1 = Quaternion.AngleAxis (90, positionTransform.right);
		for (float i = 0; i < 2 * Mathf.PI; i += 2 * Mathf.PI / resolution) {
			var newPoint = positionTransform.position+(q1*positionTransform.rotation * (new Vector3 (a * Mathf.Cos (i), 0, c* Mathf.Sin (i))));
			var lastPoint = positionTransform.position+(q1*positionTransform.rotation * (new Vector3 (a * Mathf.Cos (i + 2 * Mathf.PI / resolution), 0, c * Mathf.Sin (i + 2 * Mathf.PI / resolution))));
			Handles.DrawLine (newPoint, lastPoint);
			Gizmos.color = Color.red;outList.Add (newPoint);
			Gizmos.DrawSphere (newPoint, 0.02f);
		}
	}

	public enum BodyFace { Top, Bottom, Left, Right, Front, Back}
	public BodyFace bodyFace = BodyFace.Front;


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void DrawConnection(List<Vector3> pointsA,List<Vector3> pointsB,out float area, BodyFace face, bool draw)
	{
		area = 0f;float sectionArea;
		int A = 0;
		int B = 0;

		if (face == BodyFace.Back || face == BodyFace.Front) { A = 0; B = pointsA.Count- 1; }
		if (face == BodyFace.Bottom) { A = 0;B = pointsA.Count / 2; }
		if (face == BodyFace.Top) { A = pointsA.Count / 2; B = pointsA.Count-1; }
		if (face == BodyFace.Left) { A = pointsA.Count / 4; B = (pointsA.Count*3)/4; }

		if(face == BodyFace.Right)
        {
			for (int i = 0; i < pointsA.Count / 4; i++)
			{
				if (draw)
				{
#if UNITY_EDITOR
					Handles.color = new Color(1, 0, 0, 0.3f);
					Handles.DrawLine(pointsA[i], pointsB[i]);
					Handles.DrawAAConvexPolygon(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
#endif
				}
				sectionArea = EstimatePanelSectionArea(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
				area += sectionArea; 
			}

			for (int i = (pointsA.Count * 3) / 4; i < pointsA.Count - 1; i++)
			{
				if (draw)
				{
#if UNITY_EDITOR
					Handles.color = new Color(1, 0, 0, 0.3f);
					Handles.DrawLine(pointsA[i], pointsB[i]);
					Handles.DrawAAConvexPolygon(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
#endif
				}
				sectionArea = EstimatePanelSectionArea(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
				area += sectionArea;
			}
#if UNITY_EDITOR
			if (draw) { Handles.DrawAAConvexPolygon(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]); }
#endif
			float closeArea = EstimatePanelSectionArea(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]);
			area += closeArea;
		}
        else {

			if (pointsA.Count == pointsB.Count && pointsA.Count > 0)
			{
				for (int i = A; i < B; i++)
				{
					if (draw)
					{
						Handles.color = new Color(1, 0, 0, 0.3f);
						Handles.DrawLine(pointsA[i], pointsB[i]);
						Handles.DrawAAConvexPolygon(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
					}
					sectionArea = EstimatePanelSectionArea(pointsA[i], pointsA[i + 1], pointsB[i + 1], pointsB[i]);
					area += sectionArea; 
				}

				//DRAW FROM END BACK TO START
				if (face == BodyFace.Front || face == BodyFace.Back || face == BodyFace.Top)
				{
					if (draw) { Handles.DrawAAConvexPolygon(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]); }
					float closeArea = EstimatePanelSectionArea(pointsA[pointsA.Count - 1], pointsA[0], pointsB[0], pointsB[pointsA.Count - 1]);
					area += closeArea;
				}
			}
		}
	}
	#endif


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddElement()
	{
		if (sectionPoints != null) {point = new GameObject ("Section " + (sectionPoints.Count + 1));} 
		if(sectionPoints == null){sectionPoints = new List<SectionPoint> ();point = new GameObject ("Section 1");}
		point.transform.parent = this.transform;point.transform.localPosition = Vector3.zero;
		if (sectionPoints != null && sectionPoints.Count > 1) {
			Vector3 predisessorPosition = sectionPoints[sectionPoints.Count-1].sectionTransform.localPosition;
			point.transform.localPosition = new Vector3 (predisessorPosition.x, predisessorPosition.y, predisessorPosition.z - 0.5f);
		}
		SectionPoint dragPoint = new SectionPoint ();
		dragPoint.sectionTransform = point.transform;
		sectionPoints.Add (dragPoint);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public void AddSupplimentElement(float zFloat)
	{
		if (sectionPoints != null) {point = new GameObject ("Section " + (sectionPoints.Count + 1));} 
		point.transform.parent = this.transform;point.transform.localPosition = new Vector3(0,0,zFloat);
		SectionPoint dragPoint = new SectionPoint ();dragPoint.sectionTransform = point.transform;sectionPoints.Add (dragPoint);
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	private float EstimatePanelSectionArea(Vector3 panelLeadingLeft, Vector3 panelLeadingRight, Vector3 panelTrailingLeft, Vector3 panelTrailingRight)
	{
		//BUILD TRAPEZOID VARIABLES
		float panelArea,panelLeadingEdge,panelTipEdge,panalTrailingEdge,paneRootEdge,panelDiagonal;
		//SOLVE
		panelLeadingEdge = (panelTrailingLeft - panelLeadingLeft).magnitude;panelTipEdge = (panelTrailingRight - panelTrailingLeft).magnitude;panalTrailingEdge = (panelLeadingRight - panelTrailingRight).magnitude;paneRootEdge = (panelLeadingLeft - panelLeadingRight).magnitude;
		panelDiagonal = 0.5f * (panelLeadingEdge + panelTipEdge + panalTrailingEdge + paneRootEdge);
		panelArea = Mathf.Sqrt(((panelDiagonal-panelLeadingEdge) * (panelDiagonal-panelTipEdge) * (panelDiagonal-panalTrailingEdge) * (panelDiagonal-paneRootEdge)));
		return panelArea;
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CALCULATE FACTOR POSITION
	public Vector3 EstimateSectionPosition(Vector3 lhs,Vector3 rhs,float factor){Vector3 estimatedPosition = lhs + ((rhs - lhs) * factor);return estimatedPosition;}
}



#if UNITY_EDITOR
[CustomEditor(typeof(PhantomBody))]
public class PhantomBodyEditor: Editor
{

	private static GUIContent deleteButton = new GUIContent("Remove","Delete");
	private static GUILayoutOption buttonWidth = GUILayout.Width (60f);

	Color backgroundColor;Color silantroColor = new Color(1,0.4f,0);
	PhantomBody body;int listSize;
	SerializedObject bodyObject;SerializedProperty sectionList;
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void OnEnable()
	{
		body = (PhantomBody)target;
		bodyObject = new SerializedObject (body);
		sectionList = bodyObject.FindProperty ("sectionPoints");
	}
	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	public override void OnInspectorGUI()
	{
		backgroundColor = GUI.backgroundColor;DrawDefaultInspector ();
		EditorGUI.BeginChangeCheck();bodyObject.UpdateIfRequiredOrScript();

		EditorGUILayout.HelpBox ("Note: Still in the testing phase, please report any bugs or issues you run into :))", MessageType.Warning);
		GUILayout.Space(1f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Section Configuration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(5f);
		body.maximumDiameter = EditorGUILayout.FloatField ("Maximum Diameter", body.maximumDiameter);
		GUILayout.Space(5f);
		body.resolution = EditorGUILayout.IntSlider ("Resolution", body.resolution, 5, 50);
		GUILayout.Space(5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Sections", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		if (sectionList != null) {
			EditorGUILayout.LabelField ("Section Count", sectionList.arraySize.ToString ());
		}

		GUILayout.Space (5f);
		if (GUILayout.Button ("Create Section")) {body.AddElement ();}

		if (sectionList != null) {
			GUILayout.Space (2f);
			for (int i = 0; i < sectionList.arraySize; i++) {
				SerializedProperty reference = sectionList.GetArrayElementAtIndex (i);
				SerializedProperty widthPercentage = reference.FindPropertyRelative ("sectionDiameterPercentage");
				SerializedProperty heightPercentage = reference.FindPropertyRelative ("sectionHeightPercentage");
				SerializedProperty sectionHeight = reference.FindPropertyRelative ("height");
				SerializedProperty sectionWidth = reference.FindPropertyRelative ("width");

				GUI.color = new Color (1, 0.8f, 0);
				EditorGUILayout.HelpBox ("Section : " + (i + 1).ToString (), MessageType.None);
				GUI.color = backgroundColor;
				GUILayout.Space (3f);
				widthPercentage.floatValue = EditorGUILayout.Slider ("Width Percentage",widthPercentage.floatValue, 1f, 100f);
				GUILayout.Space (3f);
				heightPercentage.floatValue = EditorGUILayout.Slider ("Height Percentage",heightPercentage.floatValue, 1f, 100f);
				GUILayout.Space (3f);
				EditorGUILayout.LabelField ("Section Width", sectionWidth.floatValue.ToString ("0.00") + " m" );
				GUILayout.Space (1f);
				EditorGUILayout.LabelField ("Section Height", sectionHeight.floatValue.ToString ("0.00") + " m");

				GUILayout.Space (3f);
				if (GUILayout.Button (deleteButton, EditorStyles.miniButtonRight, buttonWidth)) {
					Transform trf = body.sectionPoints [i].sectionTransform;
					body.sectionPoints.RemoveAt (i);
					if (trf != null) {
						DestroyImmediate (trf.gameObject);
					}
				}
				GUILayout.Space (5f);
			}
		}

		GUILayout.Space (8f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Dimensions", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField ("Total Length", body.aircraftLength.ToString ("0.00") + " m");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Equivalent Diameter", body.maximumSectionDiameter.ToString ("0.00") + " m");


		GUILayout.Space (5f);
		GUI.color = Color.white;
		EditorGUILayout.HelpBox ("Flow Consideration", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space (3f);
		body.surfaceFinish = (PhantomBody.SurfaceFinish)EditorGUILayout.EnumPopup("Surface Finish",body.surfaceFinish);
		GUILayout.Space (5f);
		EditorGUILayout.LabelField ("Wetted Area", body.totalArea.ToString ("0.000") + " m2");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Sector Area", body.maximumCrossArea.ToString ("0.000") + " m2");
		GUILayout.Space (3f);
		EditorGUILayout.LabelField ("Fineness Ratio", body.finenessRatio.ToString ("0.00"));

		GUILayout.Space(20f);
		GUI.color = silantroColor;
		EditorGUILayout.HelpBox ("Output Data", MessageType.None);
		GUI.color = backgroundColor;
		GUILayout.Space(1f);
		EditorGUILayout.LabelField ("Friction Coefficient", body.skinDragCoefficient.ToString ("0.00000"));
		GUILayout.Space(2f);
		EditorGUILayout.LabelField ("Drag", body.totalDrag.ToString ("0.0") + " N");


		if (EditorGUI.EndChangeCheck ()) {Undo.RegisterCompleteObjectUndo (bodyObject.targetObject, "Section Change");}
		if (GUI.changed) {
			EditorUtility.SetDirty (body);
		}
		bodyObject.ApplyModifiedProperties();
	}

	//----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
	public override bool RequiresConstantRepaint ()
	{
		return true;
	}
}
#endif