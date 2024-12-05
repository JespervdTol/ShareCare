using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareCare.Module
{
    public class Task
    {
        public int TaskID { get; set; }
        public string Type { get; set; }
        public string Summary { get; set; }
        public DateTime Date { get; set; }
        public string Person { get; set; }
    }
}
