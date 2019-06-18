using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Extensions
{
	public static class DebugConditional
	{
		[Conditional("FULL_LOG")]
		public static void Log(string message)
		{
			Debug.Log(message);
		}
		
		[Conditional("FULL_LOG")]
		public static void LogFormat(string format, params object[] args)
		{
			Debug.LogFormat(format, args);
		}
		
		[Conditional("FULL_LOG")]
		public static void LogFormat(Object context, string format, params object[] args)
		{
			Debug.LogFormat(context, format, args);
		}
	}
}