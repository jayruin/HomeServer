using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace HomeServer.Utility
{
    public class SQLiteDictionary : IDictionary<string, string>
    {
        private readonly string connectionString;
        private readonly string tableName;
        public SQLiteDictionary(string connectionString, string tableName)
        {
            this.connectionString = connectionString;
            this.tableName = tableName;
            InitializeTable();
        }

        private void InitializeTable()
        {
            string commandText = $"CREATE TABLE IF NOT EXISTS {tableName} (Key text PRIMARY KEY, Value text)";
            SQLiteUtility.ExecuteCommand(connectionString, commandText);
        }

        public string this[string key]
        {
            get
            {
                if (SQLiteUtility.IsNull(key))
                {
                    throw new ArgumentNullException();
                }
                else if (!ContainsKey(key))
                {
                    throw new KeyNotFoundException();
                }
                else
                {
                    string query = $"SELECT Value FROM {tableName} WHERE Key = @key";
                    List<SqliteParameter> parameters = new List<SqliteParameter>();
                    parameters.Add(new SqliteParameter("@key", key));
                    return SQLiteUtility.GetQueryResult(connectionString, query, parameters).Rows[0][0];
                }
            }
            set
            {
                if (SQLiteUtility.IsNull(key))
                {
                    throw new ArgumentNullException();
                }
                else if (!ContainsKey(key))
                {
                    Add(key, value);
                }
                else
                {
                    string commandText = $"UPDATE TABLE {tableName} SET Value = @value WHERE Key = @key";
                    List<SqliteParameter> parameters = new List<SqliteParameter>();
                    parameters.Add(new SqliteParameter("@key", key));
                    parameters.Add(new SqliteParameter("@value", value));
                    SQLiteUtility.ExecuteCommand(connectionString, commandText, parameters);
                }
            }
        }

        public ICollection<string> Keys
        {
            get
            {
                List<string> keys = new List<string>();
                List<List<string>> rows = SQLiteUtility.GetQueryResult(connectionString, $"SELECT Key FROM {tableName}").Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    keys.Add(rows[i][0]);
                }
                return keys;
            }
        }

        public ICollection<string> Values
        {
            get
            {
                List<string> values = new List<string>();
                List<List<string>> rows = SQLiteUtility.GetQueryResult(connectionString, $"SELECT Value FROM {tableName}").Rows;
                for (int i = 0; i < rows.Count; i++)
                {
                    values.Add(rows[i][0]);
                }
                return values;
            }
        }

        public int Count
        {
            get
            {
                return SQLiteUtility.GetQueryResult(connectionString, $"SELECT * FROM {tableName}").Rows.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(string key, string value)
        {
            if (SQLiteUtility.IsNull(key))
            {
                throw new ArgumentNullException();
            }
            else if (ContainsKey(key))
            {
                throw new ArgumentException();
            }
            else
            {
                string commandText = $"INSERT INTO {tableName} (Key, Value) VALUES (@key, @value)";
                List<SqliteParameter> parameters = new List<SqliteParameter>();
                parameters.Add(new SqliteParameter("@key", key));
                parameters.Add(new SqliteParameter("@value", value));
                SQLiteUtility.ExecuteCommand(connectionString, commandText, parameters);
            }
        }

        public void Add(KeyValuePair<string, string> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            string commandText = $"DELETE FROM {tableName}";
            SQLiteUtility.ExecuteCommand(connectionString, commandText);
        }

        public bool Contains(KeyValuePair<string, string> item)
        {
            return SQLiteUtility.GetQueryResult(connectionString, $"SELECT * FROM {tableName}").Rows.Contains(new List<string>(new string[] { item.Key, item.Value }));
        }

        public bool ContainsKey(string key)
        {
            return Keys.Contains(key);
        }

        public bool ContainsValue(string value)
        {
            return Values.Contains(value);
        }

        public void CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException();
            }
            else if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            else if (array.Rank > 1)
            {
                throw new ArgumentException();
            }
            else if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentException();
            }
            else
            {
                try
                {
                    List<List<string>> rows = SQLiteUtility.GetQueryResult(connectionString, $"SELECT Key, Value FROM {tableName}").Rows;
                    for (int i = 0; i < rows.Count; i++)
                    {
                        array[arrayIndex + i] = new KeyValuePair<string, string>(rows[i][0], rows[i][1]);
                    }
                }
                catch (InvalidCastException e)
                {
                    throw new ArgumentException(e.Message);
                }
            }
        }

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            List<List<string>> rows = SQLiteUtility.GetQueryResult(connectionString, $"SELECT Key, Value FROM {tableName}").Rows;
            for (int i = 0; i < rows.Count; i++)
            {
                yield return new KeyValuePair<string, string>(rows[i][0], rows[i][1]);
            }
        }

        public bool Remove(string key)
        {
            if (SQLiteUtility.IsNull(key))
            {
                throw new ArgumentNullException();
            }
            if (!ContainsKey(key))
            {
                return false;
            }
            else
            {
                string commandText = $"DELETE FROM {tableName} WHERE Key = @key";
                List<SqliteParameter> parameters = new List<SqliteParameter>();
                parameters.Add(new SqliteParameter("@key", key));
                SQLiteUtility.ExecuteCommand(connectionString, commandText, parameters);
                return true;
            }
        }

        public bool Remove(KeyValuePair<string, string> item)
        {
            if (SQLiteUtility.IsNull(item.Key))
            {
                throw new ArgumentNullException();
            }
            if (!Contains(item))
            {
                return false;
            }
            else
            {
                string commandText = $"DELETE FROM {tableName} WHERE Key = @key AND Value = @value";
                List<SqliteParameter> parameters = new List<SqliteParameter>();
                parameters.Add(new SqliteParameter("@key", item.Key));
                parameters.Add(new SqliteParameter("@value", item.Value));
                SQLiteUtility.ExecuteCommand(connectionString, commandText, parameters);
                return true;
            }
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out string value)
        {
            if (SQLiteUtility.IsNull(key))
            {
                throw new ArgumentNullException();
            }
            else if (!ContainsKey(key))
            {
                value = default(string);
                return false;
            }
            else
            {
                value = this[key];
                return true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
