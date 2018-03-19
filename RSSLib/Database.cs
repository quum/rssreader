using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System.Reflection;

using System.Data;

namespace RSSLib
{
    public class Database
    {


        private Database()
        {
        }



        private DatabaseContext _context;
        private string _schema;

        private readonly object _synch = new object();

        public string Schema { get { return _schema; } }

        private MySqlConnection _connection;
        private MySqlTransaction _transaction;

        public DatabaseContext Context { get { return _context; } }


        private bool doOpen()
        {
            bool wasOpen = true;
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                wasOpen = false;
                _connection = new MySqlConnection(_context.ConnectionString);
                _connection.Open();
            }
            return wasOpen;
        }



        private void doClose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public int ExecuteNonQuery(string sql, object parameters = null)
        {
            lock (_synch)
            {
                return doExecuteNonQuery(sql, parameters);
            }
        }

        private int doExecuteNonQuery(string sql, object parameters)
        {
            int count = 0;
            bool wasOpen = doOpen();


            MySqlCommand cmd = new MySqlCommand(sql, _connection, _transaction);
            if (parameters != null)
            {
                foreach (PropertyInfo pi in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + pi.Name, pi.GetValue(parameters, null));
                }
            }
            cmd.CommandType = CommandType.Text;


            count = cmd.ExecuteNonQuery();


