using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;

namespace MyShop.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PurchasesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // Якщо не залогінений користувач натисне Купити -> ця сторінка покаже повідомлення
        [HttpGet]
        public IActionResult NotLoggedIn()
        {
            return View();
        }

        // Підтвердження купівлі (повинен бути залогінений)
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Confirm(int id)
        {
            var product = await _db.Products!.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // Обробка підтвердження
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ConfirmBuy(int id)
        {
            var product = await _db.Products!
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(NotLoggedIn));

            // Перевірка балансу
            if (user.Balance < product.Price)
            {
                return RedirectToAction(nameof(Insufficient));
            }

            // Знімаємо гроші
            user.Balance -= product.Price;
            await _userManager.UpdateAsync(user);

            // Видаляємо товар з каталогу (вимога)
            _db.Products.Remove(product);
            await _db.SaveChangesAsync();

            var history = new PurchaseHistory
            {
                UserId = user.Id,
                ProductName = product.Name,
                ProductDescription = product.Description,
                Price = product.Price,
                CategoryKey = product.Category?.Key
            };

            _db.PurchaseHistory.Add(history);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Success));
        }

        // Сторінка недостатньо коштів
        [Authorize]
        public IActionResult Insufficient()
        {
            return View();
        }

        // Сторінка успішної покупки
        [Authorize]
        public IActionResult Success()
        {
            return View();
        }

        // Поповнення балансу +20 грн (POST)
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> TopUp()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction(nameof(NotLoggedIn));

            user.Balance += 20m;
            await _userManager.UpdateAsync(user);

            // Повертаємо користувача назад (реферер) або на головну
            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Home");
        }
    }
}
