using UnityEngine;


namespace nobnak.Geometry {
	public class Simplification {
		public VertexInfo[] vertexInfos;
		
		public void Build(Vector3[] vertices, int[] triangles) {
			vertexInfos = new VertexInfo[vertices.Length];
			for (var i = 0; i < triangles.Length; i += 3) {
				
			}
		}
		
		public Vector4 Plane(Vector3 v0, Vector3 v1, Vector3 v2) {
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			var n = Vector3.Cross(e1, e2).normalized;
			var d = Vector3.Dot(v0, n);
			var sign = (d >= 0) ? +1f : -1f;
			n *= sign;
			return new Vector4(n.x, n.y, n.z, sign * d);
		}
		
		public class VertexInfo {
			public Vector3 vertex;
			public float[] Q;
		}
	}
}