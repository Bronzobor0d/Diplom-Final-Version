using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotateAround : MonoBehaviour {

	public GameObject TowerPlace;

	void Update ()
	{
		transform.RotateAround(TowerPlace.transform.position, Vector3.up, 20 * Time.deltaTime);
	}
}