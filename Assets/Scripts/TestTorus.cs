using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Text;

public class TestTorus : MonoBehaviour {
	public TorusObject primitive;
	public float updateInterval;
	public bool interactive;
	
	private Mesh _mesh;
	private Simplification _simp;
	private float _nextUpdateTime;
	
	void Start() {
		_mesh = primitive.GetComponent<MeshFilter>().mesh;
		_simp = new Simplification(_mesh.vertices, _mesh.triangles);
		
		_nextUpdateTime = Time.time + updateInterval;
		
		if (!interactive)
			StartCoroutine("Collapse");
	}
	
	void Update() {
		if (!interactive)
			return;
		
		if (Input.GetMouseButton(0)) {
			_simp.CollapseEdge(_simp.costs.RemoveFront());
			UpdateMesh();
		}
	}

	void OnGUI() {
		if (_mesh == null)
			return;

		GUILayout.BeginHorizontal();
		var buf = new StringBuilder();
		buf.AppendFormat("Vertex:{0}\n", _mesh.vertexCount);
		buf.AppendFormat("Triangle:{0}", _mesh.triangles.Length / 3);
		GUILayout.TextField(buf.ToString());
		GUILayout.EndHorizontal();
	}
	
	IEnumerator Collapse() {
		while (true) {
			yield return 0;
			
			if (_simp.costs.Count <= 5) {
				UpdateMesh ();
				yield return new WaitForSeconds(2f);
				_mesh = primitive.Reset();
				_simp = new Simplification(_mesh.vertices, _mesh.triangles);
			}
			
			_simp.CollapseEdge(_simp.costs.RemoveFront());
			
			if (_nextUpdateTime < Time.time) {
				_nextUpdateTime = Time.time + updateInterval;
				UpdateMesh ();
			}
		}
	}

	void UpdateMesh () { 	
		Vector3[] _outVertices;
		int[] _outTriangles;
		_simp.ToMesh(out _outVertices, out _outTriangles);
		_mesh.Clear();
		_mesh.vertices = _outVertices;
		_mesh.triangles = _outTriangles;
		_mesh.RecalculateNormals();
	}
}
