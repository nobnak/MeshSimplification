
namespace nobnak.Algebra {

	public class LU {
		public readonly int n;
		public readonly float[] a;
		public readonly float[] lu;
		public readonly int[] pivot;
		
		public LU(float[] A, int N) {
			this.n = N;
			this.a = A;
			this.lu = new float[N * N];
			this.pivot = new int[N];
			for (var i = 0; i < N; i++)
				this.pivot[i] = i;
			Decompose();
		}
		
		public void Decompose() {
			for (var j = 0; j < n; j++) {
				for (var i = 0; i < j; i++) {
					var beta = a[ij2lin(i, j)];
					for (var k = 0; k < i; k++) {
						beta -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
					}
					lu[ij2lin(i, j)] = beta;
				}
				
				{
					var maxBetajj = 0f;
					var pivotI = j;
					for (var i = j; i < n; i++) {
						var tmpBetajj = a[ij2lin(i, j)];
						for (var k = 0; k < j; k++) {
							tmpBetajj -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
						}
						tmpBetajj = Math.Abs(tmpBetajj);
						if (maxBetajj < tmpBetajj) {
							maxBetajj = tmpBetajj;
							pivotI = i;
						}
					}
					var tmp = pivot[j]; pivot[j] = pivot[pivotI]; pivot[pivotI] = tmp;
				}						
				
				{
					var i = j;
					var beta = a[ij2lin(i, j)];
					for (var k = 0; k < i; k++) {
						beta -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
					}
					lu[ij2lin(i, j)] = beta;
				}
				
				for (var i = j + 1; i < n; i++) {
					var alpha = a[ij2lin(i, j)];
					for (var k = 0; k < j; k++) {
						alpha -= lu[ij2lin(i, k)] * lu[ij2lin(k, j)];
					}
					lu[ij2lin(i, j)] = alpha / lu[ij2lin(j, j)];
				}
			}
		}
		
		public void Solve(float[] b) {
			for (var i = 0; i < n; i++) {
				var y = b[pivot[i]];
				for (var k = 0; k < i; k++) {
					y -= lu[ij2lin(i, k)] * b[pivot[k]];
				}
				b[pivot[i]] = y;
			}
			
			for (var i = n - 1; i >= 0; i--) {
				var x = b[pivot[i]];
				var rBetajj = 1f / lu[ij2lin(i, i)];
				for (var k = i + 1; k < n; k++) {
					x -= lu[ij2lin(i, k)] * b[pivot[k]];
				}
				b[pivot[i]] = x * rBetajj;
			}
		}
		
		public int ij2lin(int i, int j) {
			return pivot[i] * n + j;
		}
	}
	
	public static class Math {
		public static float Abs(float val) {
			return (val < 0f) ? -val : val;			
		}
	}
}