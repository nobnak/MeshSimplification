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
		
		var successCount = 0;
		var failCount = 0;
		foreach (var edge in simp.edges) {
			var v0 = simp.vertexInfos[edge.v0];
			var v1 = simp.vertexInfos[edge.v1];
			var q = v0.quad + v1.quad;
			try { 
				var minPos = q.MinError();
				var error = q * minPos;
				if (error < -1e-6f)
					Debug.Log(string.Format("edge={0} pos={1} error={2:e}", edge, minPos, error));
				successCount++;
			} catch (nobnak.Algebra.SingularMatrixException) {

				failCount++;
			}
		}
		Debug.Log(string.Format("success={0} fail={1}", successCount, failCount));
	}

}
