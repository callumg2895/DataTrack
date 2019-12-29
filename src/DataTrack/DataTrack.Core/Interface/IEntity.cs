using DataTrack.Core.Components.Data;
using System.Collections.Generic;

namespace DataTrack.Core.Interface
{
	public interface IEntity
	{
		internal List<IEntity> GetChildren(Mapping mapping);

		internal void MapChild(IEntity entity, Mapping mapping);

		object GetID();

		void SetID(dynamic value);

		object GetPropertyValue(string propertyName);

		List<object> GetPropertyValues();

		dynamic GetChildPropertyValues(string tableName);

		void InstantiateChildProperties();

		void AddChildPropertyValue(string tableName, IEntity entity);
	}
}
