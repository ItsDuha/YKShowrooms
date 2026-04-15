using BCrypt.Net;
using DatabaseModel.Models;
using Microsoft.AspNetCore.Mvc;
using YKShowroomSystem.Areas.Admins.Models;

namespace YKShowroomSystem.Areas.Admins.Controllers
{
    [Area("Admins")]
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        //LOGIN PAGE
        public IActionResult Login()
        {
            return View();
        }

        // LOGIN 
        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //Check if email exists
            var admin = _context.Admins.FirstOrDefault(a => a.Email == model.Email);

            if (admin == null)
            {
                ModelState.AddModelError("Email", "This email is not registered");
                return View(model);
            }

            //Check password
            if (!BCrypt.Net.BCrypt.Verify(model.Password, admin.PasswordHash))
            {
                ModelState.AddModelError("Password", "Incorrect password");
                return View(model);
            }

            HttpContext.Session.SetString("AdminName", admin.Name);
            HttpContext.Session.SetInt32("AdminId", admin.Id);
            HttpContext.Session.SetString("AdminEmail", admin.Email);

            return RedirectToAction("Index", "Dashboard", new { area = "Admins" });

        }

        // REGISTER PAGE
        public IActionResult Register()
        {
            return View();
        }

       

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            //Check email uniqueness
            if (_context.Admins.Any(a => a.Email == model.Email))
            {
                ModelState.AddModelError("Email", "This email is already registered");
            }

            if (!ModelState.IsValid)
                return View(model);

            var admin = new Admin
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                CreatedAt = DateTimeOffset.Now
            };

            _context.Admins.Add(admin);
            _context.SaveChanges();

            HttpContext.Session.SetString("AdminName", admin.Name);
            HttpContext.Session.SetString("AdminEmail", admin.Email); 
            return RedirectToAction("Login");
        }

         public IActionResult Profile()
        {
            
            if (HttpContext.Session.GetString("AdminName") == null)
            {
                return RedirectToAction("Login");
            }

            // Notifications 
            ViewBag.Notifications = _context.Notifications
                .OrderByDescending(n => n.CreatedAt)
                .Take(5)
                .ToList();

            ViewBag.UnreadCount = _context.Notifications
                .Count(n => n.IsRead == false);

            var adminId = HttpContext.Session.GetInt32("AdminId");

            var admin = _context.Admins.FirstOrDefault(a => a.Id == adminId);

            return View(admin);
        }

        // LOGOUT
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
    }