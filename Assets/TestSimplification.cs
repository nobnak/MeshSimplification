using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {

	void Start () {
		var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().mesh;
		Test01(sphere);		
		Test02(sphere);
	}

	static void Test02 (Mesh sphere) {
		var simp = new Simplification(sphere.vertices, sphere.triangles);
		var edgeCost = simp.costs.RemoveFront();
		Debug.Log("Edge Collapse : " + edgeCost.edge);
		simp.CollapseEdge(edgeCost.edge, edgeCost.minPos);
		
		Vector3[] vertices;
		int[] triangles;
		simp.ToMesh(out vertices, out triangles);
		Debug.Log(string.Format("Before:nVert={0},nTri={1} After:nVert={2},nTri={3}", sphere.vertices.Length, sphere.triangles.Length / 3, vertices.Length, triangles.Length / 3));
		
		var facesAfter = new HashSet<Face>();
		for (var iTriAfter = 0; iTriAfter < triangles.Length; iTriAfter += 3)
			facesAfter.Add(new Face(triangles[iTriAfter], triangles[iTriAfter+1], triangles[iTriAfter+2]));
		for (var iTriBefore = 0; iTriBefore < sphere.triangles.Length; iTriBefore += 3) {
			var fBefore = new Face(sphere.triangles[iTriBefore], sphere.triangles[iTriBefore+1], sphere.triangles[iTriBefore+2]);
			if (!facesAfter.Contains(fBefore))
				Debug.Log("Mismatch : " + fBefore);
		}
		
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
