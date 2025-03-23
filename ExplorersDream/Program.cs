using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ExplorersDream.Data;
using ExplorersDream.Models;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация на Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Конфигурация на Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Добавяне на Stripe
var stripeSettings = builder.Configuration.GetSection("Stripe").Get<StripeSettings>();
if (stripeSettings == null || string.IsNullOrEmpty(stripeSettings.SecretKey))
{
    throw new Exception("Stripe API ключовете не са конфигурирани!");
}
StripeConfiguration.ApiKey = stripeSettings.SecretKey;

// Конфигуриране на PayPal API в основната конфигурация
var paypalSettings = builder.Configuration.GetSection("PayPal").Get<PayPalSettings>();
if (paypalSettings == null || string.IsNullOrEmpty(paypalSettings.ClientId) || string.IsNullOrEmpty(paypalSettings.ClientSecret))
{
    throw new Exception("PayPal API ключовете не са конфигурирани!");
}

var config = new Dictionary<string, string>
{
    { "clientId", paypalSettings.ClientId },
    { "clientSecret", paypalSettings.ClientSecret },
    { "mode", paypalSettings.Mode }
};

builder.Services.AddSingleton<PayPal.Api.APIContext>(serviceProvider =>
{
    var accessToken = new PayPal.Api.OAuthTokenCredential(
        paypalSettings.ClientId,
        paypalSettings.ClientSecret
    ).GetAccessToken();

    var apiContext = new PayPal.Api.APIContext(accessToken)
    {
        Config = config
    };

    return apiContext;
});

// Добавяне на сесии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Например 30 минути сесия
});

// Добавяне на контролери и изгледи
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Конфигуриране на HTTP конвейера
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Автентикация, авторизация и сесии
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Използване на сесии в приложението

// Дефиниране на маршрути
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Стартиране на приложението
app.Run();

// Функция за създаване на роли и добавяне на администратор
static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
            Console.WriteLine($"Ролята {role} е създадена.");
        }
    }
}

// Изпълнение на инициализацията на ролите и администраторите
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedRolesAndAdminAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding roles and admin user.");
    }
}
