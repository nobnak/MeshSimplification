using UnityEngine;
using System.Collections.Generic;
using nobnak.Algebra;
using nobnak.Collection;


namespace nobnak.Geometry {
	public class Simplification {
		public static float penaltyFactor = 1000f;

		public Vector3[] vertices;
		public int[] triangles;
		public VertexInfo[] vertexInfos;
		public BinaryHeap<EdgeCost> costs;
		
		public Simplification(Vector3[] vertices, int[] triangles) {
			this.vertices = vertices;
			this.triangles = triangles;
			this.vertexInfos = InitVertexInfos(vertices, triangles);
			this.costs = InitCosts(vertices, vertexInfos);
		}
		
		public void CollapseEdge(EdgeCost edgeCost) {
			var edge = edgeCost.edge;
			var vi0 = vertexInfos[edge.v0];
			var vi1 = vertexInfos[edge.v1];
			
			var icost = 0;
			while (icost < costs.Count) {
				var cost = costs[icost];
				if (cost.edge.Contains(edge.v0) || cost.edge.Contains(edge.v1))
					costs.Remove(icost);
				else
					icost++;
			}
			
			var vinfosHavingCollapsedFaces = new HashSet<VertexInfo>();
			foreach (var f in vi0.faces) {
				if (f.Contains(edge)) {
					vinfosHavingCollapsedFaces.Add(vertexInfos[f[0]]);
					vinfosHavingCollapsedFaces.Add(vertexInfos[f[1]]);
					vinfosHavingCollapsedFaces.Add(vertexInfos[f[2]]);
				}
			}
			foreach (var vinfo in vinfosHavingCollapsedFaces) {
				var node = vinfo.faces.First;
				while (node != null) {
					var next = node.Next;
					if (node.Value.Contains(edge))
						vinfo.faces.Remove(node);
					node = next;
				}
			}
			
			foreach (var f in vi0.faces) {
				f.Renumber(edge.v0, edge.v1);
				vi1.faces.AddLast(f);
			}
			vi0.faces.Clear();
			
			vertices[vi1.iVertex] = edgeCost.minPos;
			vi1.quad = edgeCost.quad;
			var v1edges = new HashSet<Edge>();
			foreach (var f in vi1.faces) {
				for (var round = 0; round < 3; round++) {
					var v1edge = new Edge(f[round], f[round+1]);
					if (v1edge.Contains(vi1.iVertex) && !v1edges.Contains(v1edge)) {
						costs.Add(v1edge.ToCost(vertices, vertexInfos));
						v1edges.Add(v1edge);
					}
				}
			}
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
		
		static VertexInfo[] InitVertexInfos (Vector3[] vertices, int[] triangles) {
			var vertexInfos = new VertexInfo[vertices.Length];
			for (var i = 0; i < vertices.Length; i++)
				vertexInfos[i] = new VertexInfo(i);
			
			for (var iTriangle = 0; iTriangle < triangles.Length; iTriangle += 3) {
				var f = new Face(triangles[iTriangle], triangles[iTriangle + 1], triangles[iTriangle + 2]);
				for (var iv = 0; iv < 3; iv++) {
					var info = vertexInfos[f[iv]];
					info.faces.AddLast(f);
				}
			}
			
			foreach (var info in vertexInfos)
				info.CalculateQuad(vertices);
			
			return vertexInfos;
		}

		static BinaryHeap<EdgeCost> InitCosts (Vector3[] vertices, VertexInfo[] vertexInfos) {
			var costs = new BinaryHeap<EdgeCost>(new EdgeCost.Comparer());
			var edges = new HashCounter<Edge>();
			foreach (var vinfo in vertexInfos) {
				foreach (var f in vinfo.faces) {
					for (var iv = 0; iv < 3; iv++) {
						var edge = new Edge(f[iv], f[iv + 1]);
						edges[edge]++;
					}
				}
			}
			foreach (var edge in edges) {
				var cost = edge.ToCost(vertices, vertexInfos);
				if (edges[edge] == 1) {
					var vinfo = vertexInfos[edge.v0];
					Face found = null;
					foreach (var f in vinfo.faces) {
						if (f.Contains(edge)) {
							found = f;
							break;
						}
					}
					var q = new Q(found.PerpendicularPlane(vertices, edge));
					cost.quad += q * penaltyFactor;
				}
				costs.Add(cost);
			}
			return costs;
		}
	
		static void MinError(Vector3[] vertices, VertexInfo[] vertexInfos, Edge edge, out Vector3 minPos, out float minError, out Q q) {
			var vi0 = vertexInfos[edge.v0];
			var vi1 = vertexInfos[edge.v1];
			q = vi0.quad + vi1.quad;
			try { 
				minPos = q.MinError();
				minError = q * minPos;
			} catch (nobnak.Algebra.SingularMatrixException) {
				MinErrorOnEdge(vertices, vi0, vi1, q, out minPos, out minError);
			}
		}

		static void MinErrorOnEdge(Vector3[] vertices, VertexInfo vi0, VertexInfo vi1, Q q, out Vector3 bestPos, out float minError) {
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
			
			public void CalculateQuad(Vector3[] vertices) {
				foreach (var f in faces) {
					var plane = f.Plane(vertices);
					var K = new Q(plane);
					quad += K;
				}
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
			public Vector4 PerpendicularPlane(Vector3[] vertices, Edge edge) {
				var e1 = vertices[v1] - vertices[v0];
				var e2 = vertices[v2] - vertices[v0];
				var n = Vector3.Cross(e1, e2);
				return Plane(vertices[v1], vertices[v2], n + vertices[v0]);
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
			
			public bool Contains(int v) {
				return v0 == v || v1 == v;
			}
			
			public EdgeCost ToCost(Vector3[] vertices, VertexInfo[] vertexInfos) {
				Vector3 pos;
				float cost;
				Q q;
				MinError(vertices, vertexInfos, this, out pos, out cost, out q);
				return new EdgeCost(this, cost, pos, q);
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
			public Q quad;
			
			public EdgeCost(Edge edge, float cost, Vector3 minPos, Q quad) {
				this.edge = edge;
				this.cost = cost;
				this.minPos = minPos;
				this.quad = quad;
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