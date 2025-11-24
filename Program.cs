using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Models;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// DB
var conn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(conn));

// Identity with ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var supportedCultures = new[]
{
    new CultureInfo("uk-UA"),
    new CultureInfo("en-US"),
};

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var app = builder.Build();

// Seed data (roles, admin, products)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var db = services.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "myshopadmin@gmail.com";
    string adminPassword = "MyShopAdmin1!";

    // Roles
    string[] roles = new[] { "Admin", "User" };
    foreach (var r in roles)
    {
        if (!await roleManager.RoleExistsAsync(r))
            await roleManager.CreateAsync(new IdentityRole(r));
    }

    // 2. Створюємо користувача-адміна usermanager
    var admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(admin, adminPassword);

        if (!result.Succeeded)
        {
            Console.WriteLine("Помилка створення адміна:");
            foreach (var error in result.Errors)
                Console.WriteLine(error.Description);
        }
    }

    // 3. Додаємо користувача в роль Admin
    if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    // Seed categories/products if none
    if (!db.Categories.Any())
    {
        var electronics = new Category { Key = "Electronics" };
        var books = new Category { Key = "Books" };
        var kitchen = new Category { Key = "Kitchen" };
        var games = new Category { Key = "Games" };

        db.Categories.AddRange(electronics, books, kitchen, games);

        db.Products.AddRange(
            new Product { Name = "Бездротові навушники", Description = "Зручні та легкі Bluetooth-навушники.", Price = 1499, Category = electronics },
            new Product { Name = "USB-C зарядний пристрій", Description = "Швидка зарядка 30W.", Price = 399, Category = electronics },
            new Product { Name = "Ігрова мишка RaptorX", Description = "RGB підсвітка, сенсор 12000 DPI.", Price = 899, Category = electronics },
            new Product { Name = "Набір каструль SilverCook", Description = "3 каструлі з нержавіючої сталі.", Price = 1299, Category = kitchen },
            new Product { Name = "Електрочайник GlassHeat", Description = "Скляний, об‘єм 1.7 л.", Price = 699, Category = kitchen },
            new Product { Name = "Книга «Програмування на C# з нуля»", Description = "Покроковий навчальний посібник.", Price = 499, Category = books },
            new Product { Name = "Книга «Математика для програмістів»", Description = "Бази дискретної математики.", Price = 349, Category = books },
            new Product { Name = "Настільна гра «Колонізатори»", Description = "Популярна стратегічна гра.", Price = 899, Category = games },
            new Product { Name = "Карткова гра «UNO»", Description = "Класична сімейна гра.", Price = 199, Category = games }
        );

        await db.SaveChangesAsync();
    }
}

// middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("uk-UA"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Identity pages

app.Run();
