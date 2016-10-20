using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

	//initialize defaults for gun parameters

	////////////////////////////////////
	//These are accessible in the UI
	// 	   [SerializeField] allows it 
	//     to be seen in the UI
	////////////////////////////////////

	//The max number of bullets in a mag
	[SerializeField] private int magazineSize = 30;

	// The damage done by a single bullet
	[SerializeField] private int damage = 10;

	// the effective range of the current gun
	[SerializeField] private int range = 20;

	// the amount of ammo currently not in the mag of the gun
	[SerializeField] private int stockpile = 5000;

	// the amount of bullets in the current gun
	[SerializeField] private int currentMagAmount = 30;

	//the amount of time between shots in milliseconds
	[SerializeField] private float shootTime = 100.0f;

	//the type of shooting style 0 is full and 1 is semi
	[SerializeField] private int shootType = 0;

	// the time it takes to reload the gun in milliseconds
	[SerializeField] private float reloadTime = 1000f;

	// flag to tell if the gun needs to be reloaded
	[SerializeField] private bool needsReload = false;

	// muzzle flash
	[SerializeField] private ParticleSystem muzzleFlash;

	// the prefab for bullet impacts
	[SerializeField] private GameObject impactPrefab;

	// the max number of impacts we can have
	[SerializeField] private int maxImpacts;

	// Camera of player to use for aiming
	[SerializeField] private Camera gunCamera;

	////////////////////////////////////
	//These are NOT accessible in the UI
	////////////////////////////////////

	// the last time the gun was fired
	private float lastTimeFired = 0;

	// the last time the gun was reloaded
	private float lastReloadTime = 0;

	// list of all impacts
	private GameObject[] impacts;

	// number of impact we have currently
	private int currentImpact = 0;

	// holds if we are shooting or not
	private bool shooting = false;

	// the reference to our animations controller
	private Animator anim;

	// Use this for initialization
	void Start () {
		//init current magazine to full
		this.currentMagAmount = this.magazineSize;

		this.impacts = new GameObject[this.maxImpacts];

		//create all the impacts each impact will 
		//just change the location and visibility when used
		for (int i = 0; i < this.maxImpacts; i++) {
			impacts [i] = (GameObject)Instantiate (impactPrefab);
		}

		//get the animator
		anim = GetComponentInChildren<Animator> ();
	}

	void Update() {
		CheckShooting ();
	}

	// Update is called once per frame
	void FixedUpdate () {
		HandleShooting ();
		HandleReloading ();
	}

	/**
	 * Checks if the user wants to shoot
	 */
	public void CheckShooting() {
		bool isAutoShooting = (shootType == 0 && Input.GetButton ("Fire1"));
		bool isSemiShooting = (shootType == 1 && Input.GetButtonDown ("Fire1"));

		//check if we are trying to shoot and we are not currently reloading or sprinting
		if ((isAutoShooting || isSemiShooting) && !this.isReloading () && !this.anim.GetBool("Sprint")) {
			shooting = true;
		}
	}

	/**
	 * Handles the shooting of the gun
	 */
	public void HandleShooting(){

		if(this.isShooting()) {
			//set shooting to false
			shooting = false;

			//check if we can fire another bullet based off of how fast the gun shoots
			float currentMilli = Time.time * 1000;
			if (currentMilli - this.lastTimeFired > this.shootTime) {
				this.lastTimeFired = currentMilli;

				//check if we have any ammo to shoot
				if (this.currentMagAmount > 0) {
					anim.SetTrigger ("Fire"); //play the shooting animation
					muzzleFlash.Play();		  //show muzzleFlash
					this.currentMagAmount--;  //shoot the bullet 

					//shows a line in the scene view of where the bullet is shooting
					Debug.DrawRay (this.gunCamera.transform.position, this.gunCamera.transform.forward * range, Color.green);

					RaycastHit entity;
					//check to see if we hit an object
					if (Physics.Raycast (this.gunCamera.transform.position, this.gunCamera.transform.forward, out entity, range)) {
						//we hit something
						if (entity.collider.CompareTag ("Enemy")) {
							//we hit an enemy
							Debug.Log ("Shot Enemy");
						} else { 
							//we hit something else so place an impact
							impacts[currentImpact].transform.position = entity.point;
							//impacts [currentImpact].GetComponent<ParticleSystem> ().Play ();

							currentImpact = (currentImpact + 1) % maxImpacts;
							Debug.Log ("Missed Enemy hit: " + entity.collider.tag);
						}
					} else { 
						//we didn't hit anything
						Debug.Log ("Missed anything");
					}
				} else {
					//needs reload make empty sound or whatever
					Debug.Log("Needs Reload");
				}
			}
		} if (this.isReloading ()) {
			Debug.Log ("Can't Shoot When Reloading");
		}
	}

	public void HandleReloading(){
		if ((this.canReload() && Input.GetButtonDown("Reload")) || this.isReloading()) {
			float currentTime = Time.time * 1000; //convert to milliseconds
			if (this.lastReloadTime == -1) {
				this.lastReloadTime = currentTime; //set reload time to now
				this.anim.SetBool("Reload", true);
			} else if (currentTime - this.lastReloadTime >= this.reloadTime) { //time is up actually reload the gun now
				this.reload();
			}
		}
	}

	public bool canReload() {
		return (this.needsReload || this.currentMagAmount < this.magazineSize);
	}

	public bool isReloading() {
		return this.lastReloadTime != -1;
	}

	public bool isShooting() {
		return this.shooting;
	}

	public void reload(){
		int neededBullets = this.magazineSize - this.currentMagAmount;
		if (this.stockpile >= neededBullets) {
			this.stockpile -= neededBullets;
			this.currentMagAmount += neededBullets;
		} else {
			this.currentMagAmount += this.stockpile;
			this.stockpile = 0;
		}
		this.needsReload = false;
		this.lastReloadTime = -1;
		this.anim.SetBool("Reload", false);
	}
}
