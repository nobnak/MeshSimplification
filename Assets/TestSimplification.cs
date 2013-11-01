using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {
	public TestIsosphere isoSphere;
	
	private Simplification _simp;
	private Mesh _sphere;

	void Start () {
		_sphere = isoSphere.GetComponent<MeshFilter>().mesh;
		_simp = new Simplification(_sphere.vertices, _sphere.triangles);
		
		StartCoroutine("IncrementalSimplify");
	}
	
	void OnGUI() {
		GUILayout.BeginHorizontal();
		var buf = new StringBuilder();
		buf.AppendFormat("Vertex:{0}\n", _sphere.vertexCount);
		buf.AppendFormat("Triangle:{0}", _sphere.triangles.Length / 3);
		GUILayout.TextField(buf.ToString());
		GUILayout.EndHorizontal();
	}
	
	IEnumerator IncrementalSimplify() {
		while (true) {
			yield return 0;
			if (_sphere.triangles.Length < 66) {
				yield return new WaitForSeconds(3f);
				isoSphere.Reset();
				_sphere = isoSphere.GetComponent<MeshFilter>().mesh;
				_simp = new Simplification(_sphere.vertices, _sphere.triangles);
			}
				
			CollapseAnEdge(_sphere);
		}
	}
	
	void CollapseAnEdge (Mesh sphere) {
		var edgeCost = _simp.costs.RemoveFront();

		_simp.CollapseEdge(edgeCost);
		
		Vector3[] vertices;
		int[] triangles;
		_simp.ToMesh(out vertices, out triangles);
		
		sphere.Clear();
		sphere.vertices = vertices;
		sphere.triangles = triangles;
		sphere.RecalculateNormals();
		sphere.RecalculateBounds();
	}
}
