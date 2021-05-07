using AngularCrudApi.Application.Interfaces.Repositories;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Infrastructure.Persistence.Settings;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            using (SqlConnection sqlConnection = await this.GetOpenedSqlConnetion())
            {
                CodebookDetail codebookDetail = null;
                string commandText = "SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, IS_NULLABLE, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tableName";
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

            public Boolean IsNullable => !String.IsNullOrEmpty(this.IS_NULLABLE) && this.IS_NULLABLE.Equals("YES", StringComparison.InvariantCultureIgnoreCase);

            public static explicit operator ColumnDefinition(TableColumnDetail column)
                => new ColumnDefinition() { Name = column.COLUMN_NAME, IsNullable = column.IsNullable, DataType = column.DATA_TYPE, MaximumLength = column.CHARACTER_MAXIMUM_LENGTH };
        }
    }
}
