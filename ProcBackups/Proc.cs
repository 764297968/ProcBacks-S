using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcBackups
{
    public class Proc
    {
        public int ProcID { get; set; }
        public string ProcName { get; set; }
        public string ProcInfo { get; set; }
        public string ProcMD5 { get; set; }
        public string GenerateTime { get; set; }
        public string ModifyTime { get; set; }
        public string CreateTime { get; set; }
    }
}
