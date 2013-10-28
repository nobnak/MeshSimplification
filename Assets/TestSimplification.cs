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
		
		var vertexInfos = simp.vertexInfos;
		var errorCounter = 0;
		foreach (var info in vertexInfos) {
			try { 
				var minErrorPos = info.quad.MinError();
				var error = info.quad * minErrorPos;
				//Debug.Log(string.Format("Pos={0} Error={1:e}", minErrorPos, error));
			} catch {
				errorCounter++;
				var buf = new StringBuilder();
				buf.AppendFormat("v={0} nFaces={1}\n", info.iVertex, info.faces.Count);
				foreach (var f in info.faces) {
					buf.AppendFormat("Plane : {0:e2}\n", f.Plane(sphere.vertices));
				}
				Debug.Log(buf);
			}
		}
		Debug.Log("nErrors=" + errorCounter);
	}
}
