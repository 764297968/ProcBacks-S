using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using TerminalTrance;

namespace ConsoleApplication1
{
    class Program
    {
        static string dataPath = "D:\\windows服务\\BackUp";
        static System.Timers.Timer tim = new System.Timers.Timer();
        static Stopwatch sw = new Stopwatch();
        static int num = 0;
        static void Main(string[] args)
        {
            CreateTab();
            sw.Start();

            BackUpProc();

            sw.Stop();
            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //CreateTab();
            Console.ReadLine();

        }
        private static void StopTim()
        {
            num = 1000;
            tim.Stop();
        }
        private static void BackUpProc()
        {
            MessageAdd("填充------------");
            var sql = "insert into ProcBackUp values(@ProcID,@ProcName,@ProcInfo,@ProcMD5,@GenerateTime,@ModifyTime,@CreateTime)";
            SQLiteParameter[] prams = new SQLiteParameter[] {
                new SQLiteParameter("@ProcID",Guid.NewGuid().ToString()),
                new SQLiteParameter("@ProcName",Guid.NewGuid().ToString()),
                new SQLiteParameter("@ProcInfo",Guid.NewGuid().ToString()),
                new SQLiteParameter("@ProcMD5",Guid.NewGuid().ToString()),
                new SQLiteParameter("@GenerateTime",Guid.NewGuid().ToString()),
                new SQLiteParameter("@ModifyTime",Guid.NewGuid().ToString()),
                new SQLiteParameter("@CreateTime",DateTime.Now)
            };

            try
            {
                bool flag = false;
                string path = Path.Combine(dataPath, "Proc.db");
                var conn = new SQLiteConnection(string.Format("Data Source={0};version=3", path));

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    conn.Open();
                    conn.BusyTimeout = 90;
                     if (prams != null)
                    {
                        foreach (SQLiteParameter p in prams)
                        {
                            if (p != null)
                            {
                                // Check for derived output value with no value assigned
                                if ((p.Direction == ParameterDirection.InputOutput ||
                                    p.Direction == ParameterDirection.Input) &&
                                    (p.Value == null))
                                {
                                    p.Value = DBNull.Value;
                                }
                                command.Parameters.Add(p);
                            }
                        }
                    }
                    //if (new SQLiteParameter[0] != null)
                    //{
                    //    command.Parameters.AddRange(new SQLiteParameter[0]);
                    //}
                    try
                    {
                        flag = command.ExecuteNonQuery() > 0;
                    }
                    catch (Exception ex)
                    {
                        
                    }
                  
                    conn.Close();
                }
                // MessageAdd("填数据:" + flag+" "+sql+Newtonsoft.Json.JsonConvert.SerializeObject(prams.ToArray()));
            }
            catch (Exception ex)
            {
                MessageAdd("填数据异常:" + ex.Message + ex.StackTrace);
            }
        }

        private static void CreateTab()
        {
            //SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(Path.Combine(dataPath, "Proc"));
            //var conn = new System.Data.SQLite.SQLiteConnection(Path.Combine(txt_DataSourceFilePath.Text, txt_DataSourceFileName.Text));
            //conn.SetPassword("123456");
            var parameters = new List<string[]>();
            parameters.Add(new[] { "ProcID", "varchar", "(50)" });
            parameters.Add(new[] { "ProcName", "varchar", "(200)" });
            parameters.Add(new[] { "ProcInfo", "blob", "(20480)" });
            parameters.Add(new[] { "ProcMD5", "varchar", "(32)" });
            parameters.Add(new[] { "GenerateTime", "varchar", "(50)" });
            parameters.Add(new[] { "CreateTime", "varchar", "(50)" });
            parameters.Add(new[] { "ModifyTime", "varchar", "(50)" });
            ///////////////////////////////////
            MessageAdd("参数完成");
            string path = Path.Combine(dataPath, "Proc2222.db");
            if (!File.Exists(path))
            {
                SQLiteConnection.CreateFile(path);
            }
            var conn = new SQLiteConnection(string.Format("Data Source = {0};version = 3;Password=123123", path));
            
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
