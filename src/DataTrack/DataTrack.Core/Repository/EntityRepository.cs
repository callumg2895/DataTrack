﻿using DataTrack.Core.Components.Query;
using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace DataTrack.Core.Repository
{
	public class EntityRepository<TBase> : IEntityRepository<TBase> where TBase : IEntity
	{
		#region Create

		public void Create(TBase item)
		{
			new EntityQuery<TBase>()
				.Create(item)
				.Execute();
		}

		public void Create(List<TBase> items)
		{
			new EntityQuery<TBase>()
				.Create(items)
				.Execute();
		}

		#endregion

		#region Read

		public List<TBase> GetAll()
		{
			return new EntityQuery<TBase>()
				.Read()
				.Execute();
		}

		public TBase GetByID(int id)
		{
			return new EntityQuery<TBase>()
				.AddRestriction("ID", RestrictionTypes.EqualTo, id)
				.Execute()[0];
		}

		public List<TBase> GetByProperty(string propName, RestrictionTypes restriction, object propValue)
		{
			return new EntityQuery<TBase>()
				.Read()
				.AddRestriction(propName, restriction, propValue)
				.Execute();
		}

		#endregion

		#region Update

		public int Update(TBase item)
		{
			return new EntityQuery<TBase>()
				.Update(item)
				.Execute();
		}


		#endregion

		#region Delete

		public void Delete(TBase item)
		{
			new EntityQuery<TBase>()
				.Delete(item)
				.Execute();
		}

		public void DeleteAll()
		{
			new EntityQuery<TBase>()
				.Delete()
				.Execute();
		}

		#endregion
	}
}
