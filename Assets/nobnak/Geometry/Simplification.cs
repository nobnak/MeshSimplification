using UnityEngine;
using System.Collections.Generic;
using nobnak.Algebra;
using nobnak.Collection;


namespace nobnak.Geometry {
	public class Simplification {
		public Vector3[] vertices;
		public int[] triangles;
		public VertexInfo[] vertexInfos;
		public HashSet<Edge> edges;
		public BinaryHeap<EdgeCost> costs;
		
		public Simplification(Vector3[] vertices, int[] triangles) {
			this.vertices = vertices;
			this.triangles = triangles;
			this.costs = new BinaryHeap<EdgeCost>(new EdgeCost.Comparer());
			Build(vertices, triangles);
		}
		
		public void Build(Vector3[] vertices, int[] triangles) {
			vertexInfos = new VertexInfo[vertices.Length];
			for (var i = 0; i < vertices.Length; i++)
				vertexInfos[i] = new VertexInfo(i);

			var faces = new Face[triangles.Length / 3];
			edges = new HashSet<Edge>();
			for (var i = 0; i < faces.Length; i++) {
				var iTriangle = i * 3;
				var f = new Face(triangles[iTriangle], triangles[iTriangle + 1], triangles[iTriangle + 2]);
				faces[i] = f;
				for (var iv = 0; iv < 3; iv++) {
					var info = vertexInfos[f[iv]];
					info.faces.AddLast(f);
					
					var edge = new Edge(f[iv], f[iv + 1]);
					if (!edges.Contains(edge))
						edges.Add(edge);
				}
			}
			
			foreach (var info in vertexInfos) {
				foreach (var f in info.faces) {
					var plane = f.Plane(vertices);
					var K = new Q(plane);
					info.quad += K;
				}
			}
			
			foreach (var edge in edges) {
				Vector3 pos;
				float cost;
				Q q;
				MinError(edge, out pos, out cost, out q);
				costs.Add(new EdgeCost(edge, cost, pos));
			}
		}
		
		public void CollapseEdge(Edge edge, Vector3 minPos) {
			var vi0 = vertexInfos[edge.v0];
			var vi1 = vertexInfos[edge.v1];

			var involvedFaces = new HashSet<Face>();
			foreach (var f in vi0.faces)
				involvedFaces.Add(f);
			foreach (var f in vi1.faces)
				involvedFaces.Add(f);
			
			var involvedVertices = new HashSet<VertexInfo>();
			foreach (var f in involvedFaces) {
				for (var iv = 0; iv < 3; iv++) {
					involvedVertices.Add(vertexInfos[f[iv]]);
				}
			}
			foreach (var vinfo in involvedVertices) {
				var faceNode = vinfo.faces.First;
				while (faceNode != null) {
					var next = faceNode.Next;
					if (faceNode.Value.Contains(edge))
						vinfo.faces.Remove(faceNode);
					faceNode = next;
				}
			}
			
			var collapsedFaces = new List<Face>();
			foreach (var f in involvedFaces) {
				if (f.Contains(edge))
					collapsedFaces.Add(f);
			}
			foreach (var f in collapsedFaces)
				involvedFaces.Remove(f);
			
			foreach (var f in involvedFaces)
				f.Renumber(edge.v0, edge.v1);
			vi0.faces.Clear();
			vi1.faces = new LinkedList<Face>(involvedFaces);
			vertices[vi1.iVertex] = minPos;
		}
		
		public void ToMesh(out Vector3[] outVertices, out int[] outTriangles) {
			var vertexIndices = new List<int>(vertexInfos.Length);
			foreach (var vinfo in vertexInfos) {
				if (vinfo.faces.Count > 0)
					vertexIndices.Add(vinfo.iVertex);
			}
			
			var indexMap = new Dictionary<int, int>();
			for (var i = 0; i < vertexIndices.Count; i++)
				indexMap.Add(vertexIndices[i], i);
			
			outVertices = new Vector3[vertexIndices.Count];
			var faces = new HashSet<Face>();
			for (var i = 0; i < outVertices.Length; i++) {
				var vinfo = vertexInfos[vertexIndices[i]];
				outVertices[i] = this.vertices[vinfo.iVertex];
				foreach (var f in vinfo.faces)
					faces.Add(f);
			}
			var triangleStream = new List<int>();
			foreach (var f in faces) {
				for (var iv = 0; iv < 3; iv++) {
					triangleStream.Add(indexMap[f[iv]]);
				}
			}
			outTriangles = triangleStream.ToArray();
		}
		
		public void MinError(Edge edge, out Vector3 minPos, out float minError, out Q q) {
			var vi0 = vertexInfos[edge.v0];
			var vi1 = vertexInfos[edge.v1];
			q = vi0.quad + vi1.quad;
			try { 
				minPos = q.MinError();
				minError = q * minPos;
			} catch (nobnak.Algebra.SingularMatrixException) {
				MinErrorOnEdge(vi0, vi1, q, out minPos, out minError);
			}
		}
	
