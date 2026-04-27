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

    public IActionResult Index()
    {
        return View();
    }

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

        // Load ALL visitors with full related data
        var visitors = await _db.Visitors
            .Include(v => v.Showroom)
            .Include(v => v.Product)
            .OrderByDescending(v => v.VisitedAt)
            .ToListAsync();

        var systemPrompt = BuildSystemPrompt(visitors);

        var allMessages = new List<object>
        {
            new { role = "system", content = systemPrompt }
        };

        foreach (var m in request.Messages)
            allMessages.Add(new { role = m.Role, content = m.Content });

        var body = new
        {
            model = "llama-3.3-70b-versatile",
            messages = allMessages,
            stream = true
        };

        Response.Headers["Content-Type"] = "text/event-stream";
        Response.Headers["Cache-Control"] = "no-cache";
        Response.Headers["X-Accel-Buffering"] = "no";

        var client = _httpClientFactory.CreateClient("groq");

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
        {
            Content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json")
        };

        var response = await client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            await Response.WriteAsync($"data: Error from AI: {response.StatusCode}\n\n");
            await Response.Body.FlushAsync();
            return;
        }

        var stream = await response.Content.ReadAsStreamAsync();
        var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(line) || !line.StartsWith("data: ")) continue;

            var data = line.Substring(6).Trim();
            if (data == "[DONE]") break;

            try
            {
                var json = JsonDocument.Parse(data);
                var choices = json.RootElement.GetProperty("choices");
                if (choices.GetArrayLength() == 0) continue;

                var delta = choices[0].GetProperty("delta");
                if (!delta.TryGetProperty("content", out var contentProp)) continue;

                var text = contentProp.GetString();
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

    private string BuildSystemPrompt(List<Visitor> visitors)
    {
        var now = DateTime.Now;
        var today = now.Date;
        var thisWeekStart = today.AddDays(-(int)today.DayOfWeek);
        var thisMonthStart = new DateTime(today.Year, today.Month, 1);
        var threeDaysAgo = now.AddDays(-3);

        //  STATISTICS 
        var totalVisitors = visitors.Count;
        var todayVisitors = visitors.Count(v => v.VisitedAt.HasValue && v.VisitedAt.Value.Date == today);
        var thisWeekVisitors = visitors.Count(v => v.VisitedAt.HasValue && v.VisitedAt.Value.Date >= thisWeekStart);
        var thisMonthVisitors = visitors.Count(v => v.VisitedAt.HasValue && v.VisitedAt.Value.Date >= thisMonthStart);

        var pending = visitors.Where(v => v.FollowUpStatus == "Pending").ToList();
        var contacted = visitors.Where(v => v.FollowUpStatus == "Contacted").ToList();
        var notInterested = visitors.Where(v => v.FollowUpStatus == "Not Interested").ToList();
        var overdue = visitors.Where(v => v.FollowUpStatus == "Pending" && v.VisitedAt.HasValue && v.VisitedAt.Value <= threeDaysAgo).ToList();

        // BY SHOWROOM 
        var byShowroom = visitors
            .Where(v => v.Showroom != null)
            .GroupBy(v => v.Showroom.Name)
            .Select(g => new { Showroom = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        //  BY MODEL/PRODUCT 
        var byModel = visitors
            .Where(v => v.Product != null)
            .GroupBy(v => v.Product.Name)
            .Select(g => new { Model = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        //PEAK HOUR 
        var peakHour = visitors
            .Where(v => v.VisitedAt.HasValue)
            .GroupBy(v => v.VisitedAt!.Value.Hour)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();

        var sb = new StringBuilder();

        sb.AppendLine("You are an intelligent assistant for a showroom management system.");
        sb.AppendLine("You have access to the full visitor database below. Use this data to answer any question the admin asks.");
        sb.AppendLine("Respond in the same language the user writes in — Arabic or English.");
        sb.AppendLine("Be concise, clear, and use bullet points for lists. Never make up data.");
        sb.AppendLine($"Current date and time: {now:dddd, dd MMM yyyy HH:mm}");
        sb.AppendLine();

        sb.AppendLine("== OVERALL STATISTICS ==");
        sb.AppendLine($"Total visitors (all time): {totalVisitors}");
        sb.AppendLine($"Today's visitors: {todayVisitors}");
        sb.AppendLine($"This week: {thisWeekVisitors}");
        sb.AppendLine($"This month: {thisMonthVisitors}");
        sb.AppendLine($"Pending follow-up: {pending.Count}");
        sb.AppendLine($"Contacted: {contacted.Count}");
        sb.AppendLine($"Not Interested: {notInterested.Count}");
        sb.AppendLine($"Overdue (pending 3+ days): {overdue.Count}");
        if (peakHour != null)
            sb.AppendLine($"Busiest hour of day: {peakHour.Key}:00 ({peakHour.Count()} visits)");
        sb.AppendLine();

        sb.AppendLine("== VISITORS BY SHOWROOM ==");
        foreach (var s in byShowroom)
            sb.AppendLine($"- {s.Showroom}: {s.Count} visitors");
        sb.AppendLine();

        sb.AppendLine("== VISITORS BY MODEL/PRODUCT ==");
        foreach (var m in byModel)
            sb.AppendLine($"- {m.Model}: {m.Count} visitors");
        sb.AppendLine();

        if (overdue.Any())
        {
            sb.AppendLine("== OVERDUE FOLLOW-UPS (pending 3+ days) ==");
            foreach (var v in overdue.Take(20))
                sb.AppendLine($"- {v.Name} | Phone: {v.Phone} | Showroom: {v.Showroom?.Name} | Visited: {v.VisitedAt:dd MMM yyyy}");
            sb.AppendLine();
        }

        sb.AppendLine("== ALL VISITORS (most recent first) ==");
        foreach (var v in visitors.Take(100))
        {
            sb.AppendLine($"- Name: {v.Name} | Phone: {v.Phone} | Showroom: {v.Showroom?.Name ?? "N/A"} | " +
                          $"Model: {v.Product?.Name ?? "N/A"} | Status: {v.FollowUpStatus} | " +
                          $"Visited: {(v.VisitedAt.HasValue ? v.VisitedAt.Value.ToString("dd MMM yyyy HH:mm") : "N/A")}");
        }

        return sb.ToString();
    }
}