using UnityEngine;
using System.Collections;

public class Pole : MonoBehaviour {
	public float rotationSpeed = 45;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		var dt = Time.deltaTime;
		transform.localRotation *= Quaternion.AngleAxis(rotationSpeed * dt, Vector3.up);
	}
}
