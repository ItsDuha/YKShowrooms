using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DatabaseModel.Models;

namespace YourProjectName.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            ApplicationDbContext context,
            ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // =========================
        // LOAD PAGE
        // =========================
        public IActionResult Index()
        {
            ViewBag.Showrooms = _context.Showrooms.ToList();
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Salespersons = _context.Salespersons.ToList();

            return View();
        }

        // =========================
        // SAVE VISITOR
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(
            Visitor visitor,
            string marketingSource,
            string? marketingSourceOther)
        {
            // PHONE DUPLICATE
            if (_context.Visitors.Any(v =>
                v.Phone == visitor.Phone))
            {
                ModelState.AddModelError(
                    "Phone",
                    "Phone number already exists");
            }

            // EMAIL DUPLICATE
            if (!string.IsNullOrEmpty(visitor.Email)
                &&
                _context.Visitors.Any(v =>
                    v.Email == visitor.Email))
            {
                ModelState.AddModelError(
                    "Email",
                    "Email already exists");
            }

            // CPR DUPLICATE
            if (!string.IsNullOrEmpty(visitor.Cpr)
                &&
                _context.Visitors.Any(v =>
                    v.Cpr == visitor.Cpr))
            {
                ModelState.AddModelError(
                    "Cpr",
                    "CPR already exists");
            }

            // VALIDATION
            if (!ModelState.IsValid)
            {
                ViewBag.Showrooms =
                    _context.Showrooms.ToList();

                ViewBag.Products =
                    _context.Products.ToList();

                ViewBag.Salespersons =
                    _context.Salespersons.ToList();

                return View(visitor);
            }

            try
            {
                // DEFAULT VALUES
                visitor.CreatedAt =
                    DateTimeOffset.Now;

                visitor.VisitedAt =
                    DateTimeOffset.Now;

                visitor.FollowUpStatus =
                    "pending";

                // =========================
                // AUTO LEAD SCORE
                // =========================
                visitor.Score =
                    CalculateLeadScore(visitor);

                // SAVE VISITOR
                _context.Visitors.Add(visitor);

                _context.SaveChanges();

                // =========================
                // FOLLOW UP
                // =========================
                _context.FollowUps.Add(
                    new FollowUp
                    {
                        VisitorId = visitor.Id,

                        Status = "pending",

                        Score = visitor.Score ?? 0,

                        CreatedAt =
                            DateTimeOffset.Now
                    });

                // =========================
                // MARKETING SOURCE
                // =========================
                if (!string.IsNullOrEmpty(
                    marketingSource))
                {
                    _context.MarketingSources.Add(
                        new MarketingSource
                        {
                            VisitorId = visitor.Id,

                            Source = marketingSource,

                            SourceOther =
                                marketingSourceOther,

                            CreatedAt =
                                DateTimeOffset.Now
                        });
                }

                // =========================
                // NOTIFICATION 1
                // =========================
                _context.Notifications.Add(
                    new Notification
                    {
                        Type = "new_visitor",

                        Message =
                        visitor.IsReturning == true
                        ?
                        $"Returning walk-in: {visitor.Name} at showroom #{visitor.ShowroomId}"
                        :
                        $"New walk-in: {visitor.Name} at showroom #{visitor.ShowroomId}",

                        VisitorId = visitor.Id,

                        CreatedAt =
                            DateTimeOffset.Now,

                        IsRead = false
                    });

                // =========================
                // NOTIFICATION 2
                // =========================
                if ((visitor.Score ?? 0) >= 70)
                {
                    _context.Notifications.Add(
                        new Notification
                        {
                            Type = "high_score_lead",

                            Message =
                            $"High-priority lead: {visitor.Name} scored {visitor.Score}/100. Please contact this customer soon.",

                            VisitorId = visitor.Id,

                            CreatedAt =
                                DateTimeOffset.Now,

                            IsRead = false
                        });
                }

                // =========================
                // NOTIFICATION 3 VIP
                // =========================
                if (visitor.IsReturning == true
                    &&
                    (visitor.Score ?? 0) >= 70)
                {
                    _context.Notifications.Add(
                        new Notification
                        {
                            Type = "vip_customer",

                            Message =
                            $"VIP customer detected: {visitor.Name} is a returning customer with score {visitor.Score}/100.",

                            VisitorId = visitor.Id,

                            CreatedAt =
                                DateTimeOffset.Now,

                            IsRead = false
                        });
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error saving visitor.");

                ModelState.AddModelError(
                    "",
                    "An error occurred while saving. Please try again.");

                ViewBag.Showrooms =
                    _context.Showrooms.ToList();

                ViewBag.Products =
                    _context.Products.ToList();

                ViewBag.Salespersons =
                    _context.Salespersons.ToList();

                return View(visitor);
            }

            return RedirectToAction(
                "Feedback",
                new { id = visitor.Id });
        }

        // =========================
        // FEEDBACK PAGE
        // =========================
        public IActionResult Feedback(int id)
        {
            ViewBag.VisitorId = id;

            return View();
        }

        // =========================
        // SAVE FEEDBACK
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Feedback(
            int visitorId,
            int rating,
            string comment)
        {
            if (rating == 0)
            {
                ViewBag.VisitorId =
                    visitorId;

                ModelState.AddModelError(
                    "",
                    "Please select a rating");

                return View();
            }

            var visitor =
                _context.Visitors.FirstOrDefault(v =>
                    v.Id == visitorId);

            if (visitor == null)
            {
                return NotFound(
                    $"Visitor with ID {visitorId} not found.");
            }

            try
            {
                _context.Feedbacks.Add(
                    new Feedback
                    {
                        VisitorId = visitorId,

                        ShowroomId =
                            visitor.ShowroomId,

                        Rating = rating,

                        Comment = comment,

                        CreatedAt =
                            DateTimeOffset.Now
                    });

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error saving feedback.");

                ViewBag.VisitorId =
                    visitorId;

                ModelState.AddModelError(
                    "",
                    "An error occurred while saving feedback.");

                return View();
            }

            return RedirectToAction("ThankYou");
        }

        // =========================
        // THANK YOU PAGE
        // =========================
        public IActionResult ThankYou()
        {
            return View();
        }

        // =========================
        // AUTO LEAD SCORING
        // =========================
        private int CalculateLeadScore(Visitor visitor)
        {
            int score = 0;

            // SHOWROOM
            if (visitor.ShowroomId != 0)
                score += 10;

            // PRODUCT
            if (visitor.ProductId != null)
                score += 25;

            // SALESPERSON
            if (visitor.SalespersonId != null)
                score += 15;

            // EMAIL
            if (!string.IsNullOrWhiteSpace(
                visitor.Email))
                score += 10;

            // CPR
            if (!string.IsNullOrWhiteSpace(
                visitor.Cpr))
                score += 15;

            // NOTES
            if (!string.IsNullOrWhiteSpace(
                visitor.Notes))
                score += 10;

            // RETURNING CUSTOMER
            bool isReturning =
                _context.Visitors.Any(v =>

                    (!string.IsNullOrWhiteSpace(visitor.Cpr)
                    &&
                    v.Cpr == visitor.Cpr)

                    ||

                    (!string.IsNullOrWhiteSpace(visitor.Phone)
                    &&
                    v.Phone == visitor.Phone)
                );

            if (isReturning)
            {
                score += 15;

                visitor.IsReturning = true;
            }
            else
            {
                visitor.IsReturning = false;
            }

            return Math.Min(score, 100);
        }
    }
}