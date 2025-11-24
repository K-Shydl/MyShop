using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Controllers
{
    [Authorize]
    public class PurchaseHistoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchaseHistoryController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToAction("Index", "Home");

            var history = await _db.PurchaseHistory
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.PurchasedAt)
                .ToListAsync();

            return View(history);
        }
    }
}
