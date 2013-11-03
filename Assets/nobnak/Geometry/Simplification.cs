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
		public FaceDatabase faceDb;
		
		public Simplification(Vector3[] vertices, int[] triangles) {
			this.vertices = vertices;
			this.triangles = triangles;
			this.vertexInfos = InitVertexInfos();
			this.costs = InitCosts();
			this.faceDb = new FaceDatabase();
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
			var faces = faceDb.GetAdjacentFaces(vi0.iVertex);
			foreach (var f in faces) {
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
						costs.Add(v1edge.ToCost(this));
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
		
		VertexInfo[] InitVertexInfos () {
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

		BinaryHeap<EdgeCost> InitCosts () {
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
				var cost = edge.ToCost(this);
				costs.Add(cost);
			}
			return costs;
		}
	
		void MinError(Edge edge, out Vector3 minPos, out float minError, out Quality q) {
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

		void MinErrorOnEdge(VertexInfo vi0, VertexInfo vi1, Quality q, out Vector3 bestPos, out float minError) {
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

		public Vector4 GetPlane(Face f) {
			return Plane.FromTriangle(vertices[f[0]], vertices[f[1]], vertices[f[2]]);
		}
		public Vector4 PerpendicularPlane(Face f, Edge e) {
			var e1 = vertices[f[1]] - vertices[f[0]];
			var e2 = vertices[f[2]] - vertices[f[0]];
			var n = Vector3.Cross(e1, e2);
			return Plane.FromTriangle(vertices[e.v0], vertices[e.v1], n + vertices[e.v0]);
		}
		public EdgeCost ToCost(Simplification simp) {
			Vector3 pos;
			float cost;
			Quality q;
			simp.MinError(this, out pos, out cost, out q);
			return new EdgeCost(this, cost, pos, q);
		}
		
		#region Inner Classes
		public class VertexInfo {
			public int iVertex;
			public Quality quad;
			
			public VertexInfo(int vertexIndex) {
				this.iVertex = vertexIndex;
				this.quad = new Quality();
			}
			
			public void CalculateQuad(Simplification simp) {
				var faces = simp.faceDb.GetAdjacentFaces(iVertex);
				foreach (var f in faces) {
					var plane = GetPlane(f);
					var K = new Quality(plane);
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
		
		public class EdgeCost {
			public float cost;
			public Edge edge;
			public Vector3 minPos;
			public Quality quad;
			
			public EdgeCost(Edge edge, float cost, Vector3 minPos, Quality quad) {
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