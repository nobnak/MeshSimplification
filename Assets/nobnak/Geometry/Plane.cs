using UnityEngine;

namespace nobnak.Geometry {
	
	public static class Plane {
		public static Vector4 FromTriangle(Vector3 v0, Vector3 v1, Vector3 v2) {
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			var n = Vector3.Cross(e1, e2).normalized;
			var d = -Vector3.Dot(v0, n);
			return new Vector4(n.x, n.y, n.z, d);
		}
	}
}
