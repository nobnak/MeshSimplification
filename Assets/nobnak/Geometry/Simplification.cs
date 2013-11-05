using UnityEngine;
using System.Collections.Generic;
using nobnak.Algebra;
using nobnak.Collection;
using System.Text;


namespace nobnak.Geometry {
	public class Simplification {
		public float boundaryPenalty = 1000f;
		public float normalFlippingPenalty = 1000f;

		public Vector3[] vertices;
		public int[] triangles;
		public VertexInfo[] vertexInfos;
		public BinaryHeap<EdgeCost> costs;
		public FaceDatabase faceDb;
		
		public Simplification(Vector3[] vertices, int[] triangles) {
			this.vertices = vertices;
			this.triangles = triangles;
			this.faceDb = new FaceDatabase();
			this.vertexInfos = InitVertexInfos();
			this.costs = InitCosts();
		}
		
		public void CollapseEdge(EdgeCost edgeCost) {
			var edge = edgeCost.edge;
			
			var invalidEdges = new HashSet<Edge>(faceDb.GetNormalFlippingCandidateEdges(edge));
			var icost = 0;
			var nCosts = costs.Count;
			var nInvalidEdges = invalidEdges.Count;
			while (icost < nCosts && nInvalidEdges > 0) {
				var cost = costs[icost];
				if (invalidEdges.Remove(cost.edge)) {
					nInvalidEdges--;
					nCosts--;
					costs.Remove(icost);
				} else {
					icost++;
				}
			}
			
			var collapsedFaces = faceDb.GetAdjacentFaces(edge);
			foreach (var f in collapsedFaces)
				faceDb.Remove(f);
			
			var v0faces = faceDb.GetAdjacentFaces(edge.v0);
			foreach (var f in v0faces)
				f.Renumber(edge.v0, edge.v1);
			
			var vi1 = vertexInfos[edge.v1];
			vertices[vi1.iVertex] = edgeCost.minPos;
			vi1.quad = edgeCost.quad;
			foreach (var e in faceDb.GetNormalFlippingCandidateEdges(edge.v1))
				costs.Add(CalculateCost(e));
		}
		
		public void ToMesh(out Vector3[] outVertices, out int[] outTriangles) {
			var vertexIndices = new List<int>(faceDb.Vertices);
			var indexMap = new Dictionary<int, int>();
			for (var i = 0; i < vertexIndices.Count; i++)
				indexMap.Add(vertexIndices[i], i);
			
			outVertices = new Vector3[vertexIndices.Count];
			for (var i = 0; i < outVertices.Length; i++) {
				var vinfo = vertexInfos[vertexIndices[i]];
				outVertices[i] = this.vertices[vinfo.iVertex];
			}
			var triangleStream = new List<int>();
			foreach (var f in faceDb.Faces) {
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
				new Face(faceDb, triangles[iTriangle], triangles[iTriangle + 1], triangles[iTriangle + 2]);
			}
			
			foreach (var info in vertexInfos) {
				var adjFaces = faceDb.GetAdjacentFaces(info.iVertex);
				foreach (var f in adjFaces)
					info.quad += CalculateQuad(f);
			}
			
			return vertexInfos;
		}

		BinaryHeap<EdgeCost> InitCosts () {
			var costs = new BinaryHeap<EdgeCost>(new EdgeCost.Comparer());
			var edges = new HashCounter<Edge>();
			foreach (var f in faceDb.Faces) {
				for (var iv = 0; iv < 3; iv++) {
					var edge = new Edge(f[iv], f[iv + 1]);
					edges[edge]++;
				}
			}
			foreach (var edge in edges) {
				var cost = CalculateCost(edge);
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

		Vector4 GetPlane(Face f) {
			return Plane.FromTriangle(vertices[f[0]], vertices[f[1]], vertices[f[2]]);
		}
		Vector3 GetNormal(Face f) {
			var e1 = vertices[f[1]] - vertices[f[0]];
			var e2 = vertices[f[2]] - vertices[f[0]];
			return Vector3.Cross(e1, e2);
		}
		Vector3 GetNormal(Vector3 v0, Vector3 v1, Vector3 v2) {
			var e1 = v1 - v0;
			var e2 = v2 - v0;
			return Vector3.Cross(e1, e2);
		}
		Vector4 PerpendicularPlane(Face f, Edge e) {
			var n = GetNormal(f);
			return Plane.FromTriangle(vertices[e.v0], vertices[e.v1], n + vertices[e.v0]);
		}
		int CountNormalFlipping(Edge e, Vector3 minPos) {
			var fillingFaces = faceDb.GetFillHoleFaces(e);
			var normalsBefore = new Vector3[fillingFaces.Length];
			var normalsAfter = new Vector3[fillingFaces.Length];
			for (var i = 0; i < fillingFaces.Length; i++) {
				var f = fillingFaces[i];
				normalsBefore[i] = GetNormal(f);
				normalsAfter[i] = GetNormal(
					e.Contains(f[0]) ? minPos : vertices[f[0]],
					e.Contains(f[1]) ? minPos : vertices[f[1]],
					e.Contains(f[2]) ? minPos : vertices[f[2]]);
			}
			var counter = 0;
			for (var i = 0; i < normalsBefore.Length; i++) {
				if (Vector3.Dot(normalsBefore[i], normalsAfter[i]) <= 0)
					counter++;
			}
			return counter;
		}
		EdgeCost CalculateCost(Edge e) {
			Vector3 pos;
			float cost;
			Quality q;
			MinError(e, out pos, out cost, out q);
			var nNormalFlippings = CountNormalFlipping(e, pos);
			if (nNormalFlippings > 0) {
				cost += Mathf.Max(1e-6f, cost) * normalFlippingPenalty * nNormalFlippings;
			}
			return new EdgeCost(e, cost, pos, q);
		}
		
		Quality CalculateQuad(Face f) {
			var plane = GetPlane(f);
			var q = new Quality(plane);
			for (var i = 0; i < 3; i++) {
				var edge = new Edge(f[i], f[i+1]);
#if true
				var adjFaces = faceDb.GetAdjacentFaces(edge);
				if (adjFaces.Length == 1) 
					q += new Quality(PerpendicularPlane(f, edge)) * boundaryPenalty;
#endif
			}
			return q;
		}
		
		#region Inner Classes
		public class VertexInfo {
			public int iVertex;
			public Quality quad;
			
			public VertexInfo(int vertexIndex) {
				this.iVertex = vertexIndex;
				this.quad = new Quality();
			}
			
			#region Object
			public override int GetHashCode () {
				return iVertex;
			}
			public override bool Equals (object obj) {
				var vinfo = obj as VertexInfo;
				return vinfo != null && vinfo.iVertex == iVertex;
			}
			#endregion
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