            if (!wasOpen) doClose();
            return count;
        }


        public object ExecuteScalar(string sql, object parameters = null)
        {
            lock (_synch)
            {
                return doExecuteScalar(sql, parameters);
            }
        }

        private object doExecuteScalar(string sql, object parameters)
        {
            object result = null;

            bool wasOpen = doOpen();

            MySqlCommand cmd = new MySqlCommand(sql, _connection, _transaction);
            if (parameters != null)
            {
                foreach (PropertyInfo pi in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + pi.Name, pi.GetValue(parameters, null));
                }
            }
            cmd.CommandType = CommandType.Text;


            result = cmd.ExecuteScalar();


            if (!wasOpen) doClose();
            return result;
        }

        public DataTable GetDataTable(string sql, object parameters = null, string[] makeColumnsWritable = null)
        {
            lock (_synch)
            {
                return doGetDataTable(sql, parameters, makeColumnsWritable);
            }
        }

        private DataTable doGetDataTable(string sql, object parameters, string[] makeColumnsWritable)
        {
            DataTable dt = null;
            bool wasOpen = doOpen();

            MySqlCommand cmd = new MySqlCommand(sql, _connection, _transaction);
            if (parameters != null)
            {
                foreach (PropertyInfo pi in parameters.GetType().GetProperties())
                {
                    cmd.Parameters.AddWithValue("@" + pi.Name, pi.GetValue(parameters, null));
                }
            }
            cmd.CommandType = CommandType.Text;
            MySqlDataAdapter da = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();

            da.Fill(ds);


            dt = ds.Tables[0];

            if (makeColumnsWritable != null)
            {
                foreach (String columnName in makeColumnsWritable)
                {
                    dt.Columns[columnName].ReadOnly = false;
                }
            }

            if (!wasOpen) doClose();
            return dt;
        }

        public Int64 ExecuteScalarAsInt64(string sql, object parameters = null)
        {
            object tmp = ExecuteScalar(sql, parameters);
            if (tmp == null || tmp == System.DBNull.Value) return 0;
            return Convert.ToInt64(tmp);
        }

        public Int32 ExecuteScalarAsInt32(string sql, object parameters = null)
        {
            object tmp = ExecuteScalar(sql, parameters);
            if (tmp == null || tmp == System.DBNull.Value) return 0;
            return Convert.ToInt32(tmp);
        }

        public Decimal ExecuteScalarAsDecimal(string sql, object parameters = null)
        {
            object tmp = ExecuteScalar(sql, parameters);
            if (tmp == null || tmp == System.DBNull.Value) return 0;
            return Convert.ToDecimal(tmp);
        }

        public DateTime ExecuteScalarAsDateTime(string sql, object parameters = null)
        {
            object tmp = ExecuteScalar(sql, parameters);
            if (tmp == null || tmp == System.DBNull.Value) return new DateTime();
            return Convert.ToDateTime(tmp);
        }

        public String ExecuteScalarAsString(string sql, object parameters = null)
        {
            object tmp = ExecuteScalar(sql, parameters);
            if (tmp == null || tmp == System.DBNull.Value) return string.Empty;
            return Convert.ToString(tmp);
        }

        public List<String> GetStringList(string sql, object parameters = null)
        {
            List<String> ret = new List<string>();

            DataTable tmp = GetDataTable(sql, parameters);

            foreach (DataRow row in tmp.Rows)
            {
                ret.Add(row.IsNull(0) ? string.Empty : Convert.ToString(row[0]));
            }

            return ret;
        }

        public List<Int32> GetInt32List(string sql, object parameters = null)
        {
            List<Int32> ret = new List<Int32>();

            DataTable tmp = GetDataTable(sql, parameters);

            foreach (DataRow row in tmp.Rows)
            {
                if (!row.IsNull(0)) ret.Add(Convert.ToInt32(row[0]));
            }

            return ret;
        }

        public List<Decimal> GetDecimalList(string sql, object parameters = null)
        {
            List<Decimal> ret = new List<Decimal>();

            DataTable tmp = GetDataTable(sql, parameters);

            foreach (DataRow row in tmp.Rows)
            {
                if (!row.IsNull(0)) ret.Add(Convert.ToDecimal(row[0]));
            }

            return ret;
        }


        public List<T> GetList<T>(string sql, object parameters = null) where T : struct
        {
            List<T> ret = new List<T>();

            DataTable tmp = GetDataTable(sql, parameters);

            foreach (DataRow row in tmp.Rows)
            {
                ret.Add((T)Convert.ChangeType(row[0], typeof(T)));
            }

            return ret;
        }



        public List<T> Query<T>(string sql, object parameters = null) where T : class, new()
        {
            List<T> ret = new List<T>();

            DataTable tmp = GetDataTable(sql, parameters);
            HashSet<string> columnNames = new HashSet<string>();
            foreach (DataColumn col in tmp.Columns) columnNames.Add(col.ColumnName.ToUpper());

            foreach (DataRow row in tmp.Rows)
            {
                ret.Add(loadFromDataRow<T>(row, columnNames));
            }

            return ret;
        }

        public Dictionary<string, string> GetDictionary(string sql, object parameters = null)
        {
            var ret = new Dictionary<string, string>();

            foreach (DataRow row in GetDataTable(sql, parameters).Rows)
            {
                ret[Convert.ToString(row[0])] = Convert.ToString(row[1]);
            }

            return ret;
        }

        private T loadFromDataRow<T>(DataRow source, HashSet<string> columnNames) where T : class, new()
        {
            T item = new T();


            foreach (PropertyInfo propTarget in typeof(T).GetProperties())
            {
                if (propTarget.CanRead && propTarget.CanWrite && columnNames.Contains(propTarget.Name.ToUpper()) && !source.IsNull(propTarget.Name))
                {
                    if (propTarget.PropertyType == typeof(Decimal) && source[propTarget.Name].GetType() == typeof(Double))
                    {
                        propTarget.SetValue(item, Convert.ToDecimal(source[propTarget.Name]), null);
                    }
                    else if (propTarget.PropertyType == typeof(Decimal) && source[propTarget.Name].GetType() == typeof(Int32))
                    {
                        propTarget.SetValue(item, Convert.ToDecimal(source[propTarget.Name]), null);
                    }
                    else if (propTarget.PropertyType == typeof(Double) && source[propTarget.Name].GetType() == typeof(Decimal))
                    {
                        propTarget.SetValue(item, Convert.ToDouble(source[propTarget.Name]), null);
                    }
                    else if (propTarget.PropertyType == typeof(String) && source[propTarget.Name].GetType() == typeof(Int32))
                    {
                        propTarget.SetValue(item, Convert.ToString(source[propTarget.Name]), null);
                    }
                    else
                    {
                        propTarget.SetValue(item, source[propTarget.Name], null);
                    }
                }
            }

            return item;
        }

        public void SaveToDataRow(object o, DataRow target, HashSet<string> columnNames = null)
        {
            foreach (PropertyInfo propInfo in o.GetType().GetProperties())
            {
                if (propInfo.CanRead && propInfo.CanWrite && (columnNames == null || columnNames.Contains(propInfo.Name)))
                {
                    target[propInfo.Name] = propInfo.GetValue(o, null);
                }
            }
        }

        public Database(DatabaseContext context)
        {
            _context = context;
            _schema = context.Schema;
        }

        public void InTransaction(Action<Database> a)
        {
            lock (_synch)
            {
                doOpen();
                try
                {
                    _transaction = _connection.BeginTransaction();
                    a(this);
                    _transaction.Commit();
                }
                finally
                {
                    doClose();
                }
            }
        }

    }
}
