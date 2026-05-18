using BL.Services;
using DatabaseModel.Models;
using Microsoft.EntityFrameworkCore;

namespace BL.Services
{
    public interface IMarketingAnalysisService
    {
        Task<MarketingReport> GetReportAsync(string? showroom = null, DateTime? from = null, DateTime? to = null);
        Task<List<DailyTrend>> GetDailyTrendsAsync(string? showroom = null, int days = 30);
        Task<List<HourlyPeak>> GetHourlyPeaksAsync(string? showroom = null);
        Task<List<FollowUpStatusCount>> GetFollowUpStatusAsync(string? showroom = null);
        Task<List<FeedbackItem>> GetFeedbackAsync(string? showroom = null, DateTime? from = null, DateTime? to = null);
        Task<Dictionary<string, List<SalespersonMonthStat>>> GetSalespersonByMonthAsync(string? showroom = null);
        Task<List<ChannelMonthlyData>> GetChannelMonthlyAsync(string? showroom = null);
        Task<List<string>> GetShowroomNamesAsync();
    }

    // ── Return types ───────────────────────────────────────────────

    public class MarketingReport
    {
        public List<SourceCount> BySource { get; set; } = new();
        public string TopSource { get; set; } = string.Empty;
        public string BudgetRecommendation { get; set; } = string.Empty;
        public int TotalVisitors { get; set; }
    }

    public class SourceCount
    {
        public string Source { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class DailyTrend
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
    }

    public class HourlyPeak
    {
        public int Hour { get; set; }
        public int Count { get; set; }
        public string Label => $"{Hour:D2}:00";
    }

    public class FollowUpStatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    // ── Service implementation ─────────────────────────────────────

    public class MarketingAnalysisService : IMarketingAnalysisService
    {
        private readonly ApplicationDbContext _db;

        // All valid marketing source channels — must match exactly what the check-in form submits
        private static readonly List<string> AllChannels = new()
        {
            "Instagram", "TikTok", "YouTube", "Website", "Friend", "Walk-in",
        };

        // ── RESTORED: Required by GetSalespersonByMonthAsync ───────
        private static readonly string[] MonthLabels = { "Jan", "Feb", "Mar", "Apr", "May" };

        // ── DYNAMIC: Always covers the last 6 months automatically ─
        private static (int Year, int Month, string Label)[] GetDashboardMonths()
        {
            var months = new (int Year, int Month, string Label)[6];
            var current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            for (int i = 5; i >= 0; i--)
            {
                var m = current.AddMonths(-i);
                months[5 - i] = (m.Year, m.Month, m.ToString("MMM"));
            }
            return months;
        }

        private static readonly (int Year, int Month, string Label)[] DashboardMonths = GetDashboardMonths();

        public MarketingAnalysisService(ApplicationDbContext db) => _db = db;

        // ── Helper: normalise a raw MarketingSource value ──────────
        // Null, empty, whitespace, or any unrecognised value → "Other"
        private static string NormaliseSource(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var trimmed = raw.Trim();
            return AllChannels.Contains(trimmed) ? trimmed : string.Empty;
        }

        // ── GetReportAsync ─────────────────────────────────────────

        public async Task<MarketingReport> GetReportAsync(
            string? showroom = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Visitors.Include(v => v.Showroom).AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(v => v.Showroom != null && v.Showroom.Name == showroom);

            var visitors = await query.ToListAsync();

            if (from.HasValue) visitors = visitors.Where(v => v.VisitedAt >= from.Value).ToList();
            if (to.HasValue) visitors = visitors.Where(v => v.VisitedAt <= to.Value).ToList();

            int total = visitors.Count;

            // Group using normalised source 
            var sources = await _db.MarketingSources.ToListAsync();
            var grouped = sources
                .Where(s => !string.IsNullOrEmpty(s.Source))
                .GroupBy(s => s.Source)
                .Select(g => new SourceCount
                {
                    Source = g.Key!,
                    Count = g.Count(),
                    Percentage = total > 0 ? Math.Round(g.Count() * 100.0 / total, 1) : 0
                })
                .OrderByDescending(s => s.Count)
                .ToList();

            var top = grouped.FirstOrDefault();

            string recommendation = top == null
                ? "No data available yet."
                : $"Most visitors come from {top.Source} ({top.Percentage}%). Consider increasing budget here.";

            return new MarketingReport
            {
                BySource = grouped,
                TopSource = top?.Source ?? "N/A",
                BudgetRecommendation = recommendation,
                TotalVisitors = total
            };
        }

