using DatabaseModel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace YKShowroomSystem.Areas.Admins.Controllers
{
    [Area("Admins")]
    public class StaffingController : Controller
    {
        private readonly ApplicationDbContext _db;

        public StaffingController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("AdminId") == null)
                return RedirectToAction("Login", "Account");
            var now = DateTime.Now;
            var startOfWeek = now.AddDays(-7);
            var threeDaysAgo = now.AddDays(-3);
            int currentHour = now.Hour;

            // Helper: convert an hour range into a shift label (used in recommendations)
            Func<int, int, string> shiftLabel = (from, to) =>
            {
                var parts = new System.Collections.Generic.List<string>();
                if (from < 12) parts.Add("Morning");
                if (from < 17 && to > 12) parts.Add("Afternoon");
                if (to > 17) parts.Add("Evening");
                return string.Join(" · ", parts);
            };

            // ADDED: Arabic shift label helper
            Func<int, int, string> shiftLabelAr = (from, to) =>
            {
                var parts = new System.Collections.Generic.List<string>();
                if (from < 12) parts.Add("صباح");
                if (from < 17 && to > 12) parts.Add("ظهر");
                if (to > 17) parts.Add("مساء");
                return string.Join(" · ", parts);
            };

            // LOAD DATA
            var allVisitors = await _db.Visitors
                .Where(v => v.IsDeleted == 0 && v.VisitedAt.HasValue)
                .ToListAsync();

            var weekVisitors = allVisitors
                .Where(v => v.VisitedAt!.Value >= startOfWeek)
                .ToList();

            var overdueVisitors = allVisitors
                .Where(v => string.Equals(v.FollowUpStatus, "pending", StringComparison.OrdinalIgnoreCase)
                         && v.VisitedAt!.Value.DateTime <= threeDaysAgo)
                .ToList();

            var salespersons = await _db.Salespersons
                .Include(s => s.Showroom)
                .OrderBy(s => s.Name)
                .ToListAsync();

            var showrooms = await _db.Showrooms
                .OrderBy(s => s.Name)
                .ToListAsync();

            var reserveStaff = await _db.ReserveStaff
                .OrderBy(r => r.AvailableFrom)
                .ToListAsync();


            var activeShowroomIds = salespersons.Select(s => s.ShowroomId).Distinct().ToList();

            // Only  showrooms that had at least 1 visit this week
            var busiestShowroom = showrooms
                .Select(sr => new
                {
                    Name = sr.Name,
                    Count = weekVisitors.Count(v => v.ShowroomId == sr.Id)
                })
                .Where(x => x.Count > 0)
                .OrderByDescending(x => x.Count)
                .FirstOrDefault();

            var reserveAvailableNow = reserveStaff
                .Where(r => r.AvailableFrom <= currentHour && r.AvailableTo > currentHour)
                .ToList();

            ViewBag.SummaryActiveShowrooms = activeShowroomIds.Count;
            ViewBag.SummaryTotalOverdue = overdueVisitors.Count;
            ViewBag.SummaryBusiestShowroom = busiestShowroom?.Name ?? "—";
            ViewBag.SummaryBusiestCount = busiestShowroom?.Count ?? 0;
            ViewBag.SummaryReserveNow = reserveAvailableNow.Count;
            ViewBag.CurrentHourLabel = now.ToString("hh:00 tt");

            // SHOWROOM CARDS
            ViewBag.ShowroomAdvisory = showrooms.Select(sr =>
            {
                var srWeek = weekVisitors.Where(v => v.ShowroomId == sr.Id).ToList();
                var srOverdue = overdueVisitors.Where(v => v.ShowroomId == sr.Id).ToList();
                var srStaff = salespersons.Where(s => s.ShowroomId == sr.Id).ToList();

                // Peak day and peak hour are derived from this week's visits only.
                // When srWeek is empty (thisWeek = 0), both are null and the view shows "—".
                var peakDay = srWeek
                    .GroupBy(v => v.VisitedAt!.Value.DayOfWeek)
                    .Select(g => new { Day = g.Key.ToString(), Count = g.Count() })
                    .OrderByDescending(g => g.Count)
                    .FirstOrDefault();

                var peakHour = srWeek
                    .GroupBy(v => v.VisitedAt!.Value.Hour)
                    .Select(g => new
                    {
                        Hour = g.Key,
                        Label = DateTime.Today.AddHours(g.Key).ToString("hh:00 tt"),
                        Count = g.Count()
                    })
                    .OrderByDescending(g => g.Count)
                    .FirstOrDefault();

                int overdueCount = srOverdue.Count;
                int staffCount = srStaff.Count;

                // Status thresholds
                string status;
                if (staffCount == 0) status = "Unstaffed";
                else if (overdueCount == 0) status = "On Track";
                else if (overdueCount < 15) status = "Needs Attention";
                else status = "Critical";

                // Reserve staff available during peak hour (empty when no peak hour this week)
                List<ReserveStaff> peakReserve;
                if (peakHour != null)
                {
                    int ph = peakHour.Hour;
                    peakReserve = reserveStaff
                        .Where(r => r.AvailableFrom <= ph && r.AvailableTo > ph)
                        .ToList();
                }
                else
                {
                    peakReserve = new List<ReserveStaff>();
                }

                var overloadedStaff = srStaff.Where(sp =>
                {
                    int pending = weekVisitors.Count(v =>
                        v.SalespersonId == sp.Id &&
                        string.Equals(v.FollowUpStatus, "pending", StringComparison.OrdinalIgnoreCase));
                    return pending >= 16;
                }).ToList();

                // RECOMMENDATION
                string allStaffNames = string.Join(" and ", srStaff.Select(s => s.Name));
                string overloadedNames = string.Join(" and ", overloadedStaff.Select(s => s.Name));
                string peakTimeLabel = peakHour != null ? peakHour.Label : "";

                string reserveFormatted = peakReserve.Any()
                    ? string.Join(", ", peakReserve.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt})"))
                    : string.Join(", ", reserveStaff.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt})"));

                string recommendation;

                if (staffCount == 0)
                {
                    recommendation = "No salespersons are assigned to this showroom. Assign at least one salesperson so follow-ups can be tracked.";
                }
                else if (overdueCount == 0 && overloadedStaff.Count == 0)
                {
                    recommendation = $"All follow-ups are being handled on time. {allStaffNames} {(staffCount == 1 ? "is" : "are")} on top of the workload. No action needed today.";
                }
                else if (overdueCount >= 15)
                {
                    string fullDayCoverage = string.Join(", ", reserveStaff.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt}, <strong>{shiftLabel(r.AvailableFrom, r.AvailableTo)}</strong>)"));

                    recommendation = $"{overdueCount} customers are still waiting for a callback after 3+ days. "
                                   + $"{allStaffNames} must spend the full day on callbacks only. They cannot be serving customers on the showroom floor at the same time. "
                                   + $"Arrange reserve staff to cover the floor for the entire day, not just peak hours, because walk-in customers arrive throughout the day. "
                                   + (!string.IsNullOrEmpty(peakTimeLabel)
                                        ? $"The busiest period is {peakTimeLabel}, make sure coverage is arranged before then. "
                                        : "")
                                   + $"Reserve staff available: {fullDayCoverage}.";
                }
                else if (overdueCount >= 6)
                {
                    recommendation = $"{overdueCount} follow-ups are overdue. {allStaffNames} {(staffCount == 1 ? "is" : "are")} getting stretched between walk-ins and callbacks. "
                                   + $"Prioritise calling back overdue customers today before the list grows further. "
                                   + $"If the number reaches 15 or more, call reserve staff to cover the floor: {reserveFormatted}.";
                }
                else if (overdueCount >= 1)
                {
                    recommendation = $"{overdueCount} customer{(overdueCount > 1 ? "s are" : " is")} still waiting for a callback. This is a small and manageable list. "
                                   + $"Remind {allStaffNames} to make these calls today and update the follow-up status in the system after each one.";
                }
                else if (overloadedStaff.Any())
                {
                    recommendation = $"{overloadedNames} {(overloadedStaff.Count == 1 ? "is" : "are")} carrying a heavy pending load this week. "
                                   + $"No overdue follow-ups yet, but the risk is growing. "
                                   + (!string.IsNullOrEmpty(peakTimeLabel)
                                        ? $"If walk-ins increase at peak time ({peakTimeLabel}), call reserve staff to cover the floor: {reserveFormatted}."
                                        : $"Monitor closely. Reserve staff are available if needed: {reserveFormatted}.");
                }
                else
                {
                    recommendation = $"All follow-ups are being handled on time. {allStaffNames} {(staffCount == 1 ? "is" : "are")} on top of the workload. No action needed today.";
                }

                // ADDED: Arabic recommendation — same logic, same variables, Arabic text only
                // Arabic staff name connectors
                string allStaffNamesAr = string.Join(" و", srStaff.Select(s => s.Name));
                string overloadedNamesAr = string.Join(" و", overloadedStaff.Select(s => s.Name));

                // Arabic reserve staff formatted (names + times kept as-is, shift label in Arabic)
                string reserveFormattedAr = peakReserve.Any()
                    ? string.Join("، ", peakReserve.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt})"))
                    : string.Join("، ", reserveStaff.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt})"));

                string recommendationAr;

                if (staffCount == 0)
                {
                    recommendationAr = "لا يوجد مندوبو مبيعات معيّنون لهذا المعرض. قم بتعيين مندوب مبيعات واحد على الأقل حتى يمكن تتبع المتابعات.";
                }
                else if (overdueCount == 0 && overloadedStaff.Count == 0)
                {
                    recommendationAr = $"جميع المتابعات تُعالَج في الوقت المحدد. {allStaffNamesAr} {(staffCount == 1 ? "يتحكم" : "يتحكمون")} في عبء العمل. لا يلزم اتخاذ أي إجراء اليوم.";
                }
                else if (overdueCount >= 15)
                {
                    string fullDayCoverageAr = string.Join("، ", reserveStaff.Select(r =>
                        $"{r.Name} ({DateTime.Today.AddHours(r.AvailableFrom):hh:00 tt}–{DateTime.Today.AddHours(r.AvailableTo):hh:00 tt}, <strong>{shiftLabelAr(r.AvailableFrom, r.AvailableTo)}</strong>)"));

                    recommendationAr = $"لا يزال {overdueCount} عميل ينتظرون الاتصال بهم منذ أكثر من 3 أيام. "
                                     + $"يجب على {allStaffNamesAr} قضاء اليوم كاملاً في الاتصال بالعملاء فقط، ولا يمكنهم في الوقت ذاته خدمة العملاء في أرضية المعرض. "
                                     + $"رتّب الموظفين الاحتياطيين لتغطية المعرض طوال اليوم، وليس فقط في ساعات الذروة، لأن العملاء يصلون على مدار اليوم. "
                                     + (!string.IsNullOrEmpty(peakTimeLabel)
                                          ? $"أكثر الأوقات ازدحاماً هو {peakTimeLabel}، تأكد من ترتيب التغطية قبل ذلك. "
                                          : "")
                                     + $"الموظفون الاحتياطيون المتاحون: {fullDayCoverageAr}.";
                }
                else if (overdueCount >= 6)
                {
                    recommendationAr = $"{overdueCount} متابعة متأخرة. {allStaffNamesAr} {(staffCount == 1 ? "يتنقل" : "يتنقلون")} بين خدمة العملاء الزائرين والاتصال بالعملاء المعلّقين. "
                                     + $"أعطِ الأولوية للاتصال بالعملاء المتأخرين اليوم قبل أن تطول القائمة. "
                                     + $"إذا وصل العدد إلى 15 أو أكثر، اتصل بالموظفين الاحتياطيين لتغطية المعرض: {reserveFormattedAr}.";
                }
                else if (overdueCount >= 1)
                {
                    recommendationAr = $"لا يزال {overdueCount} {(overdueCount > 1 ? "عملاء ينتظرون" : "عميل ينتظر")} الاتصال بهم. هذه قائمة صغيرة يمكن إدارتها. "
                                     + $"ذكّر {allStaffNamesAr} بإجراء هذه المكالمات اليوم وتحديث حالة المتابعة في النظام بعد كل واحدة.";
                }
                else if (overloadedStaff.Any())
                {
                    recommendationAr = $"{overloadedNamesAr} {(overloadedStaff.Count == 1 ? "يحمل" : "يحملون")} عبئاً ثقيلاً من المتابعات المعلّقة هذا الأسبوع. "
                                     + $"لا توجد متابعات متأخرة حتى الآن، لكن الخطر في ازدياد. "
                                     + (!string.IsNullOrEmpty(peakTimeLabel)
                                          ? $"إذا ازداد عدد الزوار في وقت الذروة ({peakTimeLabel})، اتصل بالموظفين الاحتياطيين لتغطية المعرض: {reserveFormattedAr}."
                                          : $"راقب الوضع عن كثب. الموظفون الاحتياطيون متاحون عند الحاجة: {reserveFormattedAr}.");
                }
                else
                {
                    recommendationAr = $"جميع المتابعات تُعالَج في الوقت المحدد. {allStaffNamesAr} {(staffCount == 1 ? "يتحكم" : "يتحكمون")} في عبء العمل. لا يلزم اتخاذ أي إجراء اليوم.";
                }

                return new
                {
                    sr.Id,
                    sr.Name,
                    WeekVisitors = srWeek.Count,
                    OverdueCount = overdueCount,
                    StaffCount = staffCount,
                    StaffNames = srStaff.Select(s => new { s.Name, s.Phone }).ToList(),
                    Status = status,
                    PeakDayLabel = peakDay?.Day ?? "N/A",
                    PeakDayCount = peakDay?.Count ?? 0,
                    PeakHourLabel = peakHour?.Label ?? "N/A",
                    PeakHourCount = peakHour?.Count ?? 0,
                    PeakReserve = peakReserve,
                    Recommendation = recommendation,
                    RecommendationAr = recommendationAr,   // ADDED
                };
            })
            .ToList();

            // SALESPERSON WORKLOAD
            ViewBag.SalespersonStats = salespersons.Select(sp =>
            {
                var mine = weekVisitors.Where(v => v.SalespersonId == sp.Id).ToList();

                var pending = mine.Count(v =>
                    string.Equals(v.FollowUpStatus, "pending", StringComparison.OrdinalIgnoreCase));

                var overdue = overdueVisitors.Count(v => v.SalespersonId == sp.Id);

                string loadLevel = pending switch
                {
                    0 => "Free",
                    <= 5 => "Light",
                    <= 15 => "Normal",
                    _ => "Overloaded"
                };

                return new
                {
                    sp.Id,
                    sp.Name,
                    ShowroomName = sp.Showroom?.Name ?? "—",
                    TotalVisitors = mine.Count,
                    Pending = pending,
                    Contacted = mine.Count(v =>
                        string.Equals(v.FollowUpStatus, "contacted", StringComparison.OrdinalIgnoreCase)),
                    Overdue = overdue,
                    LoadLevel = loadLevel,
                    LoadPercent = Math.Min(pending * 100 / 20, 100),
                };
            })
            .OrderByDescending(s => s.Overdue)
            .ThenByDescending(s => s.Pending)
            .ToList();

            // RESERVE STAFF
            ViewBag.ReserveStaffAll = reserveStaff;
            ViewBag.ReserveAvailableNow = reserveAvailableNow;

            return View();
        }
    }
}