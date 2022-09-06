using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace logs_performance
{
    internal class GetLogs
    {
        public int PK { get; set; }
        public string Identify { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
