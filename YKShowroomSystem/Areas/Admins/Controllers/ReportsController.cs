using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseModel.Models;

namespace YKShowroomSystem.Areas.Admins.Controllers
{
    [Area("Admins")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string tab, string search, int? showroomId, string status, int page = 1, int? range = 30)
        {
            int pageSize = 8;

            ViewBag.Tab = tab ?? "daily";

            var query = _context.Visitors
                .Include(v => v.Showroom)
                .Where(v => v.IsDeleted == 0)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                query = query.Where(v =>
                    (v.Name != null && v.Name.ToLower().Contains(search)) ||
                    (v.Phone != null && v.Phone.Replace(" ", "").Contains(search.Replace(" ", "")))
                );
            }

            // TIME FILTER (ONLY FOR MISSED)
            if ((tab ?? "daily") == "missed" && range != null)
            {
                var fromDate = DateTime.Today.AddDays(-range.Value);

                query = query.Where(v => v.VisitedAt >= fromDate);
            }

            // SHOWROOM
            if (showroomId != null && showroomId != 0)
            {
                query = query.Where(v => v.ShowroomId == showroomId);
            }

            // STATUS (ONLY FOR DAILY)
            if (ViewBag.Tab == "daily")
            {
                if (!string.IsNullOrEmpty(status) && status != "All")
                {
                    query = query.Where(v => v.FollowUpStatus == status);
                }
            }

            //  FOLLOW-UPS FILTER
            if (ViewBag.Tab == "missed")
            {
                query = query.Where(v => v.FollowUpStatus == "pending");
            }

            var total = query.Count();

            var fullList = query
                .OrderByDescending(v => v.VisitedAt)
                .ToList();

            var visitors = fullList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            //overdue
            var today = DateTime.Today;

          

            ViewBag.MissedData = visitors.Select(v => new
            {
                Visitor = v,
                Days = v.VisitedAt.HasValue
                    ? (today - v.VisitedAt.Value.Date).Days
                    : 0
            }).ToList();

            // DELETED

            ViewBag.Deleted = _context.Visitors
            .Where(v => v.IsDeleted == -1 && v.DeletedFrom == (tab ?? "daily"))
            .ToList();
        

            ViewBag.Showrooms = _context.Showrooms.ToList();
            ViewBag.Salespersons = _context.Salespersons.ToList();
            ViewBag.SelectedShowroom = showroomId;
            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;

            ViewBag.Page = page;
            ViewBag.Range = range;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

