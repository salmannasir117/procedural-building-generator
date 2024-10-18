// This sample code demonstrates how to create geometry "on demand" based on camera motion.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMotion : MonoBehaviour {

	// move the camera, and perhaps create a new plane
	void Update () {

		// get the horizontal and vertical controls (arrows, or WASD keys)
		float dx = Input.GetAxis ("Horizontal");
		float dz = Input.GetAxis ("Vertical");

		// sensitivity factors for translate and rotate
		float translate_factor = 0.1f;
		float rotate_factor = 0.5f;

		// move the camera based on keyboard input
		if (Camera.main != null) {
			// translate forward or backwards
			Camera.main.transform.Translate (0, 0, dz * translate_factor);

			// rotate left or right
			Camera.main.transform.Rotate (0, dx * rotate_factor, 0);

		}

		// get the main camera position
		Vector3 cam_pos = Camera.main.transform.position;
		//Debug.LogFormat ("x z: {0} {1}", cam_pos.x, cam_pos.z);
	}

}