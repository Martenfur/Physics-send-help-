using System;

namespace PSH.Physics
{
	/// <summary>
	/// Object pool, but you can only take the objects one by one.
	/// If you want to return objects back, you must do it for all objects at once. 
	/// </summary>
	public class BatchReturnPool<T>
	{
		/// <summary>
		/// Pool size including taken and non-taken objects.
		/// </summary>
		public int Size => _pool.Length;
		
		public int TakenObjectsCount => _pointer;
		
		T[] _pool;
		private int _pointer;

		public BatchReturnPool(int size)
		{
			_pool = new T[size];
			for(var i = 0; i < size; i += 1)
			{
				_pool[i] = Activator.CreateInstance<T>();
			}
			_pointer = 0;
		}

		/// <summary>
		/// Marks a pool object as taken and returns an index to it.
		/// If pool runs out of objects, it doubles its size.
		/// </summary>
		public int Take()
		{
			_pointer += 1;
			if (_pointer >= _pool.Length)
			{
				DoublePoolSize();
			}
			return _pointer - 1;
		}


		/// <summary>
		/// Returns ALL previously taken objects back to the pool.
		/// </summary>
		public int ReturnAll()
		{
			_pointer = 0;
			return _pointer;
		}


		/// <summary>
		/// Tells if object at specified index is already taken.
		/// </summary>
		public bool IsTaken(int index) =>
			_pointer > index;
		
		
		/// <summary>
		/// Returns a reference to an object in the pool.
		/// </summary>
		public ref T this[int index] =>
			ref _pool[index];
		
		
		protected void DoublePoolSize()
		{
			var oldSize = _pool.Length;
			Array.Resize(ref _pool, oldSize * 2);

			for (var i = oldSize; i < _pool.Length; i += 1)
			{
				_pool[i] = Activator.CreateInstance<T>();
			}
		}

	}
}
