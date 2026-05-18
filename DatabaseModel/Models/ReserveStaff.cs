using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseModel.Models
{
    public class ReserveStaff
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Phone { get; set; } = "";
        public int AvailableFrom { get; set; }
        public int AvailableTo { get; set; }
    }
}