using UnityEngine;
using System.Collections;





public class PhantomExtension : MonoBehaviour
{
	public enum Function { ImpactSound, CaseSound, SystemReset, CleanUp, Starter }
	public Function function = Function.ImpactSound;


	// --------------------------------------------------------- Variables
	public PhantomController controller;
	private AudioSource componentSource;
	public AudioClip[] sounds;

	public float soundRange = 300f;
	public float soundVolume = 0.4f;
	public int soundCount = 1;
	public float destroyTime = 5f;
	public bool contact;





	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	void Start()
	{
		if (function == Function.CleanUp || function == Function.CaseSound)
		{
			Destroy(gameObject, destroyTime);
		}
		if (function == Function.Starter)
		{
			StartCoroutine(StartUpHelicopter());
		}
	}



	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//SETUP
	IEnumerator StartUpHelicopter()
	{
		yield return new WaitForSeconds(0.001f);//JUST LAG A BIT BEHIND CONTROLLER SCRIPT
												//STARTUP AIRCRAFT	
		controller.StartAircraft();
		//RAISE GEAR
		if (controller.gearActuator != null) { controller.gearActuator.DisengageActuator(); }
		//TURN ON LIGHTS
		controller.input.ToggleLightState();
	}


	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//CASE IMPACT SOUND
	void OnCollisionEnter(Collision col)
	{
		if (function == Function.CaseSound)
		{
			if (col.collider.tag == "Ground")
			{
				componentSource = gameObject.AddComponent<AudioSource>();
				componentSource.dopplerLevel = 0f;
				componentSource.spatialBlend = 1f;
				componentSource.rolloffMode = AudioRolloffMode.Custom;
				componentSource.maxDistance = soundRange;
				componentSource.volume = soundVolume;
				componentSource.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
			}
		}
		if (function == Function.CleanUp)
		{
			if (contact)
			{
				Destroy(gameObject);
			}
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//IMPACT EFFECT SOUND
	void OnStart()
	{
		if (function == Function.ImpactSound)
		{
			componentSource = gameObject.AddComponent<AudioSource>();
			componentSource.dopplerLevel = 0f;
			componentSource.spatialBlend = 1f;
			componentSource.rolloffMode = AudioRolloffMode.Custom;
			componentSource.maxDistance = soundRange;
			componentSource.volume = soundVolume;
			componentSource.PlayOneShot(sounds[Random.Range(0, sounds.Length)]);
		}
	}

	// ----------------------------------------------------------------------------------------------------------------------------------------------------------
	//RESET SCENE
	public void ResetScene()
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(this.gameObject.scene.name);
	}
}
