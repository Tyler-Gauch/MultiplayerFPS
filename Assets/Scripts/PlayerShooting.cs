using UnityEngine;
using System.Collections;

public class PlayerShooting : MonoBehaviour {

	//initialize defaults for gun parameters

	//The max number of bullets in a mag
	public int magazineSize = 30;

	// The damage done by a single bullet
	public int damage = 10;

	// the effective range of the current gun
	public int range = 20;

	// the amount of ammo currently not in the mag of the gun
	public int stockpile = 5000;

	// the amount of bullets in the current gun
	public int currentMagAmount = 30;

	//the amount of time between shots in milliseconds
	public float shootTime = 100.0f;

	//the type of shooting style 0 is full and 1 is semi
	public int shootType = 0;

	// the time it takes to reload the gun in milliseconds
	public float reloadTime = 1000f;

	// flag to tell if the gun needs to be reloaded
	public bool needsReload = false;

	// the last time the gun was fired
	private float lastTimeFired = 0;

	// the last time the gun was reloaded
	private float lastReloadTime = 0;

	// Use this for initialization
	void Start () {
		this.currentMagAmount = magazineSize;
	}
	
	// Update is called once per frame
	void Update () {
		HandleShooting ();
		HandleReloading ();
	}

	/**
	 * Handles the shooting of the gun
	 */
	public void HandleShooting(){
		if (((shootType == 0 && Input.GetButton ("Fire1")) || (shootType == 1 && Input.GetButtonDown ("Fire1"))) && !this.isReloading()) {
			float currentMilli = Time.time * 1000;
			if (currentMilli - this.lastTimeFired > this.shootTime) {
				this.lastTimeFired = currentMilli;
				if (this.currentMagAmount > 0) {
					this.currentMagAmount--;
					Debug.DrawRay (Camera.main.transform.position, Camera.main.transform.forward * range, Color.green);
					RaycastHit entity;
					if (Physics.Raycast (Camera.main.transform.position, Camera.main.transform.forward, out entity, range)) {
						if (entity.collider.CompareTag ("Enemy")) {
							Debug.Log ("Shot Enemy");
						} else {
							Debug.Log ("Missed Enemy hit: " + entity.collider.tag);
						}
					} else {
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
				//possibly start reload animation here
				Debug.Log("Reloading...");
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

	public void reload(){
		Debug.Log("Reloaded!");
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
	}
}
