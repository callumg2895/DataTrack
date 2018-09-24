using DataTrack.Core.Attributes;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using DataTrack.Core.Util.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;

namespace DataTrack.Core.SQL
{
    public class Transaction<T> : ITransaction<T>
    {

        private QueryBuilder<T> queryBuilder;
        private List<(string Handle, object Value)> parameters;

        public Transaction(QueryBuilder<T> queryBuilder)
        {
            this.queryBuilder = queryBuilder;
            this.parameters = queryBuilder.GetParameters();
        }

        public void Execute()
        {
            using (SqlConnection connection = DataTrackConfiguration.CreateConnection())
            {
                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = queryBuilder.ToString();

                    command.AddParameters(parameters);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
