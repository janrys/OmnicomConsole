using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Infrastructure.Persistence.Settings;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AngularCrudApi.Infrastructure.Persistence.Repositories
{
    public class SqlDatabaseCodebookRepository : ICodebookRepository
    {
        private const string RDS_TABLE_PREFIX = "CB";
        private const string ADMIN_CONSOLE_TABLE_PREFIX = "CodebookConsole";
        private const string CONFIGURATION_TABLE_SCHEME = "dbo";
        private const string CONFIGURATION_TABLE_NAME = "CodebookConsoleConfiguration";
        private const string CONFIGURATION_KEY_NAME_LOCK = "Lock";
        private readonly SqlDatabaseSettings settings;
        private readonly ILogger<SqlDatabaseCodebookRepository> log;

        public SqlDatabaseCodebookRepository(IOptions<SqlDatabaseSettings> settings, ILogger<SqlDatabaseCodebookRepository> log)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        }

        private SqlConnection GetSqlConnetion() => new SqlConnection(this.settings.ConnectionString);
        private Task<SqlConnection> GetOpenedSqlConnetion() => this.GetOpenedSqlConnetion(CancellationToken.None);
        private async Task<SqlConnection> GetOpenedSqlConnetion(CancellationToken cancellationToken)
        {
            SqlConnection connection = null;

            try
            {
                connection = this.GetSqlConnetion();
                await connection.OpenAsync(cancellationToken);
            }
            catch (Exception exception)
            {
                this.log.LogError($"Cannot create and open sql connection with connection string {this.settings.ConnectionString}", exception);
            }

            return connection;
        }

        public async Task<IEnumerable<Codebook>> GetAll(bool includeRds)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                List<Codebook> codebooks = new List<Codebook>();
                string commandText = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                commandText += $" AND NOT TABLE_NAME like '{ADMIN_CONSOLE_TABLE_PREFIX}%'";

                if (!includeRds)
                {
                    commandText += $" AND NOT TABLE_NAME like '{RDS_TABLE_PREFIX}%'";
                }

                IEnumerable<TableName> tableNames = await sqlConnection.QueryAsync<TableName>(commandText);
                return tableNames.Select(t => (Codebook)t);
            }
        }

        public async Task<CodebookDetail> GetByName(string codebookName)
        {
            if (String.IsNullOrEmpty(codebookName))
            {
                return null;
            }

            if (codebookName.Contains('.'))
            {
                codebookName = this.GetTableName(codebookName);
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                CodebookDetail codebookDetail = null;
                string commandText = @"
                    SELECT c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.IS_NULLABLE, c.DATA_TYPE, c.CHARACTER_MAXIMUM_LENGTH, COLUMNPROPERTY(object_id(c.TABLE_SCHEMA+'.'+c.TABLE_NAME), c.COLUMN_NAME, 'IsIdentity') IS_IDENTITY, IIF(CU.ORDINAL_POSITION IS NULL, 0, 1) IS_PRIMARY
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU on c.TABLE_SCHEMA = CU.TABLE_SCHEMA AND c.TABLE_NAME = CU.TABLE_NAME AND c.COLUMN_NAME = CU.COLUMN_NAME
                    WHERE c.TABLE_NAME = @tableName";
                IEnumerable<TableColumnDetail> tableColumnDetail = await sqlConnection.QueryAsync<TableColumnDetail>(commandText, new { tableName = codebookName });

                if (tableColumnDetail != null && tableColumnDetail.Any())
                {
                    codebookDetail = new CodebookDetail();
                    codebookDetail.Name = tableColumnDetail.First().TABLE_NAME;
                    codebookDetail.Scheme = tableColumnDetail.First().TABLE_SCHEMA;
                    codebookDetail.Columns = new List<ColumnDefinition>();
                    codebookDetail.Columns.AddRange(tableColumnDetail.Select(c => (ColumnDefinition)c));
                }

                return codebookDetail;
            }
        }

        public async Task<CodebookDetailWithData> GetData(string codebookName)
        {
            if (String.IsNullOrEmpty(codebookName))
            {
                return null;
            }

            if (codebookName.Contains('.'))
            {
                codebookName = this.GetTableName(codebookName);
            }

            CodebookDetail codebookDetail = await this.GetByName(codebookName);

            if (codebookDetail == null)
            {
                return null;
            }

            CodebookDetailWithData codebookDetailWithData = new CodebookDetailWithData(codebookDetail);

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = $"SELECT * FROM {codebookDetailWithData.FullName}";
                codebookDetailWithData.Data = (await sqlConnection.QueryAsync(commandText)).ToList();
                return codebookDetailWithData;
            }

        }

        private string GetTableName(string tableName) => tableName.Split('.').Last().Trim().Trim('[').Trim(']');
        private string ConfigurationTableFullName => $"{CONFIGURATION_TABLE_SCHEME}.{CONFIGURATION_TABLE_NAME}";

        public async Task<LockState> GetLock()
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = $"SELECT * FROM {this.ConfigurationTableFullName} WHERE [Key] = @keyName";
                Configuration configuration = await sqlConnection.QueryFirstOrDefaultAsync<Configuration>(commandText, new { keyName = CONFIGURATION_KEY_NAME_LOCK });

                LockState lockState;
                if (configuration == null || String.IsNullOrEmpty(configuration.Value))
                {
                    lockState = new LockState() { IsLocked = false, ForUserId = "", ForUserName = "" };
                }
                else
                {
                    lockState = JsonConvert.DeserializeObject<LockState>(configuration.Value);
                }

                return lockState;
            }
        }

        public Task<LockState> CreateLock(string userIdentifier, string userName, int releaseId, DateTime? created = null)
            => this.UpsertLock(userIdentifier, userName, releaseId, created);

        public Task<LockState> ReleaseLock(DateTime? released = null) => this.UpsertLock("", "");

        private async Task<LockState> UpsertLock(string userIdentifier, string userName, int? releaseId = null, DateTime? created = null)
        {
            if (!created.HasValue)
            {
                created = DateTime.UtcNow;
            }

            if (!releaseId.HasValue)
            {
                releaseId = -1;
            }

            Boolean isLocked = !String.IsNullOrEmpty(userIdentifier);

            LockState lockState = new LockState() { ForUserId = userIdentifier, ForUserName = userName, Created = created.Value, IsLocked = isLocked, ForReleaseId = releaseId.Value };
            string lockValue = JsonConvert.SerializeObject(lockState);

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                StringBuilder upsertCommand = new StringBuilder();
                upsertCommand.AppendLine("BEGIN TRANSACTION;");
                upsertCommand.AppendLine($"UPDATE {this.ConfigurationTableFullName} WITH (UPDLOCK, SERIALIZABLE) SET [Value] = @value WHERE [Key] = @key;");
                upsertCommand.AppendLine("IF @@ROWCOUNT = 0");
                upsertCommand.AppendLine("BEGIN");
                upsertCommand.AppendLine($"   INSERT {this.ConfigurationTableFullName}([Key], [Value]) VALUES(@key, @value);");
                upsertCommand.AppendLine("END");
                upsertCommand.AppendLine("COMMIT TRANSACTION;");

                await sqlConnection.ExecuteAsync(upsertCommand.ToString(), new { key = CONFIGURATION_KEY_NAME_LOCK, value = lockValue });
            }

            return lockState;
        }

        public Task<CodebookDetailWithData> InsertData(string codebookName, int releaseId, IDictionary<string, object> values)
        {
            return Task.FromResult((CodebookDetailWithData)null);
        }

        public Task UpdateData(string codebookName, int releaseId, object key, IDictionary<string, object> values)
        {
            return Task.CompletedTask;
        }

        public Task DeleteData(string codebookName, int releaseId, object key)
        {
            return Task.CompletedTask;
        }

        private Task SaveReleaseChange(ReleaseChange releaseChange)
        {
            return Task.CompletedTask;
        }

        private string AssembleSqlCommand()
        {
            return "";
        }

        public async Task<IEnumerable<Release>> GetAllReleases()
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRelease";
                IEnumerable<Release> releases = await sqlConnection.QueryAsync<Release>(commandText);
                return releases;
            }
        }

        public async Task<IEnumerable<Request>> GetRequests(int releaseId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRequest WHERE ReleaseId = @id";
                IEnumerable<Request> requests = await sqlConnection.QueryAsync<Request>(commandText, new { id = releaseId });
                return requests;
            }
        }

        private class TableName
        {
            public string TABLE_SCHEMA { get; set; }
            public string TABLE_NAME { get; set; }

            public static explicit operator Codebook(TableName tableName) => new Codebook() { Name = tableName.TABLE_NAME, Scheme = tableName.TABLE_SCHEMA };
        }

        private class TableColumnDetail : TableName
        {
            public string COLUMN_NAME { get; set; }

            /// <summary>
            /// Nullability of the column. If this column allows for NULL, this column returns YES. Otherwise, NO is returned.
            /// </summary>
            public string IS_NULLABLE { get; set; }

            public string DATA_TYPE { get; set; }
            public int CHARACTER_MAXIMUM_LENGTH { get; set; }

            public bool IS_IDENTITY { get; set; }
            public bool IS_PRIMARY { get; set; }

            public Boolean IsNullable => !String.IsNullOrEmpty(this.IS_NULLABLE) && this.IS_NULLABLE.Equals("YES", StringComparison.InvariantCultureIgnoreCase);

            public static explicit operator ColumnDefinition(TableColumnDetail column)
                => new ColumnDefinition()
                {
                    Name = column.COLUMN_NAME,
                    IsNullable = column.IsNullable,
                    DataType = column.DATA_TYPE,
                    MaximumLength = column.CHARACTER_MAXIMUM_LENGTH,
                    IsIdentity = column.IS_IDENTITY,
                    IsPrimaryKey = column.IS_PRIMARY
                };
        }

        private class Configuration
        {
            public int Id { get; set; }
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}
