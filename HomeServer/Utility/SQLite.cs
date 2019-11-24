using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using HomeServer.Areas.DataWarehouse.Models;
using System.Diagnostics;

namespace HomeServer.Utility
{
    public static class SQLiteUtility
    {
        public static bool IsNull(string item)
        {
            return item is null || item.Equals("NULL") || item.Equals("");
        }
        public static SqliteType ConvertDataTypeName(string dataTypeName)
        {
            switch (dataTypeName.ToUpper())
            {
                case string s when s.Contains("INT"):
                    return SqliteType.Integer;
                case string s when s.Contains("CHAR") || s.Contains("CLOB") || s.Contains("TEXT"):
                    return SqliteType.Text;
                case string s when s.Contains("REAL") || s.Contains("FLOA") || s.Contains("DOUB"):
                    return SqliteType.Real;
                case "BLOB":
                    return SqliteType.Blob;
                default:
                    return SqliteType.Text;
            }
        }
        
        public static string GetTableDefinition(string connectionString, string tableName)
        {
            string tableDefinition = "";
            string query = "SELECT sql FROM sqlite_master WHERE type=\'table\' AND name=@tableName ORDER BY NAME";
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand command = new SqliteCommand(query, conn))
                {
                    SqliteParameter parameter = new SqliteParameter("@tableName", tableName);
                    command.Parameters.Add(parameter);
                    SqliteDataReader reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        tableDefinition = reader.GetString(0);
                    }
                }
            }
            return tableDefinition;
        }
        public static List<(string, string)> GetTables(string connectionString)
        {
            List<(string, string)> tables = new List<(string, string)>();
            string query = "SELECT name, sql FROM sqlite_master WHERE type=\'table\' ORDER BY NAME";
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand command = new SqliteCommand(query, conn))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tables.Add((reader.GetString(0), reader.GetString(1)));
                    }
                }
            }
            return tables;
        }

        public static void CreateTable(string connectionString, string tableName, IList<TableAddColumnModel> tableColumns)
        {
            string commandText = "CREATE TABLE " + tableName + "(";
            foreach (TableAddColumnModel col in tableColumns)
            {
                commandText += col.Column + " " + col.Type;
                commandText += (col.PrimaryKey ? " PRIMARY KEY" : "");
                commandText += (col.NotNull ? " NOT NULL" : "");
                commandText += (col.Unique ? " UNIQUE" : "");
                commandText += ",";
            }
            commandText = commandText.Trim(new char[] { ',' });
            commandText += ")";
            ExecuteCommand(connectionString, commandText);
        }

        public static List<SQLiteColumn> GetColumns(string connectionString, string tableName)
        {
            List<SQLiteColumn> columns = new List<SQLiteColumn>();
            string query = $"SELECT * FROM pragma_table_info(\'{tableName}\')";
            string name;
            SqliteType type;
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand command = new SqliteCommand(query, conn))
                {
                    SqliteDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        name = reader.GetString(reader.GetOrdinal("name"));
                        type = ConvertDataTypeName(reader.GetString(reader.GetOrdinal("type")));
                        columns.Add(new SQLiteColumn(name, type));
                    }
                }
            }
            return columns;
        }

        public static List<List<string>> GetAllRows(string connectionString, string tableName)
        {
            string query = $"SELECT * FROM {tableName}";
            return GetQueryResult(connectionString, query).Rows;
        }

        public static void InsertRow(string connectionString, string tableName, IList<string> row)
        {
            string commandText = $"INSERT INTO {tableName} VALUES (";
            List<SqliteParameter> parameters = new List<SqliteParameter>();
            for (int i = 0; i < row.Count; i++)
            {
                string item = row[i];
                string parameterName = $"@parameter{i}";
                commandText += parameterName + ",";
                SqliteParameter parameter = new SqliteParameter();
                parameter.ParameterName = parameterName;
                if (!IsNull(item))
                {
                    parameter.Value = item;
                }
                else
                {
                    parameter.Value = DBNull.Value;
                }
                parameters.Add(parameter);
            }
            commandText = commandText.Trim(new char[] { ',' });
            commandText += ")";
            ExecuteCommand(connectionString, commandText, parameters);
        }

        public static void DeleteRow(string connectionString, string tableName, IList<string> row)
        {
            string commandText = $"DELETE FROM {tableName} WHERE ";
            List<SqliteParameter> parameters = new List<SqliteParameter>();
            List<SQLiteColumn> columns = GetColumns(connectionString, tableName);
            for (int i = 0; i < columns.Count; i++)
            {
                string item = row[i];
                
                if (!IsNull(item))
                {
                    string parameterName = $"@parameter{i}";
                    SqliteParameter parameter = new SqliteParameter();
                    parameter.ParameterName = parameterName;
                    parameter.Value = item;
                    commandText += $"{columns[i].Name} = {parameterName} AND ";
                    parameters.Add(parameter);
                }
                else
                {
                    commandText += $"{columns[i].Name} IS NULL AND ";
                }
            }
            commandText = commandText.Substring(0, commandText.Length - 5);
            ExecuteCommand(connectionString, commandText, parameters);
        }

        public static void EditRow(string connectionString, string tableName, IList<string> row, IList<string> newRow)
        {
            string commandText = $"UPDATE {tableName} SET ";
            List<SqliteParameter> parameters = new List<SqliteParameter>();
            List<SQLiteColumn> columns = GetColumns(connectionString, tableName);
            string whereClause = " WHERE ";
            for (int i = 0; i < columns.Count; i++)
            {
                string oldItem = row[i];
                if (!IsNull(oldItem))
                {
                    string parameterOldName = $"@parameterOld{i}";
                    whereClause += $"{columns[i].Name} = {parameterOldName} AND ";
                    SqliteParameter oldParameter = new SqliteParameter();
                    oldParameter.ParameterName = parameterOldName;
                    oldParameter.Value = oldItem;
                    parameters.Add(oldParameter);
                }
                else
                {
                    whereClause += $"{columns[i].Name} IS NULL AND ";
                }

                string newItem = newRow[i];
                string parameterNewName = $"@parameterNew{i}";
                commandText += $"{columns[i].Name} = {parameterNewName}, ";
                SqliteParameter newParameter = new SqliteParameter();
                newParameter.ParameterName = parameterNewName;
                if (!IsNull(newItem))
                {
                    newParameter.Value = newItem;
                }
                else
                {
                    newParameter.Value = DBNull.Value;
                }
                parameters.Add(newParameter);
            }
            whereClause = whereClause.Substring(0, whereClause.Length - 5);
            commandText = commandText.Trim(new char[] { ' ' });
            commandText = commandText.Trim(new char[] { ',' });
            commandText += whereClause;
            ExecuteCommand(connectionString, commandText, parameters);
        }

        public static void ExecuteCommand(string connectionString, string commandText, IList<SqliteParameter> parameters = null)
        {
            parameters = parameters ?? new List<SqliteParameter>();
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand command = new SqliteCommand(commandText, conn))
                {
                    foreach (SqliteParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    command.ExecuteNonQuery();
                }
            }
        }

        public static QueryResultModel GetQueryResult(string connectionString, string query, IList<SqliteParameter> parameters = null)
        {
            parameters = parameters ?? new List<SqliteParameter>();
            List<List<string>> rows = new List<List<string>>();
            List<string> row;
            List<SQLiteColumn> columns = new List<SQLiteColumn>();
            using (SqliteConnection conn = new SqliteConnection(connectionString))
            {
                conn.Open();
                using (SqliteCommand command = new SqliteCommand(query, conn))
                {
                    foreach (SqliteParameter parameter in parameters)
                    {
                        command.Parameters.Add(parameter);
                    }
                    SqliteDataReader reader = command.ExecuteReader();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        columns.Add(new SQLiteColumn(reader.GetName(i), ConvertDataTypeName(reader.GetDataTypeName(i))));
                    }
                    while (reader.Read())
                    {
                        row = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            row.Add(reader[i] == DBNull.Value ? "NULL" : reader.GetString(i));
                        }
                        rows.Add(row);
                    }
                }
            }
            return new QueryResultModel(query, columns, rows);
        }

        public static TableModel GetTableModel(string connectionString, string tableName)
        {
            TableModel model = new TableModel(GetColumns(connectionString, tableName), GetAllRows(connectionString, tableName), tableName, GetTableDefinition(connectionString, tableName));
            return model;
        }
    }

    public class SQLiteColumn
    {
        public string Name { get; set; }
        public SqliteType Type { get; set; }

        public SQLiteColumn(string name, SqliteType type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}
