using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {

	void Start () {
		var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().sharedMesh;
		Test01(sphere);
	}

	static void Test01 (Mesh sphere) {
		var simp = new Simplification(sphere.vertices, sphere.triangles);
		var costs = simp.costs;
		while (costs.Count > 0) {
			var front = costs.RemoveFront();
			Debug.Log(string.Format("Cost={0:e2} edge={1}", front.cost, front.edge));
		}
	}
}
