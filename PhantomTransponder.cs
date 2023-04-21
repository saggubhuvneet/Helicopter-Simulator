using UnityEngine;



public class PhantomTransponder : MonoBehaviour
{
	public enum SilantroTag { Helicopter, Truck, Airport, Missile, Undefined, Tank }
	public SilantroTag silantroTag = SilantroTag.Undefined;
	public Texture2D silantroTexture;
	public string AssignedID = "Default";
	public string TrackingID = "Default";
	public bool isTracked;
	public bool isLockedOn;
	public float radarSignature = 1f;
}