using UnityEngine;
using System.Collections;
using nobnak.Geometry;

public class TestPlane : MonoBehaviour {
	public PlaneObject planeObj;
	
	private Mesh _mesh;
	private Simplification _simp;
	
	
	void Start() {
		_mesh = planeObj.GetComponent<MeshFilter>().mesh;
		_simp = new Simplification(_mesh.vertices, _mesh.triangles);
		
		
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			_simp.CollapseEdge(_simp.costs.RemoveFront());
			Vector3[] outVertices;
			int[] outTriangles;
			_simp.ToMesh(out outVertices, out outTriangles);
			
			_mesh.Clear();
			_mesh.vertices = outVertices;
			_mesh.triangles = outTriangles;
		}
	}
}
