using System;
using System.Collections.Generic;
using System.Text;

namespace DataTrack.Core.Attributes
{
	public class EntityAttribute : Attribute
	{
		public Type EntityType { get; set; }

		public string EntityProperty { get; set; }

		public EntityAttribute(Type entityType, string entityProperty)
		{
			EntityType = entityType;
			EntityProperty = entityProperty;
		}
	}
}
