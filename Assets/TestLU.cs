using UnityEngine;
using System.Collections;
using nobnak.Algebra;

public class TestLU : MonoBehaviour {

	void Start () {
		var A = new float[]{
			1, 2, 3,
			2, 3, 1,
			3, 1, 2
		};
		var lu = new LU(A, 3);
		
		var refLU = new float[]{
			1, 2, 3,
			2, -1, -5,
			3, 5, 18
		};
		for (var i = 0; i < refLU.Length; i++) {
			var x = i % 3;
			var y = i / 3;
			Assert (lu.lu[i], refLU[i], string.Format("A_ij={0},{1}", x, y));
		}

		var b = new float[]{ 6, 6, 6 };
		lu.Solve(b);
		var refX = new float[] { 1, 1, 1 };
		for (var i = 0; i < refX.Length; i++) {
			Assert(b[i], refX[i], string.Format("x_i={0}", i));
		}
	}
	
	void Assert(float a, float b, string label) {
		var diff = a - b;
		if (diff < -1e-6 || 1e-6 < diff)
			Debug.Log(string.Format("{0} : a={1:e2} b={2:e2}", label, a, b));
	}
}
