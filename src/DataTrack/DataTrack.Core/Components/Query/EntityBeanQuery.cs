using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Interface;
using DataTrack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Components.Query
{
	public class EntityBeanQuery<TBase> : Query where TBase : IEntityBean
	{
		internal EntityBeanMapping<TBase> Mapping { get; set; }

		public EntityBeanQuery()
			: base(typeof(TBase))
		{
			Mapping = new EntityBeanMapping<TBase>();

			ValidateMapping(Mapping);
		}
	}
}
