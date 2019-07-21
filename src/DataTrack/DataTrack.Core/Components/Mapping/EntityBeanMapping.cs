using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Logging;
using DataTrack.Util.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	internal class EntityBeanMapping<TBase> : Mapping where TBase : IEntityBean
	{
		List<Column> Columns { get; set; }
		Dictionary<string, List<Column>> PropertyMapping {get; set;}

		internal EntityBeanMapping()
			: base(typeof(TBase))
		{
			Columns = new List<Column>();
			PropertyMapping = new Dictionary<string, List<Column>>();

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
			List<string> entityProperties = entityAttribute.EntityProperty.Replace(" ", "").Split(',').ToList();
			PropertyInfo beanProperty = ReflectionUtil.GetProperty(BaseType, entityAttribute) ?? throw new NullReferenceException();

			PropertyMapping.Add(beanProperty.Name, new List<Column>());

			foreach (string entityProperty in entityProperties)
			{
				List<Column> columns = TypeTableMapping[entityAttribute.EntityType].Columns.Where(c => c.PropertyName == entityProperty).ToList();

				Columns.AddRange(columns);
				PropertyMapping[beanProperty.Name].AddRange(columns);
			}
		}
	}
}
