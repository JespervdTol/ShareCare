using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareCare.Module
{
    public class Person
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string Intersertion  { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateOnly DateOfBirth { get; set; }
        //public List<Event> Events { get; set; } = new List<Event>();
    }
}
