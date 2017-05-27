namespace SqliteHelper
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SQLite;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class SqliteHelper
    {
        private SQLiteConnection conn;
        private static SqliteHelper instance;
        private string path;
        private static Dictionary<object, object> Sessions = new Dictionary<object, object>();

        public bool CheckConnection()
        {
            return !string.IsNullOrEmpty(this.path);
        }

        public bool CreateTable(string tableName, params string[][] parameters)
        {
            this.InitConn();
            if (this.IsExists(tableName))
            {
                this.DropTable(tableName);
            }
            if (parameters == null)
            {
                throw new NullReferenceException("参数不能为空！");
            }
            List<string> values = new List<string>();
            foreach (string[] strArray in parameters)
            {
                values.Add(string.Join(" ", strArray));
            }
            string sql = string.Format("create table {0}({1})", tableName, string.Join(",", values));
            return this.ExecuteNonQuery(sql, new SQLiteParameter[0]);
        }

        public bool Delete(string tableName, params string[] parameters)
        {
            if ((parameters == null) || (parameters.Length != 2))
            {
                throw new NullReferenceException("参数错误！");
            }
            this.InitConn();
            List<string> list = new List<string> {
                tableName
            };
            list.AddRange(parameters);
            string sql = string.Format("Delete {0} where {1} = '{2}'", (object[]) list.ToArray());
            return this.ExecuteNonQuery(sql, new SQLiteParameter[0]);
        }

        public void DropDataSource()
        {
            if (!(!string.IsNullOrEmpty(this.path) && File.Exists(this.path)))
            {
                throw new NullReferenceException("文件未找到！");
            }
            File.Delete(this.path);
        }

        public void DropTable(string tableName)
        {
            this.InitConn();
            string commandText = "Drop table " + tableName;
            using (this.conn)
            {
                this.conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(commandText, this.conn))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public object Execute(string sql, params SQLiteParameter[] sqliteparameters)
        {
            object obj2;
            this.InitConn();
            using (this.conn)
            {
                this.conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, this.conn))
                {
                    try
                    {
                        command.Transaction = this.conn.BeginTransaction();
                        if (sqliteparameters != null)
                        {
                            command.Parameters.AddRange(sqliteparameters);
                        }
                        DataSet dataSet = new DataSet();
                        new SQLiteDataAdapter(command).Fill(dataSet);
                        if (dataSet.Tables.Count == 0)
                        {
                            throw new SQLiteException("sql语句不合法.");
                        }
                        return dataSet;
                    }
                    catch (SQLiteException)
                    {
                        try
                        {
                            command.Transaction.Rollback();
                            command.Transaction = this.conn.BeginTransaction();
                            return command.ExecuteNonQuery();
                        }
                        catch (SQLiteException)
                        {
                            return null;
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                    finally
                    {
                        command.Transaction.Commit();
                    }
                }
            }
            return obj2;
        }

        public bool ExecuteNonQuery(string sql, params SQLiteParameter[] sqliteparameters)
        {
            bool flag;
            this.InitConn();
            using (this.conn)
            {
                this.conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, this.conn))
                {
                    if (sqliteparameters != null)
                    {
                        command.Parameters.AddRange(sqliteparameters);
                    }
                    flag = command.ExecuteNonQuery() > 0;
                }
            }
            return flag;
        }

        public string[] GetAllColumnsByTableName(string tableName)
        {
            string sql = string.Format("pragma table_info({0})", tableName);
            DataTable table = this.GetTable(sql, (SQLiteParameter[]) null);
            List<string> list = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(row[1].ToString());
            }
            return list.ToArray();
        }

        public string[] GetAllTables()
        {
            string sql = "select * from sqlite_master";
            DataTable table = this.GetTable(sql, (SQLiteParameter[]) null);
            List<string> list = new List<string>();
            foreach (DataRow row in table.Rows)
            {
                list.Add(row[1].ToString());
            }
            return list.ToArray();
        }

        private static SqliteHelper GetInstance()
        {
            if (instance == null)
            {
                instance = new SqliteHelper();
            }
            return instance;
        }

        public object GetOnly(string sql, params SQLiteParameter[] sqliteparameters)
        {
            object obj2;
            this.InitConn();
            using (this.conn)
            {
                this.conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, this.conn))
                {
                    if (sqliteparameters != null)
                    {
                        command.Parameters.AddRange(sqliteparameters);
                    }
                    obj2 = command.ExecuteScalar();
                }
            }
            return obj2;
        }

        public object GetSessionByKey(object key)
        {
            if (!Sessions.ContainsKey(key))
            {
                return null;
            }
            return Sessions[key];
        }

        public DataTable GetTable(string tableName, params Parameter[] parameters)
        {
            StringBuilder builder = new StringBuilder();
            List<SQLiteParameter> list = new List<SQLiteParameter>();
            builder.Append(string.Format("select * from {0} where 1 = 1 ", tableName));
            if (parameters != null)
            {
                foreach (Parameter parameter in parameters)
                {
                    SQLiteParameter item = new SQLiteParameter("@" + parameter.parameterName, parameter.parameterValue);
                    string format = string.Empty;
                    list.Add(item);
                    switch (parameter.selectMode)
                    {
                        case SelectMode.AndLike:
                            format = "and {0} like '%" + item.Value + "%' ";
                            list.Remove(item);
                            break;

                        case SelectMode.OrEqual:
                            format = "or {0} = @{0} ";
                            break;

                        case SelectMode.AndNoequal:
                            format = "and {0} <> @{0} ";
                            break;

                        case SelectMode.AndFirstLike:
                            format = "and {0} like '%" + item.Value + " ";
                            list.Remove(item);
                            break;

                        case SelectMode.AndLastLike:
                            format = "and {0} like '" + item.Value + "%' ";
                            list.Remove(item);
                            break;

                        case SelectMode.OrNoequal:
                            format = "or {0} <> @{0} ";
                            break;

                        case SelectMode.OrFirstLike:
                            format = "or {0} like '%" + item.Value + "' ";
                            list.Remove(item);
                            break;

                        case SelectMode.OrLastLike:
                            format = "and {0} like '" + item.Value + "%' ";
                            list.Remove(item);
                            break;

                        case SelectMode.OrLike:
                            format = "and like '" + item.Value + "%' ";
                            list.Remove(item);
                            break;

                        default:
                            format = "and {0} = @{0} ";
                            break;
                    }
                    builder.Append(string.Format(format, parameter.parameterName));
                }
            }
            return this.GetTable(builder.ToString(), list.ToArray());
        }

        public DataTable GetTable(string sql, params SQLiteParameter[] sqliteparameters)
        {
            DataTable table;
            this.InitConn();
            using (this.conn)
            {
                this.conn.Open();
                using (SQLiteCommand command = new SQLiteCommand(sql, this.conn))
                {
                    if (sqliteparameters != null)
                    {
                        command.Parameters.AddRange(sqliteparameters.ToArray<SQLiteParameter>());
                    }
                    DataSet dataSet = new DataSet();
                    SQLiteDataAdapter adapter = new SQLiteDataAdapter(command);
                    try
                    {
                        adapter.Fill(dataSet);
                        if (dataSet.Tables == null)
                        {
                            throw new IndexOutOfRangeException();
                        }
                        table = dataSet.Tables[0];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        table = new DataTable();
                    }
                    catch (SQLiteException)
                    {
                        table = null;
                    }
                }
            }
            return table;
        }

        public void Init()
        {
            this.InitPath();
            this.InitConn();
        }

        private void InitConn()
        {
            this.conn = new SQLiteConnection(string.Format("Data Source = {0};version = 3", this.path));
        }

        private void InitPath()
        {
            if (string.IsNullOrEmpty(this.path))
            {
                throw new NullReferenceException("请先指定数据库文件路径！");
            }
            if (!File.Exists(this.path))
            {
                SQLiteConnection.CreateFile(this.path);
            }
        }

        public bool Insert(string tableName, params string[] parameters)
        {
            if (parameters == null)
            {
                throw new NullReferenceException("数据不能为空！");
            }
            this.InitConn();
            string sql = string.Format("insert into {0} values('{1}')", tableName, string.Join("','", parameters));
            return this.ExecuteNonQuery(sql, new SQLiteParameter[0]);
        }

        public bool IsExists(string tableName)
        {
            string sql = "select count(1) from sqlite_master where name=@name";
            SQLiteParameter[] sqliteparameters = new SQLiteParameter[] { new SQLiteParameter("@name", tableName) };
            return (int.Parse(this.GetOnly(sql, sqliteparameters).ToString()) != 0);
        }

        public bool IsExists(string tableName, params Parameter[] parameters)
        {
            if ((parameters == null) || (parameters.Length == 0))
            {
                throw new NullReferenceException("没有参数！");
            }
            DataTable table = this.GetTable(tableName, parameters);
            return (((table != null) && (table.Rows != null)) && (table.Rows.Count > 0));
        }

        public void SetDataSourcePath(string path)
        {
            this.path = path;
        }

        public void SetSession(object key, object value)
        {
            if (!Sessions.ContainsKey(key))
            {
                Sessions.Add(key, null);
            }
            Sessions[key] = value;
        }

        public bool Update(string tableName, params string[] parameters)
        {
            if ((parameters == null) || (parameters.Length != 4))
            {
                throw new NullReferenceException("参数错误！");
            }
            List<string> list = new List<string> {
                tableName
            };
            list.AddRange(parameters);
            string sql = string.Format("Update {0} set {3} = '{4}' where {1} = '{2}'", (object[]) list.ToArray());
            return this.ExecuteNonQuery(sql, new SQLiteParameter[0]);
        }

        public static SqliteHelper Instance
        {
            get
            {
                return GetInstance();
            }
            set
            {
                instance = value;
            }
        }
    }
}

