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

		public override bool Equals(object obj)
		{
			EntityAttribute? attribute = obj as EntityAttribute;

			if (attribute == null)
			{
				return false;
			}

			return attribute.EntityType == EntityType && attribute.EntityProperty == EntityProperty;
		}
	}
}