		public void MinErrorOnEdge(VertexInfo vi0, VertexInfo vi1, Q q, out Vector3 bestPos, out float minError) {
			var v0 = vertices[vi0.iVertex];
			var v1 = vertices[vi1.iVertex];
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
		
		#region Inner Classes
		public class VertexInfo {
			public int iVertex;
			public LinkedList<Face> faces;
			public Q quad;
			
			public VertexInfo(int vertexIndex) {
				this.iVertex = vertexIndex;
				this.faces = new LinkedList<Face>();
				this.quad = new Q();
			}
			
			public override int GetHashCode () {
				return iVertex;
			}
			public override bool Equals (object obj) {
				var vinfo = obj as VertexInfo;
				return vinfo != null && vinfo.iVertex == iVertex;
			}
		}
		
		public class Q {
			public float[] matrix;
			
			public Q() {
				this.matrix = new float[16];
			}
			public Q(Vector4 plane) : this(plane.x, plane.y, plane.z, plane.w) {}
			public Q(float a, float b, float c, float d) {
				matrix = new float[] {
					a * a, a * b, a * c, a * d,
					b * a, b * b, b * c, b * d,
					c * a, c * b, c * c, c * d,
					d * a, d * b, d * c, d * d,
				};
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
			public static float operator*(Q q0, Vector3 p) {
				var m = q0.matrix;
				return p.x * (m[ 0] * p.x + m[ 1] * p.y + m[ 2] * p.z + m[ 3])
					 + p.y * (m[ 4] * p.x + m[ 5] * p.y + m[ 6] * p.z + m[ 7])
					 + p.z * (m[ 8] * p.x + m[ 9] * p.y + m[10] * p.z + m[11])
					 +       (m[12] * p.x + m[13] * p.y + m[14] * p.z + m[15]);
			}
			
			public Vector3 MinError() {
				var lu = new LU(Derivative(), 4);
				lu.Decompose();
				var x = new float[4];
				lu.Solve(new float[]{ 0f, 0f, 0f, 1f}, ref x);
				return new Vector3(x[0], x[1], x[2]);
			}
			public float[] Derivative() {
				return new float[] {
					matrix[ 0], matrix[ 1], matrix[ 2], matrix[ 3],
					matrix[ 4], matrix[ 5], matrix[ 6], matrix[ 7],
					matrix[ 8], matrix[ 9], matrix[10], matrix[11],
					0f,         0f,         0f,         1f
				};
			}

		}
		
		public class Face {
			public int v0, v1, v2;
			
			public Face(int v0, int v1, int v2) {
				this.v0 = v0;
				this.v1 = v1;
				this.v2 = v2;
			}
			
			public Vector4 Plane(Vector3[] vertices) {
				return Plane(vertices[v0], vertices[v1], vertices[v2]);
			}
			
			public bool Contains(Edge edge) {
				for (var i = 0; i < 3; i++) {
					if (this[i] == edge.v0) {
						return (this[i+1] == edge.v1 || this[i+2] == edge.v1);
					}
				}
				return false;
			}
			
			public Face Renumber(int vPrev, int v) {
				if (v0 == vPrev)
					v0 = v;
				if (v1 == vPrev)
					v1 = v;
				if (v2 == vPrev)
					v2 = v;
				return this;
			}
			
			public override string ToString () {
				return string.Format("Face({0},{1},{2})", v0, v1, v2);
			}
			
			public override int GetHashCode () {
				return 83 * (v0 + 151 * (v1 + 19 * v2));
			}
			public override bool Equals (object obj) {
				var f = obj as Face;
				return f != null && f.v0 == v0 && f.v1 == v1 && f.v2 == v2;
			}
			
			public int this[int index] {
				get {
					switch (index % 3) {
					case 0:
						return v0;
					case 1:
						return v1;
					case 2:
						return v2;
					default:
						Debug.Log("index % 3 = " + (index % 3));
						throw new System.IndexOutOfRangeException();
					}
				}
				set {
					switch (index % 3) {
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
			
			public static Vector4 Plane(Vector3 v0, Vector3 v1, Vector3 v2) {
				var e1 = v1 - v0;
				var e2 = v2 - v0;
				var n = Vector3.Cross(e1, e2).normalized;
				var d = -Vector3.Dot(v0, n);
				return new Vector4(n.x, n.y, n.z, d);
			}
		}
		public class Edge {
			public int v0, v1;
			
			public Edge(int v0, int v1) {
				if (v0 < v1) {
					this.v0 = v0;
					this.v1 = v1;
				} else {
					this.v0 = v1;
					this.v1 = v0;
				}
			}
			
			public override int GetHashCode () {
				return 71 * (v0 + 19 * v1);
			}
			public override bool Equals (object obj) {
				var e = obj as Edge;
				return (e != null && e.v0 == v0 && e.v1 == v1);
			}
			
			public override string ToString () {
				return string.Format("Edge({0},{1})", v0, v1);
			}
		}
		
		public class EdgeCost {
			public float cost;
			public Edge edge;
			public Vector3 minPos;
			
			public EdgeCost(Edge edge, float cost, Vector3 minPos) {
				this.edge = edge;
				this.cost = cost;
				this.minPos = minPos;
			}
			
			public class Comparer : IComparer<EdgeCost> {
				#region IComparer[System.Single] implementation
				public int Compare (EdgeCost x, EdgeCost y) {
					if (x.cost < y.cost)
						return -1;
					else if (x.cost == y.cost)
						return 0;
					else 
						return 1;
				}
				#endregion
			}
		}
		#endregion
	}
}