using System;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Core.Diagnostics.Logging
{
	/// <summary>
	/// Logger which enables type-sensitive logging functionality and multiple output sources
	/// </summary>
	public static partial class Logger
	{
		public static bool OnlyLogRegisteredTypes;

		private static readonly DateTime Started = DateTime.Now;
		private static Dictionary<string, LogHandlerItem> register;

		/// <summary>
		/// Gets or sets the list of available output sources
		/// </summary>
		public static List<IOutputSource> Outputs { get; set; }

		static Logger()
		{
			register = new Dictionary<string, LogHandlerItem>();
			Outputs = new List<IOutputSource>() { new DebugOutputSource() };
			OnlyLogRegisteredTypes = false;
		}

		private static string GetTypeKey<T>()
		{
			return Convert.ToString(typeof(T).Name);
		}

		/// <summary>
		/// Sets the loglevel filters of a specific type
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="levels">The log level filter</param>
		public static void RegisterLogLevelsFor<T>(params LogLevel[] levels)
		{
			string key = GetTypeKey<T>();

			if (!register.ContainsKey(key))
			{
				register.Add(key, new LogHandlerItem(levels.ToList()));
			}
			else
			{
				register[key].Levels = levels.ToList();
			}
		}

		/// <summary>
		/// Removes the loglevel filters of a specific type
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		public static void UnRegisterLogLevelsFor<T>()
		{
			string key = GetTypeKey<T>();

			if (register.ContainsKey(key))
			{
				register.Remove(key);
			}
		}

		/// <summary>
		/// Removes all registered types
		/// </summary>
		public static void Clear()
		{
			register.Clear();
		}

		#region Logging

		public static void Log<T>(LogLevel level, string message, params object[] args)
		{
			string key = GetTypeKey<T>();

			if (register.ContainsKey(key))
			{
				if (!register[key].IsMuted &&
					register[key].Levels.Contains(level))
				{
					Log(key, string.Format(message, args));
				}
			}
			else if (!OnlyLogRegisteredTypes)
			{
				Log(key, string.Format(message, args));
			}
		}

		private static void Log(string key, string message)
		{
			var span = (DateTime.Now - Started);
			string text = string.Format("[{0:00}:{1:00}:{2:00}][{3}] {4}", span.Hours, span.Minutes, span.Seconds, key, message);
			Outputs.ForEach(p => p.Write(text));
		}

		#endregion

		#region Mute

		/// <summary>
		/// Mutes or unmutes a specific type
		/// </summary>
		/// <typeparam name="T">The type</typeparam>
		/// <param name="state">Mute state</param>
		public static void Mute<T>(bool state)
		{
			string key = GetTypeKey<T>();

			if (!register.ContainsKey(key))
			{
				register.Add(key, new LogHandlerItem());
			}

			register[key].IsMuted = state;
		}

		/// <summary>
		/// Mutes or unmutes all types
		/// </summary>
		/// <param name="state">Mute state</param>
		public static void Mute(bool state)
		{
			foreach (var item in register.Values)
			{
				item.IsMuted = state;
			}
		}

		#endregion
	}
}