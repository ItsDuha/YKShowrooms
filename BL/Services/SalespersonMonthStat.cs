using System;
using System.Collections.Generic;
using System.Text;

namespace BL.Services
{
    // Salesperson stats for a given month
    public class SalespersonMonthStat
    {
        public string Name { get; set; } = "";
        public int CustomersHandled { get; set; }
        public double Rating { get; set; }   // avg from visitor feedbacks
        public string Month { get; set; } = ""; // "Jan","Feb","Mar","Apr","May"
    }

    // One feedback row for the feedback table
    public class FeedbackItem
    {
        public int Id { get; set; }
        public string VisitorName { get; set; } = "";
        public string Email { get; set; } = "";
        public int Rating { get; set; }
        public string Comment { get; set; } = "";
        public string Showroom { get; set; } = "";
        public string Date { get; set; } = "";   // "yyyy-MM-dd"
        public string Time { get; set; } = "";   // "HH:mm"
    }

    // Monthly visitor counts per channel (Dec–May, 6 entries)
    public class ChannelMonthlyData
    {
        public string Channel { get; set; } = "";
        public List<int> MonthlyCounts { get; set; } = new(); // index 0=Dec … 5=May
    }
}