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
	}
}
