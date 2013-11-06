using UnityEngine;
using System.Collections;
using nobnak.Geometry;
using System.Linq;

public class TestModel : MonoBehaviour {
	public GameObject prefab;
	public Transform parent;
	public float ratio = 0.5f;
	public int recursionLevel = 2;
	public float scale = 10f;
	
	void OnGUI() {
		GUILayout.BeginVertical();
		if (GUILayout.Button("Simplify"))
			Simplify();
		GUILayout.EndVertical();
	}
	
	void Simplify () {
		var rationAtLevel = 1f;
		var rScale = 1.0f / scale;
		for (var level = 0; level < recursionLevel; level++) {
			rationAtLevel *= ratio;
			var target = (GameObject)Instantiate(prefab);
			target.transform.parent = parent;
			var meshfilters = target.GetComponentsInChildren<MeshFilter>();
			for (var i = 0; i < meshfilters.Length; i++) {
				var mesh = meshfilters[i].mesh;
				var vertices = mesh.vertices.Select((v) => v * scale).ToArray();
				var simp = new Simplification(vertices, mesh.triangles);
				var targetFaceCount = (int)(rationAtLevel * mesh.triangles.Length / 3);
				while (targetFaceCount < simp.faceDb.FaceCount) {
					var edge = simp.costs.RemoveFront();
					simp.CollapseEdge(edge);
				}
				Vector3[] outVertices;
				int[] outTriangles;
				simp.ToMesh(out outVertices, out outTriangles);
				Debug.Log(string.Format("Simplification step {0}/{1}, face {2}/{3}", 
					i, meshfilters.Length, outTriangles.Length / 3, mesh.triangles.Length / 3));
				mesh.Clear();
				mesh.vertices = outVertices.Select((v) => v * rScale).ToArray();
				mesh.triangles = outTriangles;
				mesh.RecalculateNormals();
			}
		}
	}
}
