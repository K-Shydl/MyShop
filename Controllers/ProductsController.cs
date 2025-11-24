using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;

namespace MyShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index(int? categoryId)
        {
            var categories = await _db.Categories.ToListAsync();
            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = categoryId;

            var query = _db.Products.Include(p => p.Category).AsQueryable();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products!.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}
