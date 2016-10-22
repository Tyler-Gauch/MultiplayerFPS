using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour {

	public UnityStandardAssets.Characters.FirstPerson.FirstPersonController fpsController;

	public Camera fpsCamera;

	public Camera gunCamera;

	public PlayerShooting shootingScript;

	public GameObject gun;

	public float respawnTime = 2f;

	private Renderer[] renderers;

	private void Start() {
		this.renderers = GetComponentsInChildren<Renderer> ();
	}

	public override void OnStartLocalPlayer() {
	
		fpsController.enabled = true;
		fpsCamera.enabled = true;
		gunCamera.enabled = true;
		shootingScript.enabled = true;

		gun.layer = 8;

		foreach (Transform child in gun.transform.GetComponentsInChildren<Transform> ()) {
			child.gameObject.layer = 8;
		}

		gameObject.name = "LOCAL Player";

		base.OnStartLocalPlayer ();
	}

	public void ToggleRenderer(bool isAlive) {
		for(int i = 0; i < renderers.Length; i++) {
			renderers[i].enabled = isAlive;
		}
	}

	public void ToggleControls(bool isAlive) {
		fpsController.enabled = isAlive;
		shootingScript.enabled = isAlive;
	}

	// run for all clients Remote Procedure Call
	[ClientRpc]
	public void RpcResolveHit() {
		ToggleRenderer (false);
		if (isLocalPlayer) {
			Transform spawn = NetworkManager.singleton.GetStartPosition ();
			transform.position = spawn.position;
			transform.rotation = spawn.rotation;

			ToggleControls (false);
		}

		Invoke ("Respawn", respawnTime);
	}

	private void Respawn() {
		ToggleRenderer (true);

		if (isLocalPlayer) {
			ToggleControls (true);
		}
	}
}
