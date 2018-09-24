using DataTrack.Core.Enums;
using DataTrack.Core.Interface;
using DataTrack.Core.Util;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Xml;

namespace DataTrack.Core
{

    public static class DataTrackConfiguration
    {

        #region Members

        public static string ConnectionString = string.Empty;

        #endregion

        #region Methods

        public static void Init() => Init(OutputTypes.None, ConfigType.Manual, null);

        public static void Init(OutputTypes outputType, ConfigType configType, string connection)
        {
            Logger.Init(outputType);

            switch (configType)
            {
                case ConfigType.FilePath:
                    ConnectionString = GetConnectionString(connection);
                    Logger.Info(MethodBase.GetCurrentMethod(), $"Set database connection string '{ConnectionString}'");
                    break;
                case ConfigType.ConnectionString:
                    ConnectionString = connection;
                    Logger.Info(MethodBase.GetCurrentMethod(), $"Set database connection string '{ConnectionString}'");
                    break;
                case ConfigType.Manual:
                    Logger.Warn(MethodBase.GetCurrentMethod(), "Database connection string must be set manually");
                    break;
            }

        }

        public static SqlConnection CreateConnection()
        {

            SqlConnection connection = new SqlConnection();

            if (!string.IsNullOrEmpty(ConnectionString))
            {
                connection.ConnectionString = ConnectionString;
                connection.Open();
                Logger.Info(MethodBase.GetCurrentMethod(), "Successfully opened new SQL connection");
            }
            else
                Logger.Warn(MethodBase.GetCurrentMethod(), "Failed to open new SQL connection - connection string not supplied");

            return connection;
        }

        private static string GetConnectionString(string configPath)
        {

            XmlDocument doc = new XmlDocument();
            string xPath = "dbconfig/connection";
            string xPathAttr = "DataTrack";

            try
            {
                doc.Load(configPath);
                
                foreach (XmlNode node in doc.SelectNodes(xPath))
                    if (node.Attributes[0].Value == xPathAttr)
                        return new SqlConnectionStringBuilder()
                        {
                            DataSource = node.Attributes[1].Value,
                            InitialCatalog = node.Attributes[2].Value,
                            UserID = node.Attributes[3].Value,
                            Password = node.Attributes[4].Value
                        }.ToString();

                throw new Exception($"Could not find node '{xPath}' with an attribute 'type' with value '{xPathAttr}' in file: {configPath}");
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), e.Message);
                return string.Empty;
            }


        }

        #endregion
    }
}
