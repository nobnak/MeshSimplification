using UnityEngine;
using nobnak.Algebra;

namespace nobnak.Geometry {

	public class Quality {
		public double[] matrix;
		
		public Quality() {
			this.matrix = new double[16];
		}
		public Quality(Vector4D plane) : this(plane.x, plane.y, plane.z, plane.w) {}
		public Quality(double a, double b, double c, double d) {
			matrix = new double[] {
				a * a, a * b, a * c, a * d,
				b * a, b * b, b * c, b * d,
				c * a, c * b, c * c, c * d,
				d * a, d * b, d * c, d * d,
			};
		}
		
		public static Quality operator+(Quality q0, Quality q1) {
			var q = new Quality();
			var mat = q.matrix;
			var mq0 = q0.matrix;
			var mq1 = q1.matrix;
			for (var i = 0; i < mat.Length; i++)
				mat[i] = mq0[i] + mq1[i];
			return q;
		}
		public static Quality operator*(Quality q0, double multi) {
			var q = new Quality();
			var mat = q.matrix;
			var mq0 = q0.matrix;
			for (var i = 0; i < mat.Length; i++)
				mat[i] = multi * mq0[i];
			return q;
		}
		public static Quality operator*(double multi, Quality q0) { return q0 * multi; }
		public static double operator*(Quality q0, Vector3D p) {
			var m = q0.matrix;
			return p.x * (m[ 0] * p.x + m[ 1] * p.y + m[ 2] * p.z + m[ 3])
				 + p.y * (m[ 4] * p.x + m[ 5] * p.y + m[ 6] * p.z + m[ 7])
				 + p.z * (m[ 8] * p.x + m[ 9] * p.y + m[10] * p.z + m[11])
				 +       (m[12] * p.x + m[13] * p.y + m[14] * p.z + m[15]);
		}
		
		public Vector3D MinError() {
			var lu = new LU(Derivative(), 4);
			lu.Decompose();
			var x = new double[4];
			lu.Solve(new double[]{ 0.0, 0.0, 0.0, 1.0}, ref x);
			return new Vector3D(x[0], x[1], x[2]);
		}
		public double[] Derivative() {
			return new double[] {
				matrix[ 0], matrix[ 1], matrix[ 2], matrix[ 3],
				matrix[ 4], matrix[ 5], matrix[ 6], matrix[ 7],
				matrix[ 8], matrix[ 9], matrix[10], matrix[11],
				0.0,         0.0,         0.0,         1.0
			};
		}
	}
}