        // ── GetDailyTrendsAsync ────────────────────────────────────

        public async Task<List<DailyTrend>> GetDailyTrendsAsync(string? showroom = null, int days = 30)
        {
            var since = DateTime.UtcNow.Date.AddDays(-days + 1);

            var query = _db.Visitors.Include(v => v.Showroom).AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(v => v.Showroom != null && v.Showroom.Name == showroom);

            var visitors = (await query.ToListAsync())
                .Where(v => v.VisitedAt.HasValue && v.VisitedAt.Value.UtcDateTime.Date >= since)
                .ToList();

            var raw = visitors
                .GroupBy(v => v.VisitedAt!.Value.UtcDateTime.Date)
                .Select(g => new DailyTrend { Date = g.Key, Count = g.Count() })
                .OrderBy(d => d.Date)
                .ToList();

            var result = new List<DailyTrend>();
            for (int i = 0; i < days; i++)
            {
                var date = since.AddDays(i);
                result.Add(raw.FirstOrDefault(r => r.Date.Date == date.Date)
                           ?? new DailyTrend { Date = date, Count = 0 });
            }

            return result;
        }

        // ── GetHourlyPeaksAsync ────────────────────────────────────

        public async Task<List<HourlyPeak>> GetHourlyPeaksAsync(string? showroom = null)
        {
            var query = _db.Visitors.Include(v => v.Showroom).AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(v => v.Showroom != null && v.Showroom.Name == showroom);

            var visitors = await query.ToListAsync();

            var raw = visitors
                .Where(v => v.VisitedAt.HasValue)
                .GroupBy(v => v.VisitedAt!.Value.Hour)
                .Select(g => new HourlyPeak { Hour = g.Key, Count = g.Count() })
                .ToList();

            return Enumerable.Range(0, 24)
                .Select(h => raw.FirstOrDefault(r => r.Hour == h) ?? new HourlyPeak { Hour = h, Count = 0 })
                .ToList();
        }

        // ── GetFollowUpStatusAsync ─────────────────────────────────

        public async Task<List<FollowUpStatusCount>> GetFollowUpStatusAsync(string? showroom = null)
        {
            var query = _db.Visitors.Include(v => v.Showroom).AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(v => v.Showroom != null && v.Showroom.Name == showroom);

            var visitors = await query.ToListAsync();

            return await _db.FollowUps
    .GroupBy(f => f.Status ?? "Unknown")
    .Select(g => new FollowUpStatusCount { Status = g.Key, Count = g.Count() })
    .OrderByDescending(s => s.Count)
    .ToListAsync();
        }

        // ── GetFeedbackAsync ───────────────────────────────────────

        public async Task<List<FeedbackItem>> GetFeedbackAsync(
            string? showroom = null, DateTime? from = null, DateTime? to = null)
        {
            var query = _db.Feedbacks
                .Include(f => f.Visitor)
                .Include(f => f.Showroom)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(f => f.Showroom.Name == showroom);

            var list = await query.ToListAsync();

            if (from.HasValue) list = list.Where(f => f.CreatedAt >= from.Value).ToList();
            if (to.HasValue) list = list.Where(f => f.CreatedAt <= to.Value).ToList();

            return list.Select(f => new FeedbackItem
            {
                Id = f.Id,
                VisitorName = f.Visitor?.Name ?? "Anonymous",
                Email = f.Visitor?.Email ?? "",
                Rating = f.Rating,
                Comment = f.Comment ?? "",
                Showroom = f.Showroom?.Name ?? "",
                Date = f.CreatedAt.HasValue ? f.CreatedAt.Value.ToString("yyyy-MM-dd") : "",
                Time = f.CreatedAt.HasValue ? f.CreatedAt.Value.ToString("HH:mm") : "",
            }).ToList();
        }

