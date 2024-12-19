using System;
using System.Collections.Generic;

namespace ShareCare.Module
{
    public class Payment
    {
        public int PaymentID { get; set; }
        public decimal Amount { get; set; }
        public string Summary { get; set; }
        public int PeopleAmount { get; set; }
        public string Link { get; set; }
        public int TaskID { get; set; }
        public List<int> UserIDs { get; set; } = new List<int>();
    }
}
