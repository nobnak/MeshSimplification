using UnityEngine;
using System.Collections.Generic;

public class TestBinaryHeap : MonoBehaviour {

	// Use this for initialization
	void Start () {
		var list = new float[]{ 3, 1, 6, 4, 2, 9, 5, 8, 7 };
		var refHeap = new float[] { 1, 2, 5, 4, 3, 9, 6, 8, 7 };
		var refPriority = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
		var bh = new nobnak.Collection.BinaryHeap<float>(new FloatComp());
		for (var i = 0; i < list.Length; i++) {
			bh.Add(list[i]);
		}
		for (var i = 0; i < bh.Count; i++) {
			var val0 = bh[i];
			var val1 = bh[bh.Find(val0)];
			if (val0 != val1)
				Debug.Log(string.Format("Wrong Index : {0}!={1}", val0, val1));
		}
		
		bh[3] = 0;
		var front = bh.RemoveFront();
		if (front != 0)
			Debug.Log("Front should be 0 but " + front);
		bh.Add(4);
		
		for (var i = 0; i < refHeap.Length; i++) {
			if (bh[i] != refHeap[i])
				Debug.Log("Wrong Tree : " + i);
		}
		for (var i = 0; i < refPriority.Length; i++) {
			if (bh.RemoveFront() != refPriority[i])
				Debug.Log("Wrong priority : " + i);
		}
		if (bh.Count != 0)
			Debug.Log("Wrong List");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	class FloatComp : IComparer<float> {
		#region IComparer[System.Single] implementation
		public int Compare (float x, float y) {
			if (x < y)
				return -1;
			else if (x == y)
				return 0;
			else 
				return 1;
		}
		#endregion
	}
}
