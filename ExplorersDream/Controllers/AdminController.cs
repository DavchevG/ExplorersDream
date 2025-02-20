using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExplorersDream.Data;
using ExplorersDream.Models;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Dashboard за администратора
    public IActionResult Index()
    {
        return View();
    }

    // Преглед на всички потребители
    public IActionResult Users()
    {
        var users = _userManager.Users.ToList();
        return View(users);
    }

    // Изтриване на потребител
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            // Изтриване на потребителя
            await _userManager.DeleteAsync(user);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Users));
    }

    // Преглед на всички продукти
    public IActionResult Products()
    {
        var products = _context.Products.ToList();
        return View(products);
    }

    // Изтриване на продукт
    [HttpPost]
    public IActionResult DeleteProduct(int id)
    {
        var product = _context.Products.Find(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Products));
    }

    // Преглед на всички поръчки
    public IActionResult Orders()
    {
        var orders = _context.Orders
            .Include(o => o.UserId)
            .Include(o => o.Products)
            .ThenInclude(oi => oi.Product)
            .ToList();
        return View(orders);
    }

    // Изтриване на поръчка
    [HttpPost]
    public IActionResult DeleteOrder(int id)
    {
        var order = _context.Orders.Find(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }
        return RedirectToAction(nameof(Orders));
    }
}
