using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareCare.Module
{
    public class Event
    {
        public int EventID { get; set; }
        public string Summary { get; set; }
        public DateTime Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public List<Person> Persons { get; set; }
        public List<Room> Rooms { get; set; }
    }
}