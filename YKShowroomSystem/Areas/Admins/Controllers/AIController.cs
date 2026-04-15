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

        var visitors = await _db.Visitors.Take(20).ToListAsync();
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
        var sb = new StringBuilder();
        sb.AppendLine("You are an assistant for a showroom management system.");
        sb.AppendLine("Only answer based on the data provided. Do not make up information.");
        sb.AppendLine("Respond in the same language the user writes in (Arabic or English).");
        sb.AppendLine("== RECENT VISITORS ==");
        foreach (var v in visitors)
            sb.AppendLine($"- Name: {v.Name}, Phone: {v.Phone}, Status: {v.FollowUpStatus}");
        return sb.ToString();
    }
}