using Microsoft.AspNetCore.Mvc;
using DatabaseModel.Models;

namespace YKShowroomSystem.Areas.Admins.Controllers
{
    [Area("Admins")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? showroomId)
        {
            // Notifications
            ViewBag.Notifications = _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.UnreadCount = _context.Notifications
                .Count(n => n.IsRead == false);

            // Showrooms dropdown
            ViewBag.Showrooms = _context.Showrooms.ToList();
            ViewBag.SelectedShowroom = showroomId;

            // Base query
            var visitors = _context.Visitors.AsQueryable();

            if (showroomId != null && showroomId != 0)
            {
                visitors = visitors.Where(v => v.ShowroomId == showroomId);
            }

            var today = DateTime.Today;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);
            var monthStart = new DateTime(today.Year, today.Month, 1);

            // KPIs
            ViewBag.TodayCount = visitors.Count(v => v.VisitedAt >= today);
            ViewBag.WeekCount = visitors.Count(v => v.VisitedAt >= weekStart);
            ViewBag.MonthCount = visitors.Count(v => v.VisitedAt >= monthStart);

            // LEAD QUALITY SUMMARY — fixed: explicit null check before comparison
            ViewBag.HighLeads = visitors.Count(v => v.Score != null && v.Score >= 70);
            ViewBag.MediumLeads = visitors.Count(v => v.Score != null && v.Score >= 40 && v.Score < 70);
            ViewBag.LowLeads = visitors.Count(v => v.Score != null && v.Score < 40);

            var totalScoredLeads = visitors.Count(v => v.Score != null);
            ViewBag.HighLeadRate = totalScoredLeads == 0
                ? 0
                : (int)Math.Round((double)ViewBag.HighLeads / totalScoredLeads * 100);

            ViewBag.PriorityMessage = ViewBag.HighLeads > 0
                ? $"{ViewBag.HighLeads} high-priority lead(s) need quick follow-up."
                : "No high-priority leads at the moment.";

            ViewBag.MissedHighLeads = visitors.Count(v =>
                v.Score != null && v.Score >= 70 && v.FollowUpStatus == "pending");

            ViewBag.VipCustomers = visitors.Count(v => v.Score != null && v.Score >= 70 && v.IsReturning == true);

            // MOST REQUESTED CATEGORY
            var topCategory = visitors
                .Where(v => v.Product != null)
                .GroupBy(v => v.Product.Category)
                .Select(g => new { category = g.Key, count = g.Count() })
                .OrderByDescending(x => x.count)
                .FirstOrDefault();

            ViewBag.TopCategory = topCategory?.category ?? "No data";

            // AVG SCORE
            ViewBag.AvgScore = visitors.Any(v => v.Score != null)
                ? (int)visitors.Where(v => v.Score != null).Average(v => v.Score)
                : 0;

            // WALK-IN TRENDS Chart
            // LAST 30 DAYS (BY DAY)
            var last30Days = Enumerable.Range(0, 30)
                .Select(i => DateTime.Today.AddDays(-i))
                .OrderBy(d => d)
                .ToList();

            var dailyData = last30Days.Select(day => new
            {
                date = day.ToString("yyyy-MM-dd"),
                count = visitors.Count(v => v.VisitedAt.HasValue &&
                                            v.VisitedAt.Value.Date == day.Date)
            }).ToList();

            ViewBag.DailyLabels = dailyData.Select(d => d.date).ToList();
            ViewBag.DailyCounts = dailyData.Select(d => d.count).ToList();

            // BY HOUR (TODAY)
            var hourlyData = Enumerable.Range(0, 24)
                .Select(h => new
                {
                    hour = h,
                    count = visitors.Count(v => v.VisitedAt.HasValue &&
                                               v.VisitedAt.Value.Date == DateTime.Today &&
                                               v.VisitedAt.Value.Hour == h)
                }).ToList();

            ViewBag.HourLabels = hourlyData.Select(h => h.hour + ":00").ToList();
            ViewBag.HourCounts = hourlyData.Select(h => h.count).ToList();

            // LEADS BY CAR MODEL
            var carVisitors = visitors
                .Where(v => v.Showroom.Name != "Y.K. Almoayyed & Sons - Electronics and Home Appliances");

