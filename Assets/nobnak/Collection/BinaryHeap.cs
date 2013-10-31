using System.Collections.Generic;


namespace nobnak.Collection {

	public class BinaryHeap<T> {
		private IComparer<T> _comp;
		private IndexedList<T> _heap;
		
		public BinaryHeap(IComparer<T> comp) {
			this._comp = comp;
			this._heap = new IndexedList<T>();
		}
		
		public void Add(T val) {
			_heap.Add(val);
			Ascend(_heap.Count - 1);
		}
		public T RemoveFront() {
			var result = _heap[0];
			var back = _heap.Count - 1;
			_heap[0] = _heap[back];
			_heap.RemoveLast();
			Descend(0);
			return result;
		}
		public int Find(T val) {
			return _heap.Find(val);
		}
		public void Update(int i, T val) {
			_heap[i] = val;
			var parent = Parent(i);
			if (parent >= 0 && _comp.Compare(_heap[parent], _heap[i]) > 0)
				Ascend(i);
			else
				Descend(i);
		}
		
		public T Front { get { return _heap[0]; } }
		public int Count { get { return _heap.Count; } }
		public T this[int index] { 
			get { return _heap[index]; } 
			set{ Update(index, value); }
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
		
		public class IndexedList<T> {
			private Dictionary<T, int> _index;
			private List<T> _list;
			
			public IndexedList() {
				_index = new Dictionary<T, int>();
				_list = new List<T>();
			}
			
			public T this[int i]{
				get {
					return _list[i];
				}
				set {
					_index[value] = i;
					_list[i] = value;
				}
			}
			public void Add(T val) {
				_index[val] = _list.Count;
				_list.Add(val);
			}
			public T RemoveLast() {
				var last = _list[_list.Count - 1];
				_index.Remove(last);
				_list.RemoveAt(_list.Count - 1);
				return last;
			}
			public int Find(T val) {
				return _index[val];
			}
			
			public int Count { get { return _list.Count; } }
		}
	}
}