                return View(visitors);
        }

        // DELETE (SOFT)
        public IActionResult Delete(int id, string tab)
        {
            var visitor = _context.Visitors.Find(id);

            if (visitor != null)
            {
                visitor.IsDeleted = -1;
                visitor.DeletedFrom = tab;

                _context.SaveChanges();

                TempData["msg"] = "Visitor deleted successfully";
            }

            return RedirectToAction("Index", new { tab = tab });
        }

        // RESTORE
        public IActionResult Restore(int id, string tab)
        {
            var v = _context.Visitors.Find(id);

            if (v != null)
            {
                v.IsDeleted = 0;
                _context.SaveChanges();
            }

            TempData["msg"] = "Visitor restored successfully";
            return RedirectToAction("Index", new { tab = tab });
        }

        // UPDATE STATUS
        [HttpPost]
        public IActionResult UpdateStatus(int id, string status)
        {
            var v = _context.Visitors.Find(id);

            if (v != null)
            {
                v.FollowUpStatus = status;
                _context.SaveChanges();
            }

            TempData["msg"] = "Status updated";
            return RedirectToAction("Index");
        }

        //Details page
        public IActionResult Details(int id, string tab)
        {
            var visitor = _context.Visitors
                .Include(v => v.Showroom)
                .FirstOrDefault(v => v.Id == id);

            if (visitor == null)
                return NotFound();

            ViewBag.Tab = tab;

            ViewBag.Showrooms = _context.Showrooms.ToList();
            ViewBag.Products = _context.Products.ToList();
            ViewBag.Salespersons = _context.Salespersons.ToList();

            return View(visitor);
        }
        //Edit
        [HttpPost]
        public IActionResult Details(Visitor model)
        {
            var visitor = _context.Visitors.Find(model.Id);

            if (visitor == null) return NotFound();

            // UPDATE FIELDS
            visitor.Name = model.Name;
            visitor.Phone = model.Phone;
            visitor.Email = model.Email;
            visitor.ShowroomId = model.ShowroomId;
            visitor.ProductId = model.ProductId;
            visitor.FollowUpStatus = model.FollowUpStatus;
            visitor.Score = model.Score;
            visitor.Notes = model.Notes;

            _context.SaveChanges();

            TempData["msg"] = "Visitor updated successfully";

            return RedirectToAction("Details", new { id = model.Id });
        }
        //edit missed report
        [HttpPost]
        public IActionResult UpdateMissed(Visitor model)
        {
            var visitor = _context.Visitors.Find(model.Id);

            if (visitor == null) return NotFound();

            visitor.FollowUpStatus = model.FollowUpStatus;
            visitor.SalespersonId = model.SalespersonId;
            visitor.Notes = model.Notes;

            _context.SaveChanges();

            TempData["msg"] = "Follow-up updated successfully";

            return RedirectToAction("Details", new { id = model.Id, tab = "missed" });
        }

        //Export Daily
        public IActionResult Export(string search, int? showroomId, string status)
        {
            var query = _context.Visitors
                .Include(v => v.Showroom)
                .Where(v => v.IsDeleted == 0)
                .AsQueryable();

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                query = query.Where(v =>
                    (v.Name != null && v.Name.ToLower().Contains(search)) ||
                    (v.Phone != null && v.Phone.Contains(search))
                );
            }

            //SHOWROOM
            if (showroomId != null && showroomId != 0)
            {
                query = query.Where(v => v.ShowroomId == showroomId);
            }

            // STATUS
            if (!string.IsNullOrEmpty(status) && status != "All")
            {
                query = query.Where(v => v.FollowUpStatus == status);
            }

            var data = query.ToList();

            var csv = new System.Text.StringBuilder();

            // HEADER
            csv.AppendLine("Name,Phone,Email,CPR,Showroom,Status,Score,Visit Time,Product,Notes");

            // DATA
            foreach (var v in data)
            {
                csv.AppendLine($"{v.Name},{v.Phone},{v.Email},{v.Cpr},{v.Showroom?.Name},{v.FollowUpStatus},{v.Score},{v.VisitedAt},{v.Product},{v.Notes}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                        "text/csv",
                        "VisitorsReport.csv");
        }
        //Export missed
        public IActionResult ExportMissed(string search, int? range = 30)
        {
            var query = _context.Visitors
                .Include(v => v.Showroom)
                .Where(v => v.IsDeleted == 0 && v.FollowUpStatus == "pending");

            // TIME FILTER
            if (range != null)
            {
                var fromDate = DateTime.Today.AddDays(-range.Value);
                query = query.Where(v => v.VisitedAt >= fromDate);
            }

            // SEARCH
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();

                query = query.Where(v =>
                    (v.Name != null && v.Name.ToLower().Contains(search)) ||
                    (v.Phone != null && v.Phone.Contains(search))
                );
            }

            var data = query.ToList();

            var csv = new System.Text.StringBuilder();

            // HEADER 
            csv.AppendLine("Name,Phone,Showroom,VisitedAt,DaysOverdue");

            var today = DateTime.Today;

            foreach (var v in data)
            {
                var days = v.VisitedAt.HasValue
                    ? (today - v.VisitedAt.Value.Date).Days
                    : 0;

                csv.AppendLine($"{v.Name},{v.Phone},{v.Showroom?.Name},{v.VisitedAt:dd/MM/yyyy HH:mm},{days}");
            }

            return File(
                System.Text.Encoding.UTF8.GetBytes(csv.ToString()),
                "text/csv",
                "MissedFollowUps.csv"
            );
        }
        [HttpPost]
        public IActionResult AssignSalesperson(int visitorId, int? salespersonId)
        {
            var visitor = _context.Visitors.Find(visitorId);

            if (visitor == null)
                return RedirectToAction("Index", new { tab = "missed" });

            //VALIDATE salesperson exists
            var salespersonExists = _context.Salespersons.Any(u => u.Id == salespersonId);

            if (!salespersonExists && salespersonId != null)
            {
                TempData["msg"] = "Invalid salesperson";
                return RedirectToAction("Index", new { tab = "missed" });
            }

            visitor.SalespersonId = salespersonId;

            _context.SaveChanges();

            TempData["msg"] = "Salesperson assigned successfully";

            return RedirectToAction("Index", new { tab = "missed" });
        }
    }
}