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

            // Avg Score
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

            //  FOLLOW-UP STATUS
            var statusData = visitors
                .GroupBy(v => v.FollowUpStatus)
                .Select(g => new
                {
                    status = g.Key,
                    count = g.Count()
                })
                .ToList();

            ViewBag.Contacted = statusData.FirstOrDefault(x => x.status == "contacted")?.count ?? 0;
            ViewBag.Pending = statusData.FirstOrDefault(x => x.status == "pending")?.count ?? 0;
            ViewBag.NotInterested = statusData.FirstOrDefault(x => x.status == "not_interested")?.count ?? 0;

            //VISITOR TYPE (NEW vs RETURNING)
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
            //PEAK HOURS
            var peakHours = visitors
                .Where(v => v.VisitedAt.HasValue)
                .GroupBy(v => v.VisitedAt.Value.Hour)
                .Select(g => new
                {
                    hour = g.Key,
                    count = g.Count()
                })
                .OrderBy(h => h.hour)
                .ToList();

            ViewBag.PeakLabels = peakHours.Select(h => h.hour + ":00").ToList();
            ViewBag.PeakCounts = peakHours.Select(h => h.count).ToList();
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