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
using System.Data.SqlClient;
using System.Web.Security;

namespace ProcBackups
{
    partial class Backup : ServiceBase
    {
        System.Timers.Timer tim;  //计时器
        SqliteHelper.SqliteHelper instance;
        static string dataPath = Application.StartupPath;
        static string path = Path.Combine(dataPath, "Proc.db");
        string sqlConnStr = "Data Source=124.207.143.5;Initial Catalog=FMCG_BPMS;Persist Security Info=True;User ID=sa; Password=ylyato99!;Min Pool Size=1;Connection Lifetime=0;Max Pool Size=50;Connection Reset=false;Pooling= true;";
        static string tabName = "ProcBackUp";
        public Backup()
        {
            InitializeComponent();
            MessageAdd("开始调用");
            tim = new System.Timers.Timer();
            MessageAdd("赋值计时器");
            instance = SqliteHelper.SqliteHelper.Instance;
            instance.SetDataSourcePath(path);
            if (!instance.IsExists(tabName))
            {
                CreateTab();
            }
            tim.Elapsed += new System.Timers.ElapsedEventHandler(BackUpProc);
            tim.Interval = 1000*60*60*2;//2个小时执行
            tim.Enabled = true;

        }
        private void BackUpProc(object sender, System.Timers.ElapsedEventArgs e)
        {
            string getProcSql = "select a.name ProcName,"
            + "b.definition ProcInfo, "
            + "a.create_date GenerateTime, "
            + " a.modify_date ModifyTime from sys.all_objects a,sys.sql_modules b where a.is_ms_shipped = 0 and a.object_id = b.object_id and a.[type] in ('P','V','AF') order by a.[name] asc ";
            DataTable dt = ExecuteTable(sqlConnStr, CommandType.Text, getProcSql);
            if (dt.Rows.Count != 0)
            {
                foreach (DataRow item in dt.Rows)
                {
                    try
                    {
                        string procInfo = item["ProcInfo"].ToString();
                        string md5Proc = FormsAuthentication.HashPasswordForStoringInConfigFile(procInfo, "MD5");
                        string procMD5 = FormsAuthentication.HashPasswordForStoringInConfigFile(md5Proc, "SHA1");
                        //string sqlstr = "insert into ProcBackUp values(NULL, '123123', '123123', '123', '123123', '123123', '123123')";
                        object isexits = instance.GetOnly("select 1 from " + tabName + " where ProcMD5='" + procMD5 + "'");
                        if (isexits != null)
                        {
                            MessageAdd("数据未更新，md5: " + procMD5);
                        }
                        else
                        {
                            var sql = "insert into ProcBackUp values(NULL,@ProcName,@ProcInfo,@ProcMD5,@GenerateTime,@ModifyTime,@CreateTime)";
                            SQLiteParameter[] prams = new SQLiteParameter[] {
                                                    new SQLiteParameter("@ProcName",item["ProcName"]),
                                                    new SQLiteParameter("@ProcInfo",item["ProcInfo"]),
                                                    new SQLiteParameter("@ProcMD5",procMD5),
                                                    new SQLiteParameter("@GenerateTime",item["GenerateTime"] ),
                                                    new SQLiteParameter("@ModifyTime",item["ModifyTime"] ),
                                                    new SQLiteParameter("@CreateTime",DateTime.Now)
                                                };

                            bool result = instance.ExecuteNonQuery(sql, prams);
                            MessageAdd("insert数据:" + result + " md5:" + procMD5);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageAdd("helper填数据异常:" + item["ProcName"] + "  " + ex.Message + ex.StackTrace);
                    }
                }
            }

        }
        public static string FormatSqlParameter(SQLiteParameter[] parms)
        {
            Dictionary<string, string> list = new Dictionary<string, string>();
            StringBuilder sb = new StringBuilder();
            foreach (var item in parms)
            {
                sb.Append(item.ParameterName + ":" + item.Value + ",");
            }
            return sb.ToString();
        }
        private void CreateTab()
        {

            var parameters = new List<string[]>();
            parameters.Add(new[] { "ProcID", " INTEGER PRIMARY KEY" });
            parameters.Add(new[] { "ProcName", "varchar", "(200)" });
            parameters.Add(new[] { "ProcInfo", "varchar", "(2000)" });
            parameters.Add(new[] { "ProcMD5", "varchar", "(36)" });
            parameters.Add(new[] { "GenerateTime", "varchar", "(50)" });
            parameters.Add(new[] { "ModifyTime", "varchar", "(50)" });
            parameters.Add(new[] { "CreateTime", "varchar", "(50)" });

            MessageAdd("参数完成");

            try
            {
                List<string> values = new List<string>();
                foreach (string[] strArray in parameters)
                {
                    values.Add(string.Join(" ", strArray));
                }
                string sql1 = string.Format("create table {0}({1})", "ProcBackUp", string.Join(",", values));
                MessageAdd("创建表sql:" + sql1);
                bool flag = instance.CreateTable(tabName, parameters.ToArray());
                MessageAdd("创建表:" + flag);
            }
            catch (Exception ex)
            {
                MessageAdd("建表err" + ex.Message + ex.StackTrace);
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
        protected override void OnContinue()
        {
            try
            {
                this.tim.Start();
                base.OnContinue();
                MessageAdd("服务继续");
            }
            catch (Exception ex)
            {
                MessageAdd("服务继续异常:" + ex.Message);
            }
            
        }

        protected override void OnPause()
        {
            try
            {
                this.tim.Stop();
                base.OnPause();
                MessageAdd("服务暂停");
            }
            catch (Exception ex)
            {
                MessageAdd("服务暂停异常:"+ex.Message);
            }
            
            
        }
        private void MessageAdd(string msg)
        {
            string path = LogHelper.Path;
            LogHelper.WriteFile(path, msg);
        }
        private static DataTable ExecuteTable(string connectionString, CommandType commandType, string txt, params SqlParameter[] param)
        {

            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(txt, conn))
                {
                    cmd.CommandType = commandType;
                    cmd.Parameters.AddRange(param);
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dt);
                        return dt;
                    }
                }
            }
        }

    }
}
