using UnityEngine;
using System.Collections;

public class TestIsosphere : MonoBehaviour {
	private Mesh _mesh;

	void Awake () {
		_mesh = IsoSphere.Create();
		var meshFilter = GetComponent<MeshFilter>();
		meshFilter.mesh = _mesh;
	}
	
	void OnDestroy() {
		Destroy(_mesh);
	}
}
