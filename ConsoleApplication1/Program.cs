using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalTrance;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {

            CreateTab();
            Console.ReadLine();

        }
        private static void CreateTab()
        {
            string dataPath = Application.StartupPath;
            //SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(Path.Combine(dataPath, "Proc"));
            //var conn = new System.Data.SQLite.SQLiteConnection(Path.Combine(txt_DataSourceFilePath.Text, txt_DataSourceFileName.Text));
            //conn.SetPassword("123456");
            var parameters = new List<string[]>();
            parameters.Add(new[] { "ProcID", "varchar", "(50)" });
            parameters.Add(new[] { "ProcName", "varchar", "(200)" });
            parameters.Add(new[] { "ProcInfo", "blob", "(20480)" });
            parameters.Add(new[] { "ProcMD5", "varchar", "(32)" });
            parameters.Add(new[] { "CreateTime", "varchar", "(50)" });
            parameters.Add(new[] { "ModifyTime", "varchar", "(50)" });
            ///////////////////////////////////
            MessageAdd("参数完成");
            string path = Path.Combine(dataPath, "Proc.db");
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
            var conn = new SQLiteConnection(string.Format("Data Source = {0};version = 3", path));

            //SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(Path.Combine(dataPath, "Proc.db"));
            string sql = string.Format("insert into {0} values('{1}')", "ProcBackUp", string.Join("','", parameters));
            var sqliteparameters = parameters.ToArray();
            //SqliteHelper.SqliteHelper.Instance.CreateTable("FileInfo", sqliteparameters);

            MessageAdd("sql完成");
            bool flag = false;
     
            try
            {
                using (conn)
                {
                    conn.Open();
                    MessageAdd("conn打开");
                    List<string> values = new List<string>();
                    foreach (string[] strArray in sqliteparameters)
                    {
                        values.Add(string.Join(" ", strArray));
                    }
                    string sql1 = string.Format("create table {0}({1})", "ProcBackUp", string.Join(",", values));

                    using (SQLiteCommand command = new SQLiteCommand(sql1, conn))
                    {
                        if (new SQLiteParameter[0] != null)
                        {
                            command.Parameters.AddRange(new SQLiteParameter[0]);
                        }
                        flag = command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageAdd("异常:" + ex.Message + ex.StackTrace);
            }
            /////////////
            MessageAdd("创建表:" + flag);
        }
        private static void MessageAdd(string msg)
        {
            string path = LogHelper.Path;
            LogHelper.WriteFile(path, msg);
        }

    }
}
