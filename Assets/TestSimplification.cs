using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {
	public MeshFilter isoSphere;
	
	private Simplification _simp;

	void Start () {
		var sphere = isoSphere.mesh;
		
		_simp = new Simplification(sphere.vertices, sphere.triangles);
		
		Test02(sphere);
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Test02(isoSphere.mesh);
		}
	}

	void Test02 (Mesh sphere) {
		var edgeCost = _simp.costs.RemoveFront();
		Debug.Log(string.Format("{0},{1}->{2}", 
			sphere.vertices[edgeCost.edge.v0], sphere.vertices[edgeCost.edge.v1], edgeCost.minPos));		

		_simp.CollapseEdge(edgeCost);
		
		Vector3[] vertices;
		int[] triangles;
		_simp.ToMesh(out vertices, out triangles);
		Debug.Log(string.Format("nVert({0}->{1}),nTri({2}->{3})", sphere.vertices.Length, vertices.Length, sphere.triangles.Length / 3, triangles.Length / 3));

		
		sphere.Clear();
		sphere.vertices = vertices;
		sphere.triangles = triangles;
		sphere.RecalculateNormals();
		sphere.RecalculateBounds();
	}
}