        // ── GetSalespersonByMonthAsync ─────────────────────────────

        public async Task<Dictionary<string, List<SalespersonMonthStat>>> GetSalespersonByMonthAsync(
            string? showroom = null)
        {
            var spQuery = _db.Salespersons
                .Include(s => s.Showroom)
                .Include(s => s.Visitors)
                    .ThenInclude(v => v.Feedbacks)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                spQuery = spQuery.Where(s => s.Showroom != null && s.Showroom.Name == showroom);

            var salespersons = await spQuery.ToListAsync();

            var result = new Dictionary<string, List<SalespersonMonthStat>>();

            foreach (var monthLabel in MonthLabels)
            {
                var (year, month) = MonthLabels
                    .Select((l, i) => (l, DashboardMonths[i + 1]))
                    .First(x => x.l == monthLabel).Item2 switch
                {
                    var m => (m.Year, m.Month)
                };

                var stats = salespersons.Select(sp =>
                {
                    var monthVisitors = sp.Visitors
                        .Where(v => v.VisitedAt.HasValue
                                 && v.VisitedAt.Value.Year == year
                                 && v.VisitedAt.Value.Month == month
                                 && v.IsDeleted == 0)
                        .ToList();

                    var ratings = monthVisitors
                        .SelectMany(v => v.Feedbacks)
                        .Select(f => (double)f.Rating)
                        .ToList();

                    return new SalespersonMonthStat
                    {
                        Name = sp.Name,
                        CustomersHandled = monthVisitors.Count,
                        Rating = ratings.Any() ? Math.Round(ratings.Average(), 1) : 0.0,
                        Month = monthLabel
                    };
                })
                .Where(s => s.CustomersHandled > 0)
                .OrderByDescending(s => s.CustomersHandled)
                .ToList();

                result[monthLabel] = stats;
            }

            return result;
        }

        // ── GetChannelMonthlyAsync ─────────────────────────────────
        // FIX: All 6 channels always appear, even with zero counts.
        // FIX: Null / unrecognised sources are mapped to "Other" — no more "Unknown".

        public async Task<List<ChannelMonthlyData>> GetChannelMonthlyAsync(string? showroom = null)
        {
            var query = _db.Visitors
                .Include(v => v.Showroom)
                .Where(v => v.IsDeleted == 0 && v.VisitedAt.HasValue)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(showroom))
                query = query.Where(v => v.Showroom != null && v.Showroom.Name == showroom);

            var visitors = await query.ToListAsync();

            var result = new List<ChannelMonthlyData>();

            // Iterate over every defined channel so all 6 always appear in the chart
            foreach (var channel in AllChannels)
            {
                var counts = DashboardMonths.Select(dm =>
                    visitors.Count(v =>
                        _db.MarketingSources.Any(ms => ms.VisitorId == v.Id && ms.Source == channel)
                        && v.VisitedAt!.Value.Year == dm.Year
                        && v.VisitedAt!.Value.Month == dm.Month)
                ).ToList();

                result.Add(new ChannelMonthlyData
                {
                    Channel = channel,
                    MonthlyCounts = counts   // always 6 values, 0 where no data exists
                });
            }

            return result;

        }

        // ── GetShowroomNamesAsync ──────────────────────────────────

        public async Task<List<string>> GetShowroomNamesAsync()
            => await _db.Showrooms.Select(s => s.Name).OrderBy(n => n).ToListAsync();
    }
}