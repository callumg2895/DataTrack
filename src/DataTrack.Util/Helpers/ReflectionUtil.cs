using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Util.Helpers
{
	public static class ReflectionUtil
	{
		public static object GetPropertyValue(object instance, string propertyName)
		{
			return instance.GetType().GetProperty(propertyName).GetValue(instance);
		}
	}
}
