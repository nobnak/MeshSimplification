using UnityEngine;
using nobnak.Algebra;

namespace nobnak.Geometry {
	
	public static class Plane {
		public static Vector4D FromTriangle(Vector3D v0, Vector3D v1, Vector3D v2) {
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			var n = Vector3D.Cross(e1, e2).normalized;
			var d = -Vector3D.Dot(v0, n);
			return new Vector4D(n.x, n.y, n.z, d);
		}
	}
}
