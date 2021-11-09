using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ariane
{
	internal static class Extensions
	{
		public static bool IsDynamicPropertyExists(this System.Dynamic.ExpandoObject obj, string propertyName)
		{
			if (obj == null)
			{
				return false;
			}
			return ((IDictionary<String, object>)obj).ContainsKey(propertyName);
		}

		public static string ToJsonStringTraceLog(this object message)
		{
			if (message == null)
			{
				return string.Empty;
			}

			string result = string.Empty;
			try
			{
				result = System.Text.Json.JsonSerializer.Serialize(message);
			}
			catch { /* */ }
			return result;
		}
	}
}
