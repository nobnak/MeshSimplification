using UnityEngine;
using System.Collections.Generic;


namespace nobnak.Geometry {
	public class Simplification {
		public Vector3[] vertices;
		public int[] triangles;
		public VertexInfo[] vertexInfos;
		
		public void Build(Vector3[] vertices, int[] triangles) {
			vertexInfos = new VertexInfo[vertices.Length];
			for (var i = 0; i < vertices.Length; i++) {
				var vinfo = new VertexInfo() { iVertex = i };
				vertexInfos[i] = vinfo;
			}

			var faces = new Face[triangles.Length / 3];
			for (var i = 0; i < faces.Length; i++) {
				var iTriangle = i * 3;
				var f = new Face() { 
					v0 = triangles[iTriangle], 
					v1 = triangles[iTriangle + 1], 
					v2 = triangles[iTriangle + 2] 
				};
				faces[i] = f;
				for (var iv = 0; iv < 3; iv++) {
					var info = vertexInfos[f[iv]];
					if (info.faces == null)
						info.faces = new LinkedList<Face>();
					info.faces.AddLast(f);
				}
			}
			
			foreach (var info in vertexInfos) {
				if (info.faces == null)
					continue;
				var q = new Q();
				foreach (var f in info.faces) {
					var plane = Plane(vertices[f.v0], vertices[f.v1], vertices[f.v2]);
					var K = QuadError(plane.x, plane.y, plane.z, plane.w);
					q += K;
				}
			}
		}
		
		public static Vector4 Plane(Vector3 v0, Vector3 v1, Vector3 v2) {
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			var n = Vector3.Cross(e1, e2).normalized;
			var d = Vector3.Dot(v0, n);
			var sign = (d >= 0) ? +1f : -1f;
			n *= sign;
			return new Vector4(n.x, n.y, n.z, sign * d);
		}
		
		public static Q QuadError(float a, float b, float c, float d) {
			return new Q() { 
				matrix = new float[] {
					a * a, a * b, a * c, a * d,
					b * a, b * b, b * c, b * d,
					c * a, c * b, c * c, c * d,
					d * a, d * b, d * c, d * d,
				}
			};
		}
		
		#region Inner Classes
		public class VertexInfo {
			public int iVertex;
			public LinkedList<Face> faces;
			public Q quad;
		}
		
		public class Q {
			public float[] matrix;
			
			public Q() {
				this.matrix = new float[16];
			}
			
			public static Q operator+(Q q0, Q q1) {
				var q = new Q();
				var mat = q.matrix;
				var mq0 = q0.matrix;
				var mq1 = q1.matrix;
				for (var i = 0; i < mat.Length; i++)
					mat[i] = mq0[i] + mq1[i];
				return q;
			}
			public static Q operator*(Q q0, float multi) {
				var q = new Q();
				var mat = q.matrix;
				var mq0 = q0.matrix;
				for (var i = 0; i < mat.Length; i++)
					mat[i] = multi * mq0[i];
				return q;
			}
		}
		
		public class Face {
			public int v0, v1, v2;
			public int this[int index] {
				get {
					switch (index) {
					case 0:
						return v0;
					case 1:
						return v1;
					case 2:
						return v2;
					default:
						throw new System.IndexOutOfRangeException();
					}
				}
				set {
					switch (index) {
					case 0:
						v0 = value;
						break;
					case 1:
						v1 = value;
						break;
					case 2:
						v2 = value;
						break;
					}
				}
			}
		}
		#endregion
	}
}