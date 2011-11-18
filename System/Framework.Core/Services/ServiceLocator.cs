using System;
using System.Collections.Generic;

namespace Framework.Core.Services
{
	/// <summary>
	/// Helper class which should be used to store and retrieve all global objects within the system
	/// </summary>
	public static class ServiceLocator
	{
		private static readonly Dictionary<Type, object> services;

		static ServiceLocator()
		{
			services = new Dictionary<Type, object>();
		}

		public static void Register<T>(T service) where T : class
		{
			var type = typeof(T);
			services.Remove(type);
			services.Add(type, service);
		}

		public static T Get<T>() where T : class
		{
			var type = typeof(T);

			if (services.ContainsKey(type))
			{
				return (T)services[type];
			}

			return default(T);
		}

		public static void Remove<T>()
		{
			var type = typeof(T);
			services.Remove(type);
		}

		public static void Clear()
		{
			services.Clear();
		}
	}
}