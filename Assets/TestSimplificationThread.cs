using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;
using System.Threading;

public class TestSimplificationThread : MonoBehaviour {
	public TestIsosphere isoSphere;
	public float bulkReduction = 0.5f;
	
	private Simplification _simp;
	private Mesh _sphere;
	private bool _reductionInProgress = false;

	void Start () {
		_sphere = isoSphere.GetComponent<MeshFilter>().mesh;
		_simp = new Simplification(_sphere.vertices, _sphere.triangles);
	}
	
	void OnGUI() {
		GUILayout.BeginHorizontal();
		var buf = new StringBuilder();
		buf.AppendFormat("Vertex:{0}\n", _sphere.vertexCount);
		buf.AppendFormat("Triangle:{0}", _sphere.triangles.Length / 3);
		GUILayout.TextField(buf.ToString());
		GUILayout.EndHorizontal();
	}
	
	void Update() {
		lock (this) {
			if (_reductionInProgress)
				return;
			
			if (_sphere.vertexCount < 100) {
				isoSphere.Reset();
				_sphere = isoSphere.GetComponent<MeshFilter>().mesh;
				_simp = new Simplification(_sphere.vertices, _sphere.triangles);
			}
			
			_reductionInProgress = true;
			UpdateSphere();
			var targetEdgeCount = (int)(bulkReduction * _simp.costs.Count);
			ThreadPool.QueueUserWorkItem(new WaitCallback(Reduction), targetEdgeCount);
		}
	}
	
	void Reduction(System.Object targetEdgeCountObj) {
		try {
			var targetEdgeCount = (int) targetEdgeCountObj;
			while (targetEdgeCount < _simp.costs.Count) {
				CollapseAnEdge();
			}
		}finally {
			lock(this) {
				_reductionInProgress = false;
			}
		}
	}
	
	void CollapseAnEdge () {
		var edgeCost = _simp.costs.RemoveFront();
		_simp.CollapseEdge(edgeCost);
	}
	
	void UpdateSphere() {
		Vector3[] _outVertices;
		int[] _outTriangles;
		_simp.ToMesh(out _outVertices, out _outTriangles);
		_sphere.Clear();
		_sphere.vertices = _outVertices;
		_sphere.triangles = _outTriangles;
		_sphere.RecalculateNormals();
		_sphere.RecalculateBounds();
	}
}
