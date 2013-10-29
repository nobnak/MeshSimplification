using System.Collections.Generic;


namespace nobnak.Collection {

	public class BinaryHeap<T> {
		private IComparer<T> _comp;
		private List<T> _heap;
		
		public BinaryHeap(IComparer<T> comp) {
			this._comp = comp;
			this._heap = new List<T>();
		}
		
		public void Add(T val) {
			var child = _heap.Count;
			var parent = Parent(child);
			_heap.Add(val);
			while (parent >= 0 && _comp.Compare(_heap[parent], _heap[child]) > 0) {
				var tmp = _heap[parent]; _heap[parent] = _heap[child]; _heap[child] = tmp;
				child = parent;
				parent = Parent(parent);
			}
		}
		public T RemoveFront() {
			var result = _heap[0];
			var back = _heap.Count - 1;
			_heap[0] = _heap[back];
			_heap.RemoveAt(back);
			var parent = 0;
			var child = LeftChild(parent);
			while (child < _heap.Count) {
				if ((child + 1) < _heap.Count && _comp.Compare(_heap[child], _heap[child+1]) > 0)
					child = child + 1;					
				if (_comp.Compare(_heap[parent], _heap[child]) < 0)
					break;
				var tmp = _heap[parent]; _heap[parent] = _heap[child]; _heap[child] = tmp;
				parent = child;
				child = LeftChild(child);
			}
			return result;
		}
		public T Front { get { return _heap[0]; } }
		public int Count { get { return _heap.Count; } }
		public T this[int index] { get { return _heap[index]; } }
		
		public static int Parent(int child) {
			return (child - 1) >> 1;
		}
		public static int LeftChild(int parent) {
			return (parent << 1) + 1;
		}
	}
}
