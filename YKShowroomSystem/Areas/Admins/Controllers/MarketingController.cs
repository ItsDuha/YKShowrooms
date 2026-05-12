using BL.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace YKShowroomSystem.Areas.Admins.Controllers
{
    [Area("Admins")]
    public class MarketingController : Controller
    {
        private readonly IMarketingAnalysisService _marketingService;

        private static readonly JsonSerializerOptions _json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public MarketingController(IMarketingAnalysisService marketingService)
        {
            _marketingService = marketingService;
        }

        public async Task<IActionResult> Index(
            string? showroom = null,
            DateTime? from = null,
            DateTime? to = null)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminName")))
                return RedirectToAction("Login", "Account", new { area = "Admins" });

            // ── Existing data ──────────────────────────────────────
            var report = await _marketingService.GetReportAsync(showroom, from, to);
            var dailyTrends = await _marketingService.GetDailyTrendsAsync(showroom);
            var hourlyPeaks = await _marketingService.GetHourlyPeaksAsync(showroom);
            var followUp = await _marketingService.GetFollowUpStatusAsync(showroom);

            // ── New data ───────────────────────────────────────────
            var feedback = await _marketingService.GetFeedbackAsync(showroom, from, to);
            var salespersonDict = await _marketingService.GetSalespersonByMonthAsync(showroom);
            var channelMonthly = await _marketingService.GetChannelMonthlyAsync(showroom);
            var showroomNames = await _marketingService.GetShowroomNamesAsync();

            // ── ViewBag: existing ──────────────────────────────────
            ViewBag.Showroom = showroom;
            ViewBag.From = from?.ToString("yyyy-MM-dd");
            ViewBag.To = to?.ToString("yyyy-MM-dd");
            ViewBag.DailyTrends = dailyTrends;
            ViewBag.HourlyPeaks = hourlyPeaks;
            ViewBag.FollowUp = followUp;

            // ── ViewBag: new (serialized to JSON for JS consumption) ─
            ViewBag.FeedbackJson = JsonSerializer.Serialize(feedback, _json);
            ViewBag.SalespersonJson = JsonSerializer.Serialize(salespersonDict, _json);
            ViewBag.ChannelMonthlyJson = JsonSerializer.Serialize(channelMonthly, _json);
            ViewBag.ShowroomNames = showroomNames;

            // ── Scalar KPIs ────────────────────────────────────────
            ViewBag.AvgRating = feedback.Any()
                ? Math.Round(feedback.Average(f => f.Rating), 1)
                : 0.0;

            // Top salesperson = first in May (already sorted by customers desc)
            ViewBag.TopSalesperson = salespersonDict.TryGetValue("May", out var mayList) && mayList.Any()
                ? mayList[0].Name
                : "N/A";

            ViewBag.BudgetBHD = 2000m;

            return View(report);
        }

        // ── Export actions (placeholders — wire up later) ──────────
        public IActionResult ExportPdf(string? showroom)
        {
            // TODO: generate PDF using a library such as iTextSharp or QuestPDF
            TempData["Info"] = "PDF export coming soon.";
            return RedirectToAction("Index", new { showroom });
        }

        public IActionResult ExportExcel(string? showroom)
        {
            // TODO: generate Excel using ClosedXML or EPPlus
            TempData["Info"] = "Excel export coming soon.";
            return RedirectToAction("Index", new { showroom });
        }

        [HttpPost]
        public IActionResult SendReply(string email, string message)
        {
            // TODO: send email via your email service (SmtpClient / SendGrid etc.)
            TempData["Info"] = "Reply sent successfully.";
            return RedirectToAction("Index");
        }
    }
}