using UnityEngine;
using System.Collections;
using nobnak.Algebra;
using System.Text;

public class TestLU : MonoBehaviour {
	
	void Start() {
		Test01();
		Test02();
	}
	
	void Test02() {
		var A = new float[]{
			0, 1, 2,
			2, 0, 1,
			1, 2, 0
		};
		var lu = new LU(A, 3);
		var refLu = new float[] {
			2f,		0f,		1f,
			1f/2,	2f,		-1f/2,
			0f,		1f/2,	9f/4
		};
		AssertLU(lu, refLu);
		
		var b = new float[] { 8, 5, 5 };
		var x = new float[3];
		var refX = new float[] { 1, 2, 3 };
		lu.Solve(b, ref x);
		AssertX(x, refX);
	}
	
	void Test01() {
		var A = new float[]{
			1, 2, 3, 4,
			2, 3, 4, 1, 
			3, 4, 1, 2,
			4, 1, 2, 3,
		};
		var lu = new LU(A, 4);
		
		var b = new float[]{ 16, 14, 16, 14 };
		var x = new float[4];
		lu.Solve(b, ref x);
		var refX = new float[] { 1, 2, 1, 2 };
		AssertX (x, refX);
	}

	void AssertLU (LU lu, float[] refLU) {
		for (var i = 0; i < refLU.Length; i++) {
			var x = i % 3;
			var y = i / 3;
			Assert (lu.lu[i], refLU[i], string.Format("LU_ij={0},{1}", x, y));
		}
	}

	void AssertX (float[] b, float[] refX) {
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
