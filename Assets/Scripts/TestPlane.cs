using UnityEngine;
using System.Collections;
using nobnak.Geometry;

public class TestPlane : MonoBehaviour {
	public PlaneObject planeObj;
	public float updateInterval;
	
	private Mesh _mesh;
	private Simplification _simp;
	private float _nextUpdateTime;
	
	void Start() {
		_mesh = planeObj.GetComponent<MeshFilter>().mesh;
		_simp = new Simplification(_mesh.vertices, _mesh.triangles);
		
		_nextUpdateTime = Time.time + updateInterval;
		
		StartCoroutine("Collapse");
	}
	
	IEnumerator Collapse() {
		while (true) {
			yield return 0;
			
			if (_simp.costs.Count <= 5) {
				yield return new WaitForSeconds(2f);
				_mesh = planeObj.Reset();
				_simp = new Simplification(_mesh.vertices, _mesh.triangles);
			}
			
			_simp.CollapseEdge(_simp.costs.RemoveFront());
			
			if (_nextUpdateTime < Time.time) {
				_nextUpdateTime = Time.time + updateInterval;
				
				Vector3[] _outVertices;
				int[] _outTriangles;
				_simp.ToMesh(out _outVertices, out _outTriangles);
				_mesh.Clear();
				_mesh.vertices = _outVertices;
				_mesh.triangles = _outTriangles;
				_mesh.RecalculateNormals();
			}
		}
	}
}
