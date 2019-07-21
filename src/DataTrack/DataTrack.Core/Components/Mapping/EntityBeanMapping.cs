using DataTrack.Core.Attributes;
using DataTrack.Core.Exceptions;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Mapping
{
	internal class EntityBeanMapping<TBase> : Mapping where TBase : IEntityBean
	{
		internal EntityBeanMapping()
			: base(typeof(TBase))
		{
			AttributeWrapper wrapper = new AttributeWrapper(BaseType);

			foreach (EntityAttribute entityAttribute in wrapper.EntityAttributes)
			{
				MapEntity(entityAttribute.EntityType);
			}
		}
	}
}
