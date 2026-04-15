using Microsoft.AspNetCore.Mvc;
using DatabaseModel.Models;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Load page
    public IActionResult Index()
    {
        ViewBag.Showrooms = _context.Showrooms.ToList();
        ViewBag.Products = _context.Products.ToList();
        ViewBag.Salespersons = _context.Salespersons.ToList();

        return View();
    }

    // Save visitor
    [HttpPost]
    public IActionResult Index(Visitor visitor)
    {
       
            // Phone
            if (_context.Visitors.Any(v => v.Phone == visitor.Phone))
            {
                ModelState.AddModelError("Phone", "Phone number already exists");
            }

            // Email 
            if (!string.IsNullOrEmpty(visitor.Email) &&
                _context.Visitors.Any(v => v.Email == visitor.Email))
            {
                ModelState.AddModelError("Email", "Email already exists");
            }

            // CPR 
            if (!string.IsNullOrEmpty(visitor.Cpr) &&
                _context.Visitors.Any(v => v.Cpr == visitor.Cpr))
            {
                ModelState.AddModelError("Cpr", "CPR already exists");
            }

        if (!ModelState.IsValid)
        {
            ViewBag.Showrooms = _context.Showrooms.ToList();
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Salespersons = _context.Salespersons.ToList();

            return View(visitor);
        }
        
        visitor.CreatedAt = DateTimeOffset.Now;
        visitor.VisitedAt = DateTimeOffset.Now;
        visitor.FollowUpStatus = "pending";

        _context.Visitors.Add(visitor);
        _context.SaveChanges();


        // NOTIFICATION (admin)
        _context.Notifications.Add(new Notification
        {
            Type = "new_visitor",
            Message = $"New walk-in: {visitor.Name} at showroom #{visitor.ShowroomId}",
            VisitorId = visitor.Id,
            CreatedAt = DateTime.Now,
            IsRead = false
        });

        _context.SaveChanges();

        return RedirectToAction("Feedback", new { id = visitor.Id });
    }

    //feedback
    public IActionResult Feedback(int id)
    {
        ViewBag.VisitorId = id;
        return View();
    }


    [HttpPost]
    [HttpPost]
    [HttpPost]
    public IActionResult Feedback(int visitorId, int rating, string comment)
    {
        
        if (rating == 0)
        {
            ViewBag.VisitorId = visitorId; 
            ModelState.AddModelError("", "Please select a rating");

            return View(); 
        }

        var visitor = _context.Visitors.FirstOrDefault(v => v.Id == visitorId);

        var feedback = new Feedback
        {
            VisitorId = visitorId,
            ShowroomId = visitor.ShowroomId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTimeOffset.Now
        };

        _context.Feedbacks.Add(feedback);
        _context.SaveChanges();

        return RedirectToAction("ThankYou");
    }

    public IActionResult ThankYou()
    {
        return View();
    }
}