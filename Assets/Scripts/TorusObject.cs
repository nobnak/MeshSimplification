using UnityEngine;
using System.Collections;

public class TorusObject : MonoBehaviour {
	public int nbRadSeg = 24;
	public int nbSides = 18;
	
	private Mesh _mesh;

	void Awake () {
		Reset();
	}
	
	public Mesh Reset() {
		Destroy(_mesh);
		_mesh = ProceduralTorus.Create(nbRadSeg, nbSides);
		GetComponent<MeshFilter>().mesh = _mesh;
		return _mesh;
	}
	
	void OnDestroy() {
		Destroy(_mesh);
	}
}
