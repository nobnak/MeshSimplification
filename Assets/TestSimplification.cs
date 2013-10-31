using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {
	public MeshFilter isoSphere;

	void Start () {
		var sphere = isoSphere.mesh;
		Test01(sphere);		
		Test02(sphere);
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			Test02(isoSphere.mesh);
		}
	}

	static void Test02 (Mesh sphere) {
		var simp = new Simplification(sphere.vertices, sphere.triangles);
		var edgeCost = simp.costs.RemoveFront();
		Debug.Log(string.Format("{0},{1}->{2}", 
			sphere.vertices[edgeCost.edge.v0], sphere.vertices[edgeCost.edge.v1], edgeCost.minPos));
		simp.CollapseEdge(edgeCost.edge, edgeCost.minPos);
		
		Vector3[] vertices;
		int[] triangles;
		simp.ToMesh(out vertices, out triangles);
		Debug.Log(string.Format("nVert({0}->{1}),nTri({2}->{3})", sphere.vertices.Length, vertices.Length, sphere.triangles.Length / 3, triangles.Length / 3));

		
		sphere.Clear();
		sphere.vertices = vertices;
		sphere.triangles = triangles;
		sphere.RecalculateNormals();
		sphere.RecalculateBounds();
	}

	static void Test01 (Mesh sphere) {
		var simp = new Simplification(sphere.vertices, sphere.triangles);
		var minCost = float.MinValue;
		var costs = simp.costs;
		while (costs.Count > 0) {
			var front = costs.RemoveFront();
			if (front.cost < minCost)
				Debug.Log("Cost inorder");
			minCost = front.cost;
		}
	}
	
	struct Face {
		int v0, v1, v2;
		
		public Face(int v0, int v1, int v2) {
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
		}
		
		public override int GetHashCode () {
			return 13 * (v0 + 53 * (v1 + 71 * v2));
		}
		public override bool Equals (object obj) {
			if (obj.GetType() != typeof(Face))
				return false;
			var f = (Face)obj;
			return f.v0 == v0 && f.v1 == v1 && f.v2 == v2;
		}
		public override string ToString () {
			return string.Format("Face({0},{1},{2})", v0, v1, v2);
		}
	}
}