            var modelData = carVisitors
                .Where(v => v.ProductId != null)
                .GroupBy(v => v.Product.Name)
                .Select(g => new
                {
                    model = g.Key,
                    count = g.Count()
                })
                .OrderByDescending(x => x.count)
                .ToList();

            ViewBag.ModelLabels = modelData.Select(x => x.model).ToList();
            ViewBag.ModelCounts = modelData.Select(x => x.count).ToList();

            // FOLLOW-UP STATUS
            var statusData = _context.FollowUps
                .GroupBy(f => f.Status)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToList();

            ViewBag.Contacted = statusData.FirstOrDefault(x => x.status == "contacted")?.count ?? 0;
            ViewBag.Pending = statusData.FirstOrDefault(x => x.status == "pending")?.count ?? 0;
            ViewBag.NotInterested = statusData.FirstOrDefault(x => x.status == "not_interested")?.count ?? 0;

            // VISITOR TYPE (NEW vs RETURNING)
            var visitorGroups = visitors
                .Where(v => v.Phone != null)
                .GroupBy(v => v.Phone)
                .Select(g => new
                {
                    phone = g.Key,
                    count = g.Count()
                })
                .ToList();

            int newVisitors = visitorGroups.Count(v => v.count == 1);
            int returningVisitors = visitorGroups.Count(v => v.count > 1);

            ViewBag.NewVisitors = newVisitors;
            ViewBag.ReturningVisitors = returningVisitors;

            // PEAK HOURS — all-time
            var peakHours = visitors
                .Where(v => v.VisitedAt.HasValue)
                .GroupBy(v => v.VisitedAt.Value.Hour)
                .Select(g => new { hour = g.Key, count = g.Count() })
                .OrderBy(h => h.hour)
                .ToList();

            ViewBag.PeakLabels = peakHours.Select(h => h.hour + ":00").ToList();
            ViewBag.PeakCounts = peakHours.Select(h => h.count).ToList();

            // PEAK HOURS — broken down by day of week
            var peakRaw = visitors
                .Where(v => v.VisitedAt.HasValue)
                .Select(v => new
                {
                    DayOfWeek = v.VisitedAt.Value.DayOfWeek,
                    Hour = v.VisitedAt.Value.Hour
                })
                .ToList();

            var dayNames = new[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

            var peakByDay = dayNames.ToDictionary(
                day => day,
                day =>
                {
                    var dow = (DayOfWeek)Array.IndexOf(dayNames, day);
                    return peakRaw
                        .Where(x => x.DayOfWeek == dow)
                        .GroupBy(x => x.Hour)
                        .Select(g => new { hour = g.Key + ":00", count = g.Count() })
                        .OrderBy(x => x.hour)
                        .ToList<object>();
                }
            );

            ViewBag.PeakByDay = System.Text.Json.JsonSerializer.Serialize(peakByDay);

            // SALESPERSON PERFORMANCE — fixed: explicit null check + Score cast
            var topSalespersons = visitors
                .Where(v => v.SalespersonId != null)
                .GroupBy(v => v.Salesperson.Name)
                .Select(g => new
                {
                    name = g.Key,
                    highLeads = g.Count(v => v.Score != null && v.Score >= 20)
                })
                .OrderByDescending(x => x.highLeads)
                .Take(5)
                .ToList();

            ViewBag.SalesLabels = topSalespersons.Select(x => x.name).ToList();
            ViewBag.SalesCounts = topSalespersons.Select(x => x.highLeads).ToList();

            // VISITOR GROWTH BY CHANNEL
            var channelGroups = _context.MarketingSources
                .AsEnumerable()
                .GroupBy(v => v.Source)
                .Select(g => new
                {
                    channel = g.Key,
                    monthlyCounts = new int[]
                    {
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 12),
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 1),
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 2),
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 3),
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 4),
                        g.Count(v => v.CreatedAt.HasValue && v.CreatedAt.Value.Month == 5),
                    }
                })
                .ToList();

            ViewBag.ChannelMonthlyJson = System.Text.Json.JsonSerializer.Serialize(channelGroups);

            return View();
        }

        // Mark all notifications as read
        public IActionResult MarkAllRead()
        {
            var notifs = _context.Notifications
                .Where(n => n.IsRead == false)
                .ToList();

            foreach (var n in notifs)
            {
                n.IsRead = true;
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}