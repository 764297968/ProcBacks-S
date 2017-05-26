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

namespace ProcBackups
{
    partial class Backup : ServiceBase
    {
        System.Timers.Timer tim;  //计时器
         SqliteHelper.SqliteHelper instance;
        string dataPath = Application.StartupPath;
        public Backup()
        {
            MessageAdd("开始调用");
            InitializeComponent();
            
        }
        private void BackUpProc(object sender, System.Timers.ElapsedEventArgs e)
        {
           
            var sql = "insert into FileInfo values(@fileID,@filemd5,@fileName,@filepath,@fileInfo,@parentID)";
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
                bool result = instance.ExecuteNonQuery(sql, prams);
                MessageAdd("填数据:" + result);
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
            var parameters = new List<string[]>();
            parameters.Add(new[] { "ProcID", "varchar", "(50)" });
            parameters.Add(new[] { "ProcName", "varchar", "(200)" });
            parameters.Add(new[] { "ProcInfo", "blob", "(20480)" });
            parameters.Add(new[] { "ProcMD5", "varchar", "(32)" });
            parameters.Add(new[] { "CreateTime", "varchar", "(50)" });
            parameters.Add(new[] { "ModifyTime", "varchar", "(50)" });
            ///////////////////////////////////
            MessageAdd("参数完成");
            var conn = new SQLiteConnection(string.Format("Data Source = {0};version = 3", Path.Combine(dataPath, "Proc.db")));
            string sql = string.Format("insert into {0} values('{1}')", "ProcBackUp", string.Join("','", parameters));
            MessageAdd("sql完成");
            bool flag =false;
            var sqliteparameters = parameters.ToArray();
            try
            {
                using (conn)
                {
                    conn.Open();
                    MessageAdd("conn打开");
                    using (SQLiteCommand command = new SQLiteCommand(sql, conn))
                    {
                        if (sqliteparameters != null)
                        {
                            command.Parameters.AddRange(sqliteparameters);
                        }
                        flag = command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageAdd("异常:"+ ex.Message + ex.StackTrace);
            }
            /////////////
            MessageAdd("创建表:" + flag);
        }
        protected override void OnStart(string[] args)
        {


            tim = new System.Timers.Timer();
            MessageAdd("赋值计时器");

            tim.Elapsed += new System.Timers.ElapsedEventHandler(BackUpProc);
            tim.Interval = 5000;
            tim.Enabled = true;


            MessageAdd("路径"+ Path.Combine(dataPath, "Proc.db"));
            //instance = SqliteHelper.SqliteHelper.Instance;
            MessageAdd("调用createtab方法");
            CreateTab();

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
