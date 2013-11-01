using UnityEngine;
using System.Collections;

public class PlaneObject : MonoBehaviour {
	public int resolution = 2;
	
	private Mesh _mesh;

	void Awake () {
		_mesh = ProcedualPlane.Create(resolution);
		GetComponent<MeshFilter>().mesh = _mesh;
	}
	
}
