using UnityEngine;
using System.Collections;

public class PlaneObject : MonoBehaviour {
	public int resolution = 2;
	public float size = 1f;
	
	private Mesh _mesh;

	void Awake () {
		Reset();
	}
	
	public Mesh Reset() {
		Destroy(_mesh);
		_mesh = ProcedualPlane.Create(resolution, size);
		GetComponent<MeshFilter>().mesh = _mesh;
		return _mesh;
	}
	
	void OnDestroy() {
		Destroy(_mesh);
	}
	
}
