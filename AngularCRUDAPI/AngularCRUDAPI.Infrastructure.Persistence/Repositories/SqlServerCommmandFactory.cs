using AngularCrudApi.Application.Exceptions;
using AngularCrudApi.Domain.Entities;
using AngularCrudApi.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngularCrudApi.Infrastructure.Persistence.Repositories
{
    public interface ICommandFactory
    {
        string GetCommand(CodebookRecordChanges codebookRecordChanges);
    }

    public class SqlServerCommmandFactory : ICommandFactory
    {
        public string GetCommand(CodebookRecordChanges codebookRecordChanges)
        {
            return string.Join(Environment.NewLine, codebookRecordChanges.Changes.Select(c => this.GetCommandText(codebookRecordChanges.FullName, codebookRecordChanges.Columns, c)));
        }

        private string GetCommandText(string tableFullName, IEnumerable<ColumnDefinition> columns, RecordChange recordChange)
        {
            if (recordChange.Operation.Equals(RecordChangeOperationEnum.Delete.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetDeleteCommandText(tableFullName, columns, recordChange.RecordKey.Value);
            }

            if (recordChange.Operation.Equals(RecordChangeOperationEnum.Insert.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetInsertCommandText(tableFullName, columns, recordChange.RecordChanges);
            }

            if (recordChange.Operation.Equals(RecordChangeOperationEnum.Update.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                return this.GetUpdateCommandText(tableFullName, columns, recordChange.RecordKey.Value, recordChange.RecordChanges);
            }

            throw new ValidationException($"Cannot generate sql command for operation type {recordChange.Operation}");
        }

        private string GetUpdateCommandText(string tableFullName, IEnumerable<ColumnDefinition> columns, KeyValuePair<string, object> recordKey, Dictionary<string, object> recordChanges)
        {
            ColumnDefinition columnDefinition = columns.FirstOrDefault(c => c.Name.Equals(recordKey.Key, StringComparison.InvariantCultureIgnoreCase));
            if (columnDefinition == null || recordKey.Value == null)
            {
                throw new Exception("Error generating SQL update command. Missing primary key column/value for where condition");
            }

            string whereCondition = $"[{columnDefinition.Name}] = {this.GetValueSqlRepresentation(recordKey.Value, columnDefinition)}";


            List<string> setValues = new List<string>();
            foreach (KeyValuePair<string, object> recordChange in recordChanges)
            {
                columnDefinition = columns.FirstOrDefault(c => c.Name.Equals(recordChange.Key, StringComparison.InvariantCultureIgnoreCase));

                if (columnDefinition == null || columnDefinition.IsPrimaryKey || columnDefinition.IsIdentity)
                {
                    continue;
                }

                setValues.Add($"[{columnDefinition.Name}]={this.GetValueSqlRepresentation(recordChange.Value, columnDefinition)}");
            }


            return $"UPDATE {tableFullName} SET {String.Join(",", setValues)} WHERE {whereCondition}";
        }

        private string GetInsertCommandText(string tableFullName, IEnumerable<ColumnDefinition> columns, Dictionary<string, object> recordChanges)
        {
            List<ColumnDefinition> usedColumns = new List<ColumnDefinition>();
            List<string> usedValueList = new List<string>();

            foreach (KeyValuePair<string, object> recordChange in recordChanges)
            {
                ColumnDefinition columnDefinition = columns.FirstOrDefault(c => c.Name.Equals(recordChange.Key, StringComparison.InvariantCultureIgnoreCase));

                if (columnDefinition == null || columnDefinition.IsIdentity)
                {
                    continue;
                }

                usedColumns.Add(columnDefinition);
                usedValueList.Add(this.GetValueSqlRepresentation(recordChange.Value, columnDefinition));
            }

            return $"INSERT INTO {tableFullName} ({String.Join(",", usedColumns.Select(c => "[" + c.Name + "]").ToList())}) VALUES ({String.Join(",", usedValueList)})";
        }

        private string GetDeleteCommandText(string tableFullName, IEnumerable<ColumnDefinition> columns, KeyValuePair<string, object> recordKey)
        {
            ColumnDefinition columnDefinition = columns.FirstOrDefault(c => c.Name.Equals(recordKey.Key));

            if (columnDefinition == null)
            {
                throw new ValidationException($"Missing column {recordKey.Key}");
            }

            return $"DELETE FROM {tableFullName} WHERE [{recordKey.Key}] = {this.GetValueSqlRepresentation(recordKey.Value, columnDefinition)}; ";
        }

        private string GetValueSqlRepresentation(object value, ColumnDefinition columnDefinition)
        {
            if (value == null)
            {
                return "null";
            }

            if (columnDefinition.DataType.Contains("int", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("decimal", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("bit", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("numeric", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("float", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("real", StringComparison.InvariantCultureIgnoreCase)
                )
            {
                return $"{value}";
            }

            if (columnDefinition.DataType.Contains("char", StringComparison.InvariantCultureIgnoreCase)
                || columnDefinition.DataType.Contains("text", StringComparison.InvariantCultureIgnoreCase))
            {
                return $"'{value}'";
            }

            // fallback for the rest
            return $"'{value}'";
        }
    }
}
