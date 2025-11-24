using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _db;
        public AdminController(ApplicationDbContext db) => _db = db;

        public async Task<IActionResult> Index()
        {
            var products = await _db.Products!.Include(p => p.Category).ToListAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _db.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Key");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _db.Categories.ToListAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Key", model.CategoryId);

                return View(model);
            }

            _db.Products.Add(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _db.Products!.FindAsync(id);
            if (product == null) return NotFound();
            var categories = await _db.Categories.ToListAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Key");
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _db.Categories.ToListAsync();
                ViewBag.Categories = new SelectList(categories, "Id", "Key", model.CategoryId);

                return View(model);
            }

            _db.Products.Update(model);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _db.Products!.FindAsync(id);
            if (product != null)
            {
                _db.Products!.Remove(product);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
