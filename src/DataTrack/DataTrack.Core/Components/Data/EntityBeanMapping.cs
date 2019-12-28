using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Data
{
	internal class EntityBeanMapping<TBase> : Mapping where TBase : IEntityBean
	{
		internal List<Column> Columns { get; set; }
		internal Dictionary<string, Column> PropertyMapping {get; set;}

		internal EntityBeanMapping()
			: base(typeof(TBase))
		{
			Columns = new List<Column>();
			PropertyMapping = new Dictionary<string, Column>();

			MapBean();
		}

		private void MapBean()
		{
			AttributeWrapper wrapper = new AttributeWrapper(BaseType);

			foreach (EntityAttribute entityAttribute in wrapper.EntityAttributes)
			{
				MapEntity(entityAttribute.EntityType);
				MapToBean(entityAttribute);
			}
		}

		private void MapToBean(EntityAttribute entityAttribute)
		{
			string entityProperty = entityAttribute.EntityProperty;
			PropertyInfo beanProperty = ReflectionUtil.GetProperty(BaseType, entityAttribute) ?? throw new NullReferenceException();
			Column? column = TypeTableMapping[entityAttribute.EntityType].Columns.Where(c => c.PropertyName == entityProperty).FirstOrDefault();

			if (column != null)
			{
				Columns.Add(column);
				PropertyMapping.Add(beanProperty.Name, column);
			}
		}
	}
}
