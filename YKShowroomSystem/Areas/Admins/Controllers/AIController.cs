using DatabaseModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

public class AIController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpClientFactory _httpClientFactory;

    public AIController(ApplicationDbContext db, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _httpClientFactory = httpClientFactory;
    }

    public IActionResult Index() => View();

    public class MessageDto
    {
        public string Role { get; set; } = "";
        public string Content { get; set; } = "";
    }

    public class ChatRequestDto
    {
        public List<MessageDto> Messages { get; set; } = new();
    }

    [HttpPost]
    public async Task SendMessage([FromBody] ChatRequestDto request)
    {
        if (request.Messages == null || request.Messages.Count == 0)
        {
            Response.StatusCode = 400;
            return;
        }

        var visitors = await _db.Visitors
            .Include(v => v.Showroom)
            .Include(v => v.Product)
            .Include(v => v.Salesperson)
            .Where(v => v.IsDeleted == 0 && v.VisitedAt.HasValue)
            .OrderByDescending(v => v.VisitedAt)
            .ToListAsync();

        var salespersons = await _db.Salespersons
            .Include(s => s.Showroom)
            .Include(s => s.Visitors)
            .ToListAsync();

        var showrooms = await _db.Showrooms.ToListAsync();
        var reserveStaff = await _db.ReserveStaff.OrderBy(r => r.AvailableFrom).ToListAsync();
        var feedback = await _db.Feedbacks.ToListAsync();

        var prompt = BuildSystemPrompt(visitors, salespersons, showrooms, reserveStaff, feedback);

        var messages = new List<object> { new { role = "system", content = prompt } };
        foreach (var m in request.Messages)
            messages.Add(new { role = m.Role, content = m.Content });

        var body = new { model = "llama-3.3-70b-versatile", messages, stream = true };

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var client = _httpClientFactory.CreateClient("groq");
        var req = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            await Response.WriteAsync($"data: Error from AI: {response.StatusCode}\n\n");
            await Response.Body.FlushAsync();
            return;
        }

        using var reader = new StreamReader(await response.Content.ReadAsStreamAsync());
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (!line.StartsWith("data: ")) continue;
            var data = line[6..].Trim();
            if (data == "[DONE]") break;
            try
            {
                var json = JsonDocument.Parse(data);
                var delta = json.RootElement.GetProperty("choices")[0].GetProperty("delta");
                if (!delta.TryGetProperty("content", out var cp)) continue;
                var text = cp.GetString();
                if (!string.IsNullOrEmpty(text))
                {
                    await Response.WriteAsync($"data: {text}\n\n");
                    await Response.Body.FlushAsync();
                }
            }
            catch { }
        }

        await Response.WriteAsync("data: [DONE]\n\n");
        await Response.Body.FlushAsync();
    }

    private string BuildSystemPrompt(
        List<Visitor> visitors,
        List<Salesperson> salespersons,
        List<Showroom> showrooms,
        List<ReserveStaff> reserveStaff,
        List<Feedback> feedback)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var lastWeekStart = weekStart.AddDays(-7);
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var lastMonthStart = monthStart.AddMonths(-1);
        var lastMonthEnd = monthStart.AddDays(-1);
        var threeDaysAgo = now.AddDays(-3);
        int nowHour = now.Hour;

        Func<int, int, string> shift = (f, t) =>
        {
            var p = new List<string>();
            if (f < 12) p.Add("Morning");
            if (f < 17 && t > 12) p.Add("Afternoon");
            if (t > 17) p.Add("Evening");
            return string.Join(" · ", p);
        };

        // SEGMENT VISITORS 
        var pending = visitors.Where(v => v.FollowUpStatus == "pending").ToList();
        var contacted = visitors.Where(v => v.FollowUpStatus == "contacted").ToList();
        var notInterested = visitors.Where(v => v.FollowUpStatus == "not_interested").ToList();

        var overdueVisitors = pending
            .Where(v => v.VisitedAt!.Value <= threeDaysAgo)
            .OrderBy(v => v.VisitedAt)
            .ToList();

        var recentPending = pending
            .Where(v => v.VisitedAt!.Value > threeDaysAgo)
            .OrderByDescending(v => v.VisitedAt)
            .ToList();

        // TIME BUCKETS 
        var todayV = visitors.Where(v => v.VisitedAt!.Value.Date == today).ToList();
        var thisWeekV = visitors.Where(v => v.VisitedAt!.Value.Date >= weekStart).ToList();
        var lastWeekV = visitors.Where(v => v.VisitedAt!.Value.Date >= lastWeekStart && v.VisitedAt!.Value.Date < weekStart).ToList();
        var thisMonthV = visitors.Where(v => v.VisitedAt!.Value.Date >= monthStart).ToList();
        var lastMonthV = visitors.Where(v => v.VisitedAt!.Value.Date >= lastMonthStart && v.VisitedAt!.Value.Date <= lastMonthEnd).ToList();

        // RETURNING VS NEW 
        var phoneGroups = visitors.Where(v => !string.IsNullOrEmpty(v.Phone)).GroupBy(v => v.Phone!).ToList();
        var returningCount = phoneGroups.Count(g => g.Count() > 1);
        var newCount = phoneGroups.Count(g => g.Count() == 1);

        // LEAD SCORE 
        var scored = visitors.Where(v => v.Score > 0).ToList();
        var avgScore = scored.Any() ? Math.Round(scored.Average(v => v.Score!.Value), 1) : 0;
        var topLead = scored.OrderByDescending(v => v.Score).FirstOrDefault();

        // LANGUAGE 
        var byLang = visitors
            .GroupBy(v => string.IsNullOrEmpty(v.Language) ? "Unknown" : v.Language)
            .Select(g => new { g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();

        // BY SHOWROOM & MODEL 
        var bySR = visitors.Where(v => v.Showroom != null)
            .GroupBy(v => v.Showroom!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();

        var byModel = visitors.Where(v => v.Product != null)
            .GroupBy(v => v.Product!.Name)
            .Select(g => new { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();

        //  PEAK DAYS & HOURS 
        var byDay = visitors
            .GroupBy(v => v.VisitedAt!.Value.DayOfWeek)
            .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();

        var byHour = visitors
            .GroupBy(v => v.VisitedAt!.Value.Hour)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count).Take(5).ToList();

        //  TREND SUMMARY 
        var trendDays = visitors
            .Where(v => v.VisitedAt!.Value.Date >= today.AddDays(-29))
            .GroupBy(v => v.VisitedAt!.Value.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count).ToList();
        var avgPerDay = trendDays.Any() ? Math.Round(trendDays.Average(t => t.Count), 1) : 0;
        var bestDay = trendDays.FirstOrDefault();

        //  FEEDBACK 
        var avgRating = feedback.Any() ? Math.Round(feedback.Average(f => f.Rating), 1) : 0;
        var ratingDist = feedback
            .GroupBy(f => f.Rating)
            .Select(g => new { Stars = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Stars).ToList();
        var comments = feedback.Where(f => !string.IsNullOrEmpty(f.Comment)).TakeLast(6).ToList();

        // SALESPERSON STATS 
        var spStats = salespersons.Select(sp =>
        {
            var all = sp.Visitors?.Where(v => v.IsDeleted == 0 && v.VisitedAt.HasValue).ToList() ?? new();
            var pend = all.Count(v => v.FollowUpStatus == "pending");
            var cont = all.Count(v => v.FollowUpStatus == "contacted");
            var nint = all.Count(v => v.FollowUpStatus == "not_interested");
            var ovd = all.Count(v => v.FollowUpStatus == "pending" && v.VisitedAt!.Value <= threeDaysAgo);
            var wk = all.Count(v => v.VisitedAt!.Value.Date >= weekStart);
            var mo = all.Count(v => v.VisitedAt!.Value.Date >= monthStart);
            var cr = all.Count > 0 ? Math.Round((double)cont / all.Count * 100, 1) : 0;
            var load = pend == 0 ? "Free" : pend <= 5 ? "Light" : pend <= 15 ? "Normal" : "Overloaded";
            var topOvd = all
                .Where(v => v.FollowUpStatus == "pending" && v.VisitedAt!.Value <= threeDaysAgo)
                .OrderBy(v => v.VisitedAt)
                .Take(3)
                .Select(v => $"{v.Name}({(int)(now - v.VisitedAt!.Value).TotalDays}d)");
            return new
            {
                sp.Name,
                Phone = sp.Phone ?? "N/A",
                SR = sp.Showroom?.Name ?? "N/A",
                Total = all.Count,
                Week = wk,
                Month = mo,
                Pending = pend,
                Contacted = cont,
                NotInt = nint,
                Overdue = ovd,
                CR = cr,
                Load = load,
                TopOvd = string.Join(", ", topOvd)
            };
        }).OrderByDescending(x => x.Overdue).ThenByDescending(x => x.Pending).ToList();

        // SHOWROOM HEALTH 
        var reserveNow = reserveStaff
            .Where(r => r.AvailableFrom <= nowHour && r.AvailableTo > nowHour).ToList();

        var srHealth = showrooms.Select(sr =>
        {
            var srStaff = salespersons.Where(s => s.ShowroomId == sr.Id).ToList();
            var ovd = overdueVisitors.Count(v => v.ShowroomId == sr.Id);
            var pend = pending.Count(v => v.ShowroomId == sr.Id);
            var cont = contacted.Count(v => v.ShowroomId == sr.Id);
            var wk = thisWeekV.Count(v => v.ShowroomId == sr.Id);
            var mo = thisMonthV.Count(v => v.ShowroomId == sr.Id);
            var tot = visitors.Count(v => v.ShowroomId == sr.Id);
            var cr = tot > 0 ? Math.Round((double)cont / tot * 100, 1) : 0;

            var pkH = thisMonthV
                .Where(v => v.ShowroomId == sr.Id)
                .GroupBy(v => v.VisitedAt!.Value.Hour)
                .OrderByDescending(g => g.Count()).FirstOrDefault();

            var pkD = thisMonthV
                .Where(v => v.ShowroomId == sr.Id)
                .GroupBy(v => v.VisitedAt!.Value.DayOfWeek)
                .OrderByDescending(g => g.Count()).FirstOrDefault();

            var pkRes = pkH != null
                ? reserveStaff
                    .Where(r => r.AvailableFrom <= pkH.Key && r.AvailableTo > pkH.Key)
                    .Select(r => r.Name)
                : Enumerable.Empty<string>();

            var status = srStaff.Count == 0 ? "Unstaffed"
                       : ovd == 0 ? "On Track"
                       : ovd < 15 ? "Needs Attention"
                       : "Critical";

            var action = ovd == 0 ? "No action needed"
                       : ovd <= 5 ? "Remind staff to call today (1-5 overdue)"
                       : ovd <= 14 ? "Prioritise callbacks (6-14 overdue)"
                       : "Call reserve staff — full day coverage needed (15+ overdue)";

            return new
            {
                sr.Name,
                Phone = sr.Phone ?? "N/A",
                Loc = sr.Location ?? "N/A",
                StaffCount = srStaff.Count,
                StaffNames = string.Join(", ", srStaff.Select(s => s.Name)),
                Total = tot,
                Week = wk,
                Month = mo,
                Pending = pend,
                Overdue = ovd,
                Contacted = cont,
                CR = cr,
                Status = status,
                Action = action,
                BusiestDay = pkD?.Key.ToString() ?? "N/A",
                PeakHour = pkH != null ? $"{pkH.Key}:00" : "N/A",
                PeakRes = string.Join(", ", pkRes)
            };
        }).ToList();

        //  BUILD PROMPT 
        var sb = new StringBuilder();

        sb.AppendLine("You are the AI assistant for Y.K. Almoayyed & Sons showroom management system in Bahrain.");
        sb.AppendLine("Answer using ONLY the data below. Never invent numbers. Be precise.");
        sb.AppendLine("Reply in the same language the user uses — Arabic or English.");
        sb.AppendLine("Use bullet points for lists. Highlight key numbers with **bold**.");
        sb.AppendLine("For customer lookup: search by name, phone, or CPR in the visitor lists below.");
        sb.AppendLine($"Current time: {now:dddd dd MMM yyyy hh:mm tt}");
        sb.AppendLine();

        // OVERALL STATISTICS
        sb.AppendLine("=== OVERALL STATISTICS ===");
        sb.AppendLine($"Total visitors all time: {visitors.Count}");
        sb.AppendLine($"Today: {todayV.Count} | This week: {thisWeekV.Count} | Last week: {lastWeekV.Count}");
        sb.AppendLine($"This month: {thisMonthV.Count} | Last month: {lastMonthV.Count}");
        sb.AppendLine($"Pending: {pending.Count} | Contacted: {contacted.Count} | Not Interested: {notInterested.Count}");
        sb.AppendLine($"Overdue (pending 3+ days): {overdueVisitors.Count}");
        sb.AppendLine($"New visitors: {newCount} | Returning visitors: {returningCount}");
        sb.AppendLine($"Average lead score: {avgScore}/100");
        if (topLead != null)
            sb.AppendLine($"Highest lead: {topLead.Name} | {topLead.Phone} | Score: {topLead.Score}/100");
        sb.AppendLine($"Avg visitors/day (last 30 days): {avgPerDay}");
        if (bestDay != null)
            sb.AppendLine($"Busiest day (last 30 days): {bestDay.Date:dd MMM yyyy} with {bestDay.Count} visitors");
        sb.AppendLine($"Week-on-week: {thisWeekV.Count} this week vs {lastWeekV.Count} last week");
        sb.AppendLine($"Month-on-month: {thisMonthV.Count} this month vs {lastMonthV.Count} last month");
        sb.AppendLine();

        // LANGUAGE
        sb.AppendLine("=== LANGUAGE BREAKDOWN ===");
        foreach (var l in byLang) sb.AppendLine($"- {l.Key}: {l.Count} visitors");
        sb.AppendLine();

        // BUSIEST DAYS & HOURS
        sb.AppendLine("=== BUSIEST DAYS OF WEEK (all time) ===");
        foreach (var d in byDay) sb.AppendLine($"- {d.Day}: {d.Count} visits");
        sb.AppendLine();

        sb.AppendLine("=== TOP 5 PEAK HOURS (all time) ===");
        foreach (var h in byHour) sb.AppendLine($"- {h.Hour}:00 — {h.Count} visits");
        sb.AppendLine();

        // BY SHOWROOM & MODEL
        sb.AppendLine("=== VISITORS BY SHOWROOM (all time) ===");
        foreach (var s in bySR) sb.AppendLine($"- {s.Name}: {s.Count} visitors");
        sb.AppendLine();

        sb.AppendLine("=== VISITORS BY CAR MODEL (all time) ===");
        foreach (var m in byModel) sb.AppendLine($"- {m.Name}: {m.Count} visitors");
        sb.AppendLine();

        // FEEDBACK
        sb.AppendLine("=== CUSTOMER FEEDBACK ===");
        sb.AppendLine($"Total ratings: {feedback.Count} | Average rating: {avgRating}/5");
        foreach (var r in ratingDist) sb.AppendLine($"- {r.Stars} stars: {r.Count} responses");
        if (comments.Any())
        {
            sb.AppendLine("Recent comments:");
            foreach (var c in comments) sb.AppendLine($"  · \"{c.Comment}\"");
        }
        sb.AppendLine();

        // OVERDUE — full list, oldest first, highest priority
        sb.AppendLine("=== OVERDUE FOLLOW-UPS (pending 3+ days — COMPLETE LIST, oldest first) ===");
        sb.AppendLine($"Total overdue: {overdueVisitors.Count}");
        if (overdueVisitors.Any())
        {
            foreach (var v in overdueVisitors)
                sb.AppendLine($"- {v.Name} | {v.Phone} | Showroom: {v.Showroom?.Name ?? "N/A"} | " +
                              $"Salesperson: {v.Salesperson?.Name ?? "Unassigned"} | " +
                              $"Visited: {v.VisitedAt!.Value:dd MMM yyyy} | " +
                              $"{(int)(now - v.VisitedAt!.Value).TotalDays} days overdue");
        }
        else
        {
            sb.AppendLine("No overdue follow-ups. All pending visitors were contacted within 3 days.");
        }
        sb.AppendLine();

        // RECENT PENDING (not yet overdue)
        if (recentPending.Any())
        {
            sb.AppendLine("=== PENDING — NOT YET OVERDUE (visited within last 3 days) ===");
            foreach (var v in recentPending)
                sb.AppendLine($"- {v.Name} | {v.Phone} | Showroom: {v.Showroom?.Name ?? "N/A"} | " +
                              $"Salesperson: {v.Salesperson?.Name ?? "Unassigned"} | " +
                              $"Visited: {v.VisitedAt!.Value:dd MMM yyyy}");
            sb.AppendLine();
        }

        // CONTACTED — for customer name/phone lookup
        sb.AppendLine("=== RECENTLY CONTACTED VISITORS (latest 30 — for customer lookup) ===");
        sb.AppendLine("Name | Phone | Showroom | Model | Salesperson | Score | Visited");
        foreach (var v in contacted.Take(30))
            sb.AppendLine($"- {v.Name} | {v.Phone} | {v.Showroom?.Name ?? "N/A"} | " +
                          $"{v.Product?.Name ?? "N/A"} | {v.Salesperson?.Name ?? "N/A"} | " +
                          $"Score:{v.Score ?? 0} | {v.VisitedAt!.Value:dd MMM yyyy}");
        sb.AppendLine();

        // SALESPERSON PERFORMANCE
        sb.AppendLine("=== SALESPERSON PERFORMANCE ===");
        sb.AppendLine("Load levels: Free=0 pending | Light=1-5 | Normal=6-15 | Overloaded=16+");
        foreach (var sp in spStats)
        {
            sb.AppendLine($"- {sp.Name} | Phone: {sp.Phone} | Showroom: {sp.SR}");
            sb.AppendLine($"  Total:{sp.Total} | Month:{sp.Month} | Week:{sp.Week} | " +
                          $"Pending:{sp.Pending} | Contacted:{sp.Contacted} | " +
                          $"NotInterested:{sp.NotInt} | Overdue:{sp.Overdue} | " +
                          $"ContactRate:{sp.CR}% | Load:{sp.Load}");
            if (!string.IsNullOrEmpty(sp.TopOvd))
                sb.AppendLine($"  Most overdue customers: {sp.TopOvd}");
        }
        sb.AppendLine();

        // SHOWROOM HEALTH
        sb.AppendLine("=== SHOWROOM HEALTH ===");
        sb.AppendLine("Status rules: On Track=0 overdue | Needs Attention=1-14 | Critical=15+ | Unstaffed=no staff assigned");
        sb.AppendLine("Action rules: 1-5 overdue=remind staff | 6-14=prioritise callbacks | 15+=call reserve staff full day");
        foreach (var sr in srHealth)
        {
            sb.AppendLine($"- {sr.Name} | Phone: {sr.Phone} | Location: {sr.Loc}");
            sb.AppendLine($"  Staff: {sr.StaffCount} ({sr.StaffNames})");
            sb.AppendLine($"  Total:{sr.Total} | Month:{sr.Month} | Week:{sr.Week} | " +
                          $"Pending:{sr.Pending} | Overdue:{sr.Overdue} | " +
                          $"Contacted:{sr.Contacted} | ContactRate:{sr.CR}%");
            sb.AppendLine($"  Status: {sr.Status} | Action: {sr.Action}");
            sb.AppendLine($"  Busiest day: {sr.BusiestDay} | Peak hour: {sr.PeakHour}");
            if (!string.IsNullOrEmpty(sr.PeakRes))
                sb.AppendLine($"  Reserve staff available at peak hour: {sr.PeakRes}");
        }
        sb.AppendLine();

        // RESERVE STAFF
        sb.AppendLine("=== RESERVE STAFF ===");
        sb.AppendLine($"Available right now ({now:hh:mm tt}): {reserveNow.Count}");
        foreach (var r in reserveStaff)
        {
            var tag = reserveNow.Any(x => x.Id == r.Id) ? " [AVAILABLE NOW]" : "";
            sb.AppendLine($"- {r.Name} | Phone: {r.Phone} | " +
                          $"{DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt} | " +
                          $"Shift: {shift(r.AvailableFrom, r.AvailableTo)}{tag}");
        }

        return sb.ToString();
    }
}