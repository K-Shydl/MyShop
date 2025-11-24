using Microsoft.AspNetCore.Mvc;
using MyShop.Data;
using MyShop.Models;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products!.Include(p => p.Category).ToListAsync();
            return View(products);
        }
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
