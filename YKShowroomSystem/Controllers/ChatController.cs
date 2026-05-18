using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DatabaseModel.Models;
using System.Text;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    public class ChatRequest
    {
        public string? Message { get; set; }
    }

    [HttpPost]
    public JsonResult Ask([FromBody] ChatRequest? request)
    {
        string? message = request?.Message;

        if (string.IsNullOrWhiteSpace(message))
        {
            message = Request.Form["message"].FirstOrDefault()
                      ?? Request.Query["message"].FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return Json(new
            {
                reply = "Please type your question. / من فضلك اكتب سؤالك."
            });
        }

        string userMessage = message.Trim().ToLower();

        // greetings
        if (ContainsAny(userMessage, "hello", "hi", "hey", "مرحبا", "هلا", "السلام", "سلام"))
        {
            return Json(new
            {
                reply = "Hello! I’m your showroom assistant. You can ask me about available cars, categories, showrooms, locations, showroom phone numbers, salespersons, working hours, working days, and ratings. / أهلاً! أنا مساعد الشوروم. يمكنك سؤالي عن السيارات، الفئات، الشورومات، المواقع، أرقام الهواتف، موظفي المبيعات، أوقات العمل، أيام العمل، والتقييمات."
            });
        }

        // best rated showroom must be before general rating/showroom checks
        if (ContainsAny(userMessage, "best showroom", "highest rating", "top showroom", "which showroom has the highest rating", "أفضل شوروم", "أفضل فرع", "اعلى تقييم"))
        {
            var bestShowroom = _context.Feedbacks
                .GroupBy(f => f.ShowroomId)
                .Select(g => new
                {
                    ShowroomId = g.Key,
                    AvgRating = g.Average(x => x.Rating)
                })
                .OrderByDescending(x => x.AvgRating)
                .FirstOrDefault();

            if (bestShowroom == null)
            {
                return Json(new
                {
                    reply = "There is no showroom rating data available right now."
                });
            }

            var showroomName = _context.Showrooms
                .Where(s => s.Id == bestShowroom.ShowroomId)
                .Select(s => s.Name)
                .FirstOrDefault();

            return Json(new
            {
                reply = $"The highest-rated showroom is {showroomName} with an average rating of {Math.Round(bestShowroom.AvgRating, 1)} out of 5."
            });
        }

        // working hours
        if (ContainsAny(userMessage, "working hours", "hours", "hour", "open", "opening", "time", "وقات", "وقت", "ساعات", "يفتح", "دوام"))
        {
            return Json(new
            {
                reply = "Our working hours are from 9:00 AM to 8:00 PM. / ساعات العمل من 9:00 صباحًا إلى 8:00 مساءً."
            });
        }

        // working days
        if (ContainsAny(userMessage, "working days", "days", "day", "weekend", "أيام العمل", "ايام العمل", "أيام", "ايام"))
        {
            return Json(new
            {
                reply = "We are open from Sunday to Thursday. / نحن نعمل من الأحد إلى الخميس."
            });
        }

        // showroom phone
        if (ContainsAny(userMessage, "phone", "number", "contact", "telephone", "هاتف", "رقم", "اتصال", "تواصل"))
        {
            var matchedShowroom = FindMatchingShowroom(userMessage);
            if (matchedShowroom != null)
            {
                if (string.IsNullOrWhiteSpace(matchedShowroom.Phone))
                {
                    return Json(new
                    {
                        reply = $"The phone number for {matchedShowroom.Name} is not available right now. / رقم هاتف {matchedShowroom.Name} غير متوفر حاليًا."
                    });
                }

                return Json(new
                {
                    reply = $"The phone number for {matchedShowroom.Name} is {matchedShowroom.Phone}."
                });
            }

            var matchedSalesperson = FindMatchingSalesperson(userMessage);
            if (matchedSalesperson != null)
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(matchedSalesperson.Phone))
                    parts.Add($"phone: {matchedSalesperson.Phone}");

                if (!string.IsNullOrWhiteSpace(matchedSalesperson.Email))
                    parts.Add($"email: {matchedSalesperson.Email}");

                if (parts.Count == 0)
                {
                    return Json(new
                    {
                        reply = $"No contact details are available for {matchedSalesperson.Name} right now."
                    });
                }

                return Json(new
                {
                    reply = $"{matchedSalesperson.Name} contact details: {string.Join(", ", parts)}."
                });
            }

            return Json(new
            {
                reply = "Please mention the showroom or salesperson name so I can show the correct contact details."
            });
        }

        // showroom location
        if (ContainsAny(userMessage, "location", "where", "address", "located", "موقع", "وين", "أين", "عنوان"))
        {
            var matchedShowroom = FindMatchingShowroom(userMessage);
            if (matchedShowroom != null)
            {
                if (string.IsNullOrWhiteSpace(matchedShowroom.Location))
                {
                    return Json(new
                    {
                        reply = $"The location for {matchedShowroom.Name} is not available right now. / موقع {matchedShowroom.Name} غير متوفر حاليًا."
                    });
                }

                return Json(new
                {
                    reply = $"{matchedShowroom.Name} is located in {matchedShowroom.Location}."
                });
            }

            var showroomLocations = _context.Showrooms
                .Select(s => new { s.Name, s.Location })
                .ToList();

            if (!showroomLocations.Any())
            {
                return Json(new
                {
                    reply = "No showroom locations are available right now. / لا توجد مواقع شورومات متوفرة حاليًا."
                });
            }

            var sb = new StringBuilder();

            foreach (var item in showroomLocations)
            {
                sb.Append($"{item.Name}: {item.Location}. ");
            }

            return Json(new { reply = sb.ToString().Trim() });
        }

        // categories
        if (ContainsAny(userMessage, "category", "categories", "type", "types", "فئة", "فئات", "نوع", "أنواع"))
        {
            var categories = _context.Products
                .Where(p => p.Category != null && p.Category != "")
                .Select(p => p.Category)
                .Distinct()
                .ToList();

            if (!categories.Any())
            {
                return Json(new
                {
                    reply = "There are no product categories available right now. / لا توجد فئات منتجات متوفرة حاليًا."
                });
            }

            return Json(new
            {
                reply = "Available categories: " + string.Join(", ", categories)
            });
        }

        // products by category
        var categoriesInDb = _context.Products
            .Where(p => p.Category != null && p.Category != "")
            .Select(p => p.Category!)
            .Distinct()
            .ToList();

        foreach (var category in categoriesInDb)
        {
            if (userMessage.Contains(category.ToLower()))
            {
                var categoryProducts = _context.Products
                    .Where(p => p.Category == category)
                    .Select(p => p.Name)
                    .Distinct()
                    .ToList();

                if (categoryProducts.Any())
                {
                    return Json(new
                    {
                        reply = $"{category} cars/products: " + string.Join(", ", categoryProducts)
                    });
                }
            }
        }

        // products / cars
        if (ContainsAny(userMessage, "car", "cars", "product", "products", "model", "models", "vehicle", "vehicles", "available cars", "سيارة", "سيارات", "موديل", "الموديلات"))
        {
            var products = _context.Products
                .Select(p => p.Name)
                .Distinct()
                .ToList();

            if (!products.Any())
            {
                return Json(new
                {
                    reply = "There are no available cars/products in the system right now. / لا توجد سيارات أو منتجات متوفرة حاليًا في النظام."
                });
            }

            return Json(new
            {
                reply = "Available cars/products: " + string.Join(", ", products)
            });
        }

        // specific product exists?
        var matchedProduct = FindMatchingProduct(userMessage);
        if (matchedProduct != null)
        {
            var productReply = new StringBuilder();
            productReply.Append($"{matchedProduct.Name}");

            if (!string.IsNullOrWhiteSpace(matchedProduct.Category))
                productReply.Append($" is in the {matchedProduct.Category} category");

            productReply.Append(".");

            return Json(new { reply = productReply.ToString() });
        }

        // showrooms list
        if (ContainsAny(userMessage, "showroom", "showrooms", "branch", "branches", "فرع", "فروع", "شوروم", "شورومات"))
        {
            var showrooms = _context.Showrooms
                .Select(s => s.Name)
                .Distinct()
                .ToList();

            if (!showrooms.Any())
            {
                return Json(new
                {
                    reply = "There are no showrooms available right now. / لا توجد شورومات متوفرة حاليًا."
                });
            }

            return Json(new
            {
                reply = "Available showrooms: " + string.Join(", ", showrooms)
            });
        }

        // salespersons by showroom
        if (ContainsAny(userMessage, "sales", "salesperson", "employee", "staff", "who works", "مندوب", "موظف", "موظفين", "سيلز", "بائع"))
        {
            var matchedShowroom = FindMatchingShowroom(userMessage);

            if (matchedShowroom != null)
            {
                var people = _context.Salespersons
                    .Where(s => s.ShowroomId == matchedShowroom.Id)
                    .Select(s => s.Name)
                    .Distinct()
                    .ToList();

                if (!people.Any())
                {
                    return Json(new
                    {
                        reply = $"There are no salespersons assigned to {matchedShowroom.Name} right now."
                    });
                }

                return Json(new
                {
                    reply = $"Salespersons in {matchedShowroom.Name}: " + string.Join(", ", people)
                });
            }

            var salespersons = _context.Salespersons
                .Select(s => s.Name)
                .Distinct()
                .ToList();

            if (!salespersons.Any())
            {
                return Json(new
                {
                    reply = "No salespersons are available right now. / لا يوجد موظفو مبيعات متوفرون حاليًا."
                });
            }

            return Json(new
            {
                reply = "Available salespersons: " + string.Join(", ", salespersons)
            });
        }

        // ratings / average feedback
        if (ContainsAny(userMessage, "rating", "ratings", "feedback", "review", "reviews", "تقييم", "تقييمات", "مراجعات"))
        {
            var avg = _context.Feedbacks.Any()
                ? Math.Round(_context.Feedbacks.Average(f => f.Rating), 1)
                : 0;

            if (avg == 0)
            {
                return Json(new
                {
                    reply = "There is no feedback available right now. / لا توجد تقييمات متوفرة حاليًا."
                });
            }

            return Json(new
            {
                reply = $"The average customer rating is {avg} out of 5."
            });
        }

        // thanks
        if (ContainsAny(userMessage, "thanks", "thank you", "شكرا", "شكرًا", "مشكور", "يعطيك العافية"))
        {
            return Json(new
            {
                reply = "You’re welcome! If you need anything else, feel free to ask. / العفو! إذا احتجت أي شيء ثاني أنا حاضر."
            });
        }

        return Json(new
        {
            reply = "Sorry, I can help with cars, categories, showrooms, showroom locations, phone numbers, salespersons, ratings, working hours, and working days. / عذرًا، أستطيع المساعدة في السيارات، الفئات، الشورومات، المواقع، أرقام الهواتف، موظفي المبيعات، التقييمات، أوقات العمل، وأيام العمل."
        });
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(k => text.Contains(k));
    }

    private Showroom? FindMatchingShowroom(string userMessage)
    {
        return _context.Showrooms
            .AsEnumerable()
            .FirstOrDefault(s =>
                (!string.IsNullOrWhiteSpace(s.Name) && userMessage.Contains(s.Name.ToLower())) ||
                (!string.IsNullOrWhiteSpace(s.NameAr) && userMessage.Contains(s.NameAr.ToLower())) ||
                (!string.IsNullOrWhiteSpace(s.Location) && userMessage.Contains(s.Location.ToLower()))
            );
    }

    private Product? FindMatchingProduct(string userMessage)
    {
        return _context.Products
            .AsEnumerable()
            .FirstOrDefault(p =>
                (!string.IsNullOrWhiteSpace(p.Name) && userMessage.Contains(p.Name.ToLower())) ||
                (!string.IsNullOrWhiteSpace(p.NameAr) && userMessage.Contains(p.NameAr.ToLower())) ||
                (!string.IsNullOrWhiteSpace(p.Category) && userMessage.Contains(p.Category.ToLower()))
            );
    }

    private Salesperson? FindMatchingSalesperson(string userMessage)
    {
        return _context.Salespersons
            .AsEnumerable()
            .FirstOrDefault(s =>
                (!string.IsNullOrWhiteSpace(s.Name) && userMessage.Contains(s.Name.ToLower())) ||
                (!string.IsNullOrWhiteSpace(s.Email) && userMessage.Contains(s.Email.ToLower())) ||
                (!string.IsNullOrWhiteSpace(s.Phone) && userMessage.Contains(s.Phone.ToLower()))
            );
    }
}