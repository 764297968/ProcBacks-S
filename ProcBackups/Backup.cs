using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TerminalTrance;
using System.Data.SQLite;
using System.IO;
using System.Data;

namespace ProcBackups
{
    partial class Backup : ServiceBase
    {
        System.Timers.Timer tim;  //计时器
         SqliteHelper.SqliteHelper instance;
        string dataPath = Application.StartupPath;
        string tabName = "ProcBackUp";
        public Backup()
        {
            InitializeComponent();
            MessageAdd("开始调用");
            tim = new System.Timers.Timer();
            MessageAdd("赋值计时器");

            tim.Elapsed += new System.Timers.ElapsedEventHandler(BackUpProc);
            tim.Interval = 3000;
            tim.Enabled = true;
            
            MessageAdd("路径" + Path.Combine(dataPath, "Proc.db"));
            //instance = SqliteHelper.SqliteHelper.Instance;
            MessageAdd("调用createtab方法");
            //CreateTab();

        }
        private void BackUpProc(object sender, System.Timers.ElapsedEventArgs e)
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

            string path = Path.Combine(dataPath, "Proc.db");
            try
            {
                SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(path);
                SqliteHelper.SqliteHelper.Instance.ExecuteNonQuery(sql, prams);
            }
            catch (Exception ex)
            {
                MessageAdd("helper填数据异常:" + ex.Message + ex.StackTrace);
            }
            try
            {
                bool flag = false;
                
                var conn = new SQLiteConnection(string.Format("Data Source={0};version=3", path));

                using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                {
                    conn.Open();
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
                    flag = command.ExecuteNonQuery() > 0;
                    conn.Close();
                }
                MessageAdd("填数据:" + flag + " ");
                // MessageAdd("填数据:" + flag+" "+sql+Newtonsoft.Json.JsonConvert.SerializeObject(prams.ToArray()));
            }
            catch (Exception ex)
            {
                MessageAdd("填数据异常:" + ex.Message + ex.StackTrace);
            }
        }
        private void CreateTab()
        {
            //SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(Path.Combine(dataPath, "Proc"));
            //var conn = new System.Data.SQLite.SQLiteConnection(Path.Combine(txt_DataSourceFilePath.Text, txt_DataSourceFileName.Text));
            //conn.SetPassword("123456");

            string path = Path.Combine(dataPath, "Proc.db");
            string tabName = "ProcBackUp";
            var conn = new SQLiteConnection(string.Format("Data Source={0};version=3", path));
            bool isexits = false;

            using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(1) FROM sqlite_master where type='table' and name='" + tabName + "'", conn))
            {
                conn.Open();
                isexits = Convert.ToInt16(cmd.ExecuteScalar()) > 0;
                conn.Close();
            }
            MessageAdd("表是否存在:" + isexits);
            if (!isexits)
            {
                var parameters = new List<string[]>();
                parameters.Add(new[] { "ProcID", "varchar", "(50)" });
                parameters.Add(new[] { "ProcName", "varchar", "(200)" });
                parameters.Add(new[] { "ProcInfo", "varchar", "(2000)" });
                parameters.Add(new[] { "ProcMD5", "varchar", "(36)" });
                parameters.Add(new[] { "GenerateTime", "varchar", "(50)" });
                parameters.Add(new[] { "CreateTime", "varchar", "(50)" });
                parameters.Add(new[] { "ModifyTime", "varchar", "(50)" });
                ///////////////////////////////////
                MessageAdd("参数完成");
                if (!File.Exists(path))
                {
                    SQLiteConnection.CreateFile(path);
                }
                MessageAdd("创建完database");


                //SqliteHelper.SqliteHelper.Instance.SetDataSourcePath(Path.Combine(dataPath, "Proc.db"));
                //MessageAdd("连接");
                //var sqliteparameters = parameters.ToArray();
                //SqliteHelper.SqliteHelper.Instance.CreateTable("FileInfo", sqliteparameters);
                MessageAdd("sql日志");



                MessageAdd("sql完成");
                bool flag = false;
                var sqliteparameters = parameters.ToArray();
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
                        string sql = string.Format("create table {0}({1})", tabName, string.Join(",", values));
                        using (SQLiteCommand command = new SQLiteCommand(sql, conn))
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
                    MessageAdd("异常:" + ex.Message + ex.Source + ex.StackTrace);
                }
                /////////////
                MessageAdd("创建表:" + flag);
            }


        }
        protected override void OnStart(string[] args)
        {
            
            string msg = "启动服务";
            try
            {
                this.tim.Start();
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
            }
            MessageAdd(msg);
        }

        protected override void OnStop()
        {
            string msg = "服务停止";
            try
            {
                this.tim.Start();
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
            }
            MessageAdd(msg);
        }
        private void MessageAdd(string msg)
        {
            string path = LogHelper.Path;
            LogHelper.WriteFile(path, msg);
        }
    }
}
