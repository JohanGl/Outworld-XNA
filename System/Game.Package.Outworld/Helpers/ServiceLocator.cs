using System;
using System.Collections.Generic;

namespace Game.Package.Outworld.Helpers
{
	public class ServiceA : IService
	{
		public void doA()
		{
		}
	}

	public class ServiceB : IService
	{
		public void doB()
		{
		}
	}

	public class ServiceLocator
	{
		private static readonly Dictionary<Type, object> services;

		static ServiceLocator()
		{
			services = new Dictionary<Type, object>();
		}

		public static T Get<T>() where T : class
		{
			var key = typeof(T);

			if (services.ContainsKey(key))
			{
				return (T)services[key];
			}

			return default(T);
		}

		public static void Set<T>(object service) where T : class
		{
			var key = typeof(T);

			if (services.ContainsKey(key))
			{
				services[key] = service;
			}
			else
			{
				services.Add(key, service);
			}
		}

		public static void Clear()
		{
			services.Clear();
		}
	}
}