using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.Repository
{
	public class EntityBeanRepository<TBase> : IEntityBeanRepository<TBase> where TBase : IEntityBean
	{
		public List<TBase> GetAll()
		{
			return new EntityBeanQuery<TBase>().Execute();
		}

		public List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue)
		{
			Type propType = propValue.GetType();

			EntityBeanQuery<TBase> query = new EntityBeanQuery<TBase>();

			MethodInfo addRestriction;
			addRestriction = query.GetType().GetMethod("AddRestriction", BindingFlags.Instance | BindingFlags.Public);
			addRestriction.Invoke(query, new object[] { propName, restriction, propValue });

			return query.Execute();
		}
	}
}
