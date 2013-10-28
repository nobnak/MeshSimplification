using UnityEngine;
using System.Collections;

namespace nobnak.Test {
	
	public static class Assertion {
		public const float EPSILON = 1e-6f;
		
		public static void Assert(float a, float b, string label) {
			var diff = a - b;
			if (diff < -EPSILON && EPSILON < diff) {
				Debug.Log(string.Format("{0} : a={1:e} b={2:e} diff={3:e}", label, a, b, diff));
			}
		}		
	}
}