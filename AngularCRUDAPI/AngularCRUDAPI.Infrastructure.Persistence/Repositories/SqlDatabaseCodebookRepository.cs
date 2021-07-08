using AngularCrudApi.Application.Helpers;
using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using AngularCrudApi.Infrastructure.Persistence.Settings;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Azure.Services.AppAuthentication;
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
        public const string RDS_TABLE_PREFIX = "CB";
        private const string ADMIN_CONSOLE_TABLE_PREFIX = "CodebookConsole";
        private const string CONFIGURATION_TABLE_SCHEME = "dbo";
        private const string CONFIGURATION_TABLE_NAME = "CodebookConsoleConfiguration";
        private const string CONFIGURATION_KEY_NAME_LOCK = "Lock";
        private const string CONFIGURATION_KEY_LAST_IMPORTED_PACKAGE = "LastImportedPackage";
        private const string CONFIGURATION_KEY_LAST_EXPORTED_PACKAGE = "LastExportedPackage";
        private const int DEFAULT_FIRST_IMPORT_PACKAGE_NUMBER = 0;
        private readonly SqlDatabaseSettings settings;
        private readonly ICommandFactory commandFactory;
        private readonly ILogger<SqlDatabaseCodebookRepository> log;
        private const string SQL_RESOURCE_STRING = "https://database.windows.net/";

        public SqlDatabaseCodebookRepository(IOptions<SqlDatabaseSettings> settings, ICommandFactory commandFactory, ILogger<SqlDatabaseCodebookRepository> log)
        {
            this.commandFactory = commandFactory ?? throw new ArgumentNullException(nameof(commandFactory));
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

                if (!String.IsNullOrEmpty(settings.IdentityId))
                {
                    try
                    {
                        connection.AccessToken = await new AzureServiceTokenProvider($"RunAs=App;AppId={settings.IdentityId}").GetAccessTokenAsync(SQL_RESOURCE_STRING);
                    }
                    catch (Exception exception)
                    {
                        throw new Exception($"Cannot get access token for identity {settings.IdentityId}", exception);
                    }
                }

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
                string commandText = "SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                commandText += $" AND NOT TABLE_NAME like '{ADMIN_CONSOLE_TABLE_PREFIX}%'";

                if (!includeRds)
                {
                    commandText += $" AND NOT TABLE_NAME like '{RDS_TABLE_PREFIX}%'";
                }

                IEnumerable<TableName> tableNames = await sqlConnection.QueryAsync<TableName>(commandText);
                IEnumerable<Codebook> codebooks = tableNames.Select(t => (Codebook)t);
                codebooks.ToList().ForEach(c => c.IsEditable = IsCodebookEditable(c));

                return codebooks;
            }
        }

        public static bool IsCodebookEditable(Codebook codebook) => codebook != null && !String.IsNullOrEmpty(codebook.Name) && !codebook.Name.StartsWith(RDS_TABLE_PREFIX);

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
                    codebookDetail.IsEditable = IsCodebookEditable(codebookDetail);
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
            Configuration configuration = await this.GetConfiguration(CONFIGURATION_KEY_NAME_LOCK); LockState lockState;

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

        public Task<LockState> CreateLock(string userIdentifier, string userName, int requestId, DateTime? created = null)
            => this.UpsertLock(userIdentifier, userName, requestId, created);

        public Task<LockState> ReleaseLock(DateTime? released = null) => this.UpsertLock("", "");

        private async Task<LockState> UpsertLock(string userIdentifier, string userName, int? requestId = null, DateTime? created = null)
        {
            if (!created.HasValue)
            {
                created = DateTime.UtcNow;
            }

            if (!requestId.HasValue)
            {
                requestId = -1;
            }

            Boolean isLocked = !String.IsNullOrEmpty(userIdentifier);

            LockState lockState = new LockState() { ForUserId = userIdentifier, ForUserName = userName, Created = created.Value, IsLocked = isLocked, ForRequestId = requestId.Value };
            string lockValue = JsonConvert.SerializeObject(lockState);
            await this.UpsertConfiguration(CONFIGURATION_KEY_NAME_LOCK, lockValue);
            return lockState;
        }

        private async Task UpsertConfiguration(string key, string value)
        {
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

                await sqlConnection.ExecuteAsync(upsertCommand.ToString(), new { key = key, value = value });
            }
        }

        private async Task<Configuration> GetConfiguration(string key)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = $"SELECT * FROM {this.ConfigurationTableFullName} WHERE [Key] = @key";
                Configuration configuration = await sqlConnection.QueryFirstOrDefaultAsync<Configuration>(commandText, new { key });
                return configuration;
            }
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

        public async Task<Release> GetReleaseById(int id)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRelease WHERE Id = @id";
                Release release = await sqlConnection.QueryFirstOrDefaultAsync<Release>(commandText, new { id = id });
                return release;
            }
        }

        public async Task<IEnumerable<Release>> GetReleaseById(int[] ids)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRelease WHERE Id in @ids";
                IEnumerable<Release> releases = await sqlConnection.QueryAsync<Release>(commandText, new { ids = ids });
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


        public async Task<Request> GetRequestById(int id)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRequest WHERE Id = @id";
                Request requests = await sqlConnection.QueryFirstOrDefaultAsync<Request>(commandText, new { id = id });
                return requests;
            }
        }

        public async Task<IEnumerable<Request>> GetRequestById(int[] requestsId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRequest WHERE Id in @ids";

                IEnumerable<Request> requests = await sqlConnection.QueryAsync<Request>(commandText, new { ids = requestsId });
                return requests;
            }
        }

        public async Task<IEnumerable<Request>> GetRequestByReleaseId(int[] releaseIds, RequestStateEnum state = null)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRequest WHERE ReleaseId in @releaseIds";

                IEnumerable<Request> requests;
                if (state == null)
                {
                    var sqlParam = new { releaseIds = releaseIds };
                    requests = await sqlConnection.QueryAsync<Request>(commandText, sqlParam);
                }
                else
                {
                    commandText += " AND Status = @status";
                    var sqlParam = new { releaseIds = releaseIds, status = state.Name };
                    requests = await sqlConnection.QueryAsync<Request>(commandText, sqlParam);
                }

                return requests;
            }
        }

        public async Task<Release> CreateRelease(Release release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"INSERT INTO [dbo].[CodebookConsoleRelease] ([Name], [Date], [Version], [Status])
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Date, @Version, @Status)";
                int insertedID = await sqlConnection.QuerySingleAsync<int>(createCommand, new
                {
                    release.Name,
                    release.Date,
                    release.Version,
                    release.Status
                });
                release.Id = insertedID;
            }

            return release;
        }

        public async Task<Release> UpdateRelease(Release release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"UPDATE [dbo].[CodebookConsoleRelease]
                    SET [Name] = @Name, 
                        [Date] = @Date, 
                        [Version] = @Version,
                        [Status] = @Status
                    WHERE [Id] = @Id";
                int affectedRows = await sqlConnection.ExecuteAsync(createCommand, new
                {
                    release.Id,
                    release.Name,
                    release.Date,
                    release.Version,
                    release.Status
                });

                if (affectedRows != 1)
                {
                    this.log.LogWarning($"Update affected {affectedRows} rows, but should be just one. Release {release.ToLogString()}");
                }
            }

            return release;
        }

        public async Task DeleteRelease(int releaseId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"DELETE FROM [dbo].[CodebookConsoleRelease]  WHERE [Id] = @Id";
                int affectedRows = await sqlConnection.ExecuteAsync(createCommand, new
                {
                    releaseId
                });

                if (affectedRows != 1)
                {
                    this.log.LogWarning($"Delete affected {affectedRows} rows, but should be just one. Release id {releaseId}");
                }
            }
        }

        public async Task<Request> CreateRequest(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"INSERT INTO [dbo].[CodebookConsoleRequest] ([Name], [SequenceNumber], [Description], [Status], [ReleaseId], [WasExported])
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @SequenceNumber, @Description, @Status, @ReleaseId, @WasExported)";
                int insertedID = await sqlConnection.QuerySingleAsync<int>(createCommand, new
                {
                    request.Name,
                    request.SequenceNumber,
                    request.Description,
                    request.Status,
                    request.ReleaseId,
                    request.WasExported
                });
                request.Id = insertedID;
            }

            return request;
        }

        public async Task<Request> UpdateRequest(Request request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"UPDATE [dbo].[CodebookConsoleRequest]
                    SET [Name] = @Name, 
                        [SequenceNumber] = @SequenceNumber, 
                        [Description] = @Description,
                        [Status] = @Status,
                        [ReleaseId] = @ReleaseId,
                        [WasExported] = @WasExported
                    WHERE [Id] = @Id";
                int affectedRows = await sqlConnection.ExecuteAsync(createCommand, new
                {
                    request.Id,
                    request.Name,
                    request.SequenceNumber,
                    request.Description,
                    request.Status,
                    request.ReleaseId,
                    request.WasExported
                });

                if (affectedRows != 1)
                {
                    this.log.LogWarning($"Update affected {affectedRows} rows, but should be just one. Request {request.ToLogString()}");
                }
            }

            return request;
        }

        public async Task DeleteRequest(int requestId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"DELETE FROM [dbo].[CodebookConsoleRequest]  WHERE [Id] = @Id";
                int affectedRows = await sqlConnection.ExecuteAsync(createCommand, new
                {
                    Id = requestId
                });

                if (affectedRows != 1)
                {
                    this.log.LogWarning($"Delete affected {affectedRows} rows, but should be just one. Request id {requestId}");
                }
            }
        }

        public async Task DeleteRequestsByReleaseId(int releaseId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"DELETE FROM [dbo].[CodebookConsoleRequest]  WHERE [ReleaseId] = @ReleaseId";
                int affectedRows = await sqlConnection.ExecuteAsync(createCommand, new
                {
                    ReleaseId = releaseId
                });
            }
        }

        public async Task ApplyChanges(int requestId, CodebookRecordChanges codebookRecordChanges)
        {
            String sqlCommand = this.commandFactory.GetCommand(codebookRecordChanges);
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                try
                {
                    int affectedRows = await sqlConnection.ExecuteAsync(sqlCommand);
                    this.log.LogInformation("Executed codebook change sql: {sqlCommand}, affected rows {affectedRows}", sqlCommand, affectedRows);
                }
                catch (Exception exception)
                {
                    string errorMessage = $"Cannot execute apply changes sql command {sqlCommand}";
                    this.log.LogError(errorMessage, exception);
                    throw new Exception(errorMessage);
                }

                await this.SaveRequestChanges(codebookRecordChanges, requestId, sqlConnection);
            }
        }

        private async Task SaveRequestChanges(CodebookRecordChanges codebookRecordChanges, int requestId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                await this.SaveRequestChanges(codebookRecordChanges, requestId, sqlConnection);
            }
        }

        private async Task SaveRequestChanges(CodebookRecordChanges codebookRecordChanges, int requestId, SqlConnection openedConnection)
        {
            int nextSequenceNumber = await this.GetMaxSequenceNumber(requestId, openedConnection);
            string createCommand = @"INSERT INTO [dbo].[CodebookConsoleRequestChange] ([RequestId],[SequenceNumber],[CodebookName],[ChangeType],[RecordId],[Change],[Command])
                    VALUES (@RequestId, @SequenceNumber, @CodebookName, @ChangeType, @RecordId, @Change, @Command)";

            JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
            List<RequestChange> requestChanges = new List<RequestChange>();
            foreach (RecordChange change in codebookRecordChanges.Changes)
            {
                nextSequenceNumber++;

                RequestChange requestChange = new RequestChange()
                {
                    ChangeType = change.Operation,
                    CodebookName = codebookRecordChanges.FullName,
                    RequestId = requestId,
                    Change = JsonConvert.SerializeObject(change, jsonSerializerSettings),
                    SequenceNumber = nextSequenceNumber,
                    RecordId = change.RecordKey.HasValue ? change.RecordKey.ToString() : "",
                    Command = change.Command
                };
                requestChanges.Add(requestChange);
            }

            await openedConnection.ExecuteAsync(createCommand, requestChanges);
        }

        private async Task<int> GetMaxSequenceNumber(int requestId, SqlConnection openedConnection)
        {
            string commandText = "SELECT Max(SequenceNumber) FROM dbo.CodebookConsoleRequestChange WHERE RequestId = @requestId";
            int? maxSequenceNumber = await openedConnection.QueryFirstOrDefaultAsync<int?>(commandText, new { requestId = requestId });
            return maxSequenceNumber.HasValue ? maxSequenceNumber.Value : 0;
        }


        public async Task<IEnumerable<RequestChange>> GetRequestChanges(int[] requestsId)
        {
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string commandText = "SELECT * FROM dbo.CodebookConsoleRequestChange WHERE RequestId in @requestIds";

                IEnumerable<RequestChange> requestsChanges = await sqlConnection.QueryAsync<RequestChange>(commandText, new { requestIds = requestsId });
                return requestsChanges;
            }
        }

        public async Task<int> GetLastExportedPackageNumber()
        {
            Configuration configuration = await this.GetConfiguration(CONFIGURATION_KEY_LAST_EXPORTED_PACKAGE);

            if (configuration == null || string.IsNullOrEmpty(configuration.Value) || !Int32.TryParse(configuration.Value, out int packageNumber))
            {
                return DEFAULT_FIRST_IMPORT_PACKAGE_NUMBER;
            }

            return packageNumber;
        }

        public Task SaveLastExportedPackageNumber(int lastPackageNumber)
        {
            return this.UpsertConfiguration(CONFIGURATION_KEY_LAST_EXPORTED_PACKAGE, lastPackageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public async Task<int> GetLastImportedPackageNumber()
        {
            Configuration configuration = await this.GetConfiguration(CONFIGURATION_KEY_LAST_IMPORTED_PACKAGE);

            if (configuration == null || string.IsNullOrEmpty(configuration.Value) || !Int32.TryParse(configuration.Value, out int packageNumber))
            {
                return DEFAULT_FIRST_IMPORT_PACKAGE_NUMBER;
            }

            return packageNumber;
        }

        public Task SaveLastImportedPackageNumber(int lastPackageNumber)
        {
            return this.UpsertConfiguration(CONFIGURATION_KEY_LAST_IMPORTED_PACKAGE, lastPackageNumber.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        public async Task UpdateRequestState(RequestStateEnum state, int[] requestsId, Boolean? wasExported)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (requestsId == null || !requestsId.Any())
            {
                throw new ArgumentNullException(nameof(requestsId));
            }

            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                string createCommand = @"UPDATE [dbo].[CodebookConsoleRequest]
                    SET [Status] = @Status"
                    + (wasExported.HasValue ? ", WasExported = @WasExported " : Environment.NewLine)
                    + "WHERE [Id] IN @Ids";

                if (wasExported.HasValue)
                {
                    await sqlConnection.ExecuteAsync(createCommand, new { Status = state.Name, Ids = requestsId, WasExported = wasExported.Value ? 1 : 0 });
                }
                else
                {
                    await sqlConnection.ExecuteAsync(createCommand, new { Status = state.Name, Ids = requestsId });
                }

            }
        }

        public async Task<DateTime> Ping()
        {
            String sqlCommand = "SELECT GetDate();";
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                try
                {
                    DateTime serverDate = await sqlConnection.QuerySingleAsync<DateTime>(sqlCommand);
                    return serverDate;
                }
                catch (Exception exception)
                {
                    string errorMessage = $"Cannot execute sql command {sqlCommand}";

                    Exception innerException = exception.InnerException;
                    while (innerException != null)
                    {
                        errorMessage += $"; inner exception: {innerException.Message}";
                        innerException = innerException.InnerException;
                    }

                    this.log.LogError(errorMessage, exception);
                    throw new Exception(errorMessage, exception);
                }
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
