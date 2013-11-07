using UnityEngine;

namespace nobnak.Algebra {

	public class Vector3D {
		public double x;
		public double y;
		public double z;
		
		public Vector3D() : this(0.0, 0.0, 0.0) { }
		public Vector3D(double x, double y, double z) {
			this.x = x; this.y = y; this.z = z;
		}
		
		public Vector3D normalized {
			get {
				var rLen = 1.0 / System.Math.Sqrt(x * x + y * y + z * z);
				return new Vector3D(x * rLen, y * rLen, z * rLen);
			}
		}
		
		public double this[int index] {
			get {
				switch (index) {
				case 0:
					return x;
				case 1:
					return y;
				case 2:
					return z;
				default:
					throw new System.IndexOutOfRangeException();
				}
			}
			set {
				switch (index) {
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				case 2:
					z = value;
					break;
				default:
					throw new System.IndexOutOfRangeException();
				}
			}
		}
		
		public static double Dot(Vector3D a, Vector3D b) {
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}
		public static double Dot(Vector3 a, Vector3 b) {
			return (double)a.x * b.x + (double)a.y * b.y + (double)a.z * b.z;
		}
		public static Vector3D Cross(Vector3D a, Vector3D b) {
			return new Vector3D(
				a.y * b.z - a.z * b.y,
				a.z * b.x - a.x * b.z,
				a.x * b.y - a.y * b.x);
		}
		public static Vector3D Cross(Vector3 a, Vector3 b) {
			return new Vector3D(
				(double)a.y * b.z - (double)a.z * b.y,
				(double)a.z * b.x - (double)a.x * b.z,
				(double)a.x * b.y - (double)a.y * b.x);
		}
		public static double Distance(Vector3D a, Vector3D b) {
			var dx = a.x - b.x;
			var dy = a.y - b.y;
			var dz = a.z - b.z;
			return System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
		}
		
		public static Vector3D operator+(Vector3D a, Vector3D b) { return new Vector3D(a.x + b.x, a.y + b.y, a.z + b.z); }
		public static Vector3D operator-(Vector3D a, Vector3D b) { return new Vector3D(a.x - b.x, a.y - b.y, a.z - b.z); }
		public static Vector3D operator*(Vector3D a, double c) { return new Vector3D(c * a.x, c * a.y, c * a.z); }
		public static Vector3D operator*(double c, Vector3D a) { return new Vector3D(c * a.x, c * a.y, c * a.z); }
		
		public static implicit operator Vector3D(Vector3 vf) { return new Vector3D(vf.x, vf.y, vf.z); }
		public static explicit operator Vector3(Vector3D vd) { return new Vector3((float)vd.x, (float)vd.y, (float)vd.z); }			
	}

	
	public class Vector4D {
		public double x;
		public double y;
		public double z;
		public double w;
		
		public Vector4D() : this(0.0, 0.0, 0.0, 0.0) { }
		public Vector4D(double x, double y, double z, double w) {
			this.x = x; this.y = y; this.z = z; this.w = w;
		}
		
		public double this[int index] {
			get {
				switch (index) {
				case 0:
					return x;
				case 1:
					return y;
				case 2:
					return z;
				case 3:
					return w;
				default:
					throw new System.IndexOutOfRangeException();
				}
			}
			set {
				switch (index) {
				case 0:
					x = value;
					break;
				case 1:
					y = value;
					break;
				case 2:
					z = value;
					break;
				case 3:
					w = value;
					break;
				default:
					throw new System.IndexOutOfRangeException();
				}
			}
		}
	}
}