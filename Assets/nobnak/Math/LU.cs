
namespace nobnak.Math {

	public class LU {
		public int n;
		public float[] a;
		public float[] lu;
		
		public LU(float[] A, int N) {
			this.n = N;
			this.a = A;
			this.lu = new float[N * N];
			Decompose();
		}
		
		public void Decompose() {
			for (var j = 0; j < n; j++) {
				for (var i = 0; i <= j; i++) {
					var beta = a[ij2lin(i, j)];
					for (var k = 0; k < i; k++) {
						beta -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
					}
					lu[ij2lin(i, j)] = beta;
				}
				
				var rBetajj = 1f / lu[ij2lin(j, j)];
				for (var i = j + 1; i < n; i++) {
					var alpha = a[ij2lin(i, j)];
					for (var k = 0; k < j; k++) {
						alpha -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
					}
					lu[ij2lin(i, j)] = alpha * rBetajj;
				}
			}
		}
		
		public int ij2lin(int i, int j) {
			return i * n + j;
		}
	}
}