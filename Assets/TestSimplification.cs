using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Collections.Generic;
using System.Text;

public class TestSimplification : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).GetComponent<MeshFilter>().sharedMesh;
		Test01(sphere);
	}

	static void Test01 (Mesh sphere) {
		var simp = new Simplification(sphere.vertices, sphere.triangles);
		var faceCounter = new Dictionary<int, int>();
		for (var i = 0; i < sphere.triangles.Length; i++) {
			var c = 0;
			var v = sphere.triangles[i];
			faceCounter.TryGetValue(v, out c);
			faceCounter[v] = c + 1;
		}
		
		var invertibleCount = 0;
		var singularCount = 0;
		foreach (var edge in simp.edges) {
			var vi0 = simp.vertexInfos[edge.v0];
			var vi1 = simp.vertexInfos[edge.v1];
			var q = vi0.quad + vi1.quad;
			try { 
				var minPos = q.MinError();
				var error = q * minPos;
				invertibleCount++;
			} catch (nobnak.Algebra.SingularMatrixException) {
				Vector3 bestPos;
				float minError;
				MinErrorOnEdge(simp, vi0, vi1, q, out bestPos, out minError);
				singularCount++;
			}
		}
		Debug.Log(string.Format("invertible={0} singular={1}", invertibleCount, singularCount));
	}
	
	static void MinErrorOnEdge(Simplification simp, Simplification.VertexInfo vi0, Simplification.VertexInfo vi1, Simplification.Q q, out Vector3 bestPos, out float minError) {
		var v0 = simp.vertices[vi0.iVertex];
		var v1 = simp.vertices[vi1.iVertex];
		var vmid = (v0 + v1) * 0.5f;
		var errorV0 = q * v0;
		var errorV1 = q * v1;
		var errorVmid = q * vmid;
		if (errorV0 < errorV1) {
			if (errorV0 < errorVmid) {
				bestPos = v0;
				minError = errorV0;
			} else {
				bestPos = vmid;
				minError = errorVmid;
			}
		} else {
			if (errorV1 < errorVmid) {
				bestPos = v1;
				minError = errorV1;
			} else {
				bestPos = vmid;
				minError = errorVmid;
			}
		}
	}

}
