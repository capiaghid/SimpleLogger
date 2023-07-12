using System;
using System.Data;
using System.Data.Common;

using SimpleLogger.Logging.Module.Database;
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable IntroduceOptionalParameters.Global

namespace SimpleLogger.Logging.Module
{
    public class DatabaseLoggerModule : LoggerModule
    {
        private readonly string       _connectionString;
        private readonly DatabaseType _databaseType;
        private readonly string       _tableName;

        public DatabaseLoggerModule(DatabaseType databaseType, string connectionString)
            : this(databaseType, connectionString, "Log")
        {
        }

        public DatabaseLoggerModule(DatabaseType databaseType, string connectionString, string tableName)
        {
            _databaseType     = databaseType;
            _connectionString = connectionString;
            _tableName        = tableName;
        }

        public override string Name => DatabaseFactory.GetDatabaseName(_databaseType);

        [Obsolete("Obsolete")]
#pragma warning disable CS0809 // Veraltetes Element überschreibt nicht veraltetes Element
        public override void Initialize()
#pragma warning restore CS0809 // Veraltetes Element überschreibt nicht veraltetes Element
        {
            CreateTable();
        }

        private DbParameter GetParameter(DbCommand command, string name, object value, DbType type)
        {
            DbParameter parameter = command.CreateParameter();
            parameter.DbType        = type;
            parameter.ParameterName = (_databaseType.Equals(DatabaseType.Oracle) ? ":" : "@") + name;
            parameter.Value         = value;
            return parameter;
        }

        private void AddParameter(DbCommand command, string name, object value, DbType type)
        {
            command.Parameters.Add(GetParameter(command, name, value, type));
        }

        /// <inheritdoc />
        [Obsolete]
#pragma warning disable CS0809 // Veraltetes Element überschreibt nicht veraltetes Element
        public override void AfterLog(LogMessage logMessage)
#pragma warning restore CS0809 // Veraltetes Element überschreibt nicht veraltetes Element
        {
            using (DbConnection connection = DatabaseFactory.GetConnection(_databaseType, _connectionString))
            {
                connection.Open();
                string commandText = DatabaseFactory.GetInsertCommand(_databaseType, _tableName);
                DbCommand sqlCommand = DatabaseFactory.GetCommand(_databaseType, commandText, connection);

                AddParameter(sqlCommand, "text", logMessage.Text, DbType.String);
                AddParameter(sqlCommand, "dateTime", logMessage.DateTime, DbType.Date);
                AddParameter(sqlCommand, "log_level", logMessage.Level.ToString(), DbType.String);
                AddParameter(sqlCommand, "callingClass", logMessage.CallingClass, DbType.String);
                AddParameter(sqlCommand, "callingMethod", logMessage.CallingMethod, DbType.String);

                sqlCommand.ExecuteNonQuery();
            }
        }

        [Obsolete]
        private void CreateTable()
        {
            DbConnection connection = DatabaseFactory.GetConnection(_databaseType, _connectionString);

            using (connection)
            {
                connection.Open();

                DbCommand sqlCommand = DatabaseFactory.GetCommand
                (
                    _databaseType,
                    DatabaseFactory.GetCheckIfShouldCreateTableQuery(_databaseType),
                    connection
                );

                AddParameter(sqlCommand, "tableName", _tableName.ToLower(), DbType.String);

                object result = sqlCommand.ExecuteScalar();

                if (result == null)
                {
                    string commandText = string.Format(DatabaseFactory.GetCreateTableQuery(_databaseType), _tableName);
                    sqlCommand = DatabaseFactory.GetCommand(_databaseType, commandText, connection);
                    sqlCommand.ExecuteMultipleNonQuery();
                }
            }
        }
    }
}