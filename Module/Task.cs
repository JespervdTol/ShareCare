using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareCare.Module
{
    internal class Task
    {
        public int TaskID { get; set; }
        public string? Name { get; set; }
        public DateOnly Date { get; set; }
    }
}
