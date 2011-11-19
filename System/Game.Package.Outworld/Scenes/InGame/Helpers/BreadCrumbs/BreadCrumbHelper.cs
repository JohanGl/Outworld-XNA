using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Outworld.Scenes.InGame.Helpers.BreadCrumbs
{
	public class BreadCrumbHelper
	{
		public List<BreadCrumb> BreadCrumbs { get; private set; }
		private readonly int capacity;

		public BreadCrumbHelper(int capacity)
		{
			this.capacity = capacity;

			BreadCrumbs = new List<BreadCrumb>(capacity + 4);
		}

		public void Add(Vector3 position, Vector3 angle)
		{
			// Skip adding this breadcrumb if its position is too close (or the same) as the previous one
			if (BreadCrumbs.Count > 0 && GetDistanceToLastBreadCrumb(position) < 1)
			{
				return;
			}

			// Initialize the new breadcrumb
			var breadCrumb = new BreadCrumb
			{
				Position = position,
				Angle = angle
			};

			// Remove the oldest entry if we have reached the max capacity for breadcrumb storage
			if (BreadCrumbs.Count == capacity)
			{
				BreadCrumbs.RemoveAt(0);
			}

			// Add the new breadcrumb
			BreadCrumbs.Add(breadCrumb);
		}

		public void Clear()
		{
			BreadCrumbs.Clear();
		}

		private float GetDistanceToLastBreadCrumb(Vector3 position)
		{
			var lastPosition = BreadCrumbs[BreadCrumbs.Count - 1].Position;
			return Vector3.Distance(position, lastPosition);
		}

		public BreadCrumb GetLastBreadCrumb()
		{
			if (BreadCrumbs.Count > 0)
			{
				return BreadCrumbs[BreadCrumbs.Count - 1];
			}

			return null;
		}

		public void RemoveLastBreadCrumb()
		{
			if (BreadCrumbs.Count > 0)
			{
				BreadCrumbs.RemoveAt(BreadCrumbs.Count - 1);
			}
		}
	}
}