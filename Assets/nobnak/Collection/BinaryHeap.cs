using System.Collections.Generic;


namespace nobnak.Collection {

	public class BinaryHeap<T> : IEnumerable<T> {
		private IComparer<T> _comp;
		private List<T> _heap;
		
		public BinaryHeap(IComparer<T> comp) {
			this._comp = comp;
			this._heap = new List<T>();
		}
		
		public void Add(T val) {
			_heap.Add(val);
			Ascend(_heap.Count - 1);
		}
		public T RemoveFront() {
			var result = _heap[0];
			var back = _heap.Count - 1;
			_heap[0] = _heap[back];
			_heap.RemoveAt(back);
			Descend(0);
			return result;
		}
		public T Remove(int i) {
			var removed = _heap[i];
			if (i == _heap.Count - 1) {
				_heap.RemoveAt(i);
				return removed;
			}
			
			_heap[i] = _heap[_heap.Count-1];
			_heap.RemoveAt(_heap.Count-1);
			Descend(i);
			
			return removed;
		}
		public int Find(T query) {
			return Find (query, 0);
		}
		public int Find(T query, int parent) {
			var compared = _comp.Compare(query, _heap[parent]);
			if (compared == 0)
				return parent;
			if (compared < 0)
				return -1;
			
			var left = LeftChild(parent);
			for (var i = 0; i < 2; i++) {
				var child = left + i;
				if (child < _heap.Count) {
					var found = Find (query, child);
					if (found >= 0)
						return found;
				}
			}
			return -1;
		}
		
		public T Front { get { return _heap[0]; } }
		public int Count { get { return _heap.Count; } }
		public T this[int index] { 
			get { return _heap[index]; } 
		}

		void Ascend (int child) {
			var parent = Parent(child);
			while (parent >= 0 && _comp.Compare(_heap[parent], _heap[child]) > 0) {
				var tmp = _heap[parent]; _heap[parent] = _heap[child]; _heap[child] = tmp;
				child = parent;
				parent = Parent(parent);
			}
		}

		void Descend (int parent) {
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
		}
		
		public static int Parent(int child) {
			return (child - 1) >> 1;
		}
		public static int LeftChild(int parent) {
			return (parent << 1) + 1;
		}

		#region IEnumerable[T] implementation
		public IEnumerator<T> GetEnumerator () {
			return _heap.GetEnumerator();
		}
		#endregion

		#region IEnumerable implementation
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator () {
			return _heap.GetEnumerator();
		}
		#endregion
	}
}
