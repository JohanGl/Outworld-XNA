using System;
using System.Collections.Generic;

namespace Framework.Core.Common
{
	public class Pool<T>
	{
		private readonly List<T> items;
		private readonly Queue<T> freeItems;
		private readonly Func<T> createItemAction;

		public List<T> Items
		{
			get
			{
				return items;
			}
		}

		public Pool(Func<T> createItemAction, int initialCapacity = 0)
		{
			items = new List<T>();
			freeItems = new Queue<T>();
			this.createItemAction = createItemAction;

			// Allocate the initial number of items within the pool
			for (int i = 0; i < initialCapacity; i++)
			{
				items.Add(createItemAction());
			}
		}

		public T GetFreeItem()
		{
			if (freeItems.Count == 0)
			{
				T item = createItemAction();
				items.Add(item);

				return item;
			}

			return freeItems.Dequeue();
		}

		public void ReleaseItem(T item)
		{
			freeItems.Enqueue(item);
		}

		public void Clear()
		{
			items.Clear();
			freeItems.Clear();
		}
	}
}