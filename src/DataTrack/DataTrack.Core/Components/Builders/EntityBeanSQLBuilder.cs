using DataTrack.Core.Components.Mapping;
using DataTrack.Core.Components.Query;
using DataTrack.Core.Components.SQL;
using DataTrack.Core.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataTrack.Core.Components.Builders
{
	internal class EntityBeanSQLBuilder<TBase> : SQLBuilder where TBase : IEntityBean
	{
		internal EntityBeanSQLBuilder(EntityBeanMapping<TBase> mapping)
			: base(typeof(TBase), mapping)
		{

		}
	}
}
