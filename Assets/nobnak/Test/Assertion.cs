using UnityEngine;
using System.Collections;

namespace nobnak.Test {
	
	public static class Assertion {
		public const double EPSILON = 1e-6;
		
		public static void Assert(double a, double b, string label) {
			var diff = a - b;
			if (diff < -EPSILON && EPSILON < diff) {
				Debug.Log(string.Format("{0} : a={1:e} b={2:e} diff={3:e}", label, a, b, diff));
			}
		}
	}
}