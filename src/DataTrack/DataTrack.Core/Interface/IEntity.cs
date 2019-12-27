﻿using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
	public interface IEntity
	{
		object GetID();

		void SetID(dynamic value);

		object GetPropertyValue(string propertyName);

		List<object> GetPropertyValues();

		dynamic GetChildPropertyValues(string tableName);

		void InstantiateChildProperties();

		void AddChildPropertyValue(string tableName, IEntity entity);
	}
}
