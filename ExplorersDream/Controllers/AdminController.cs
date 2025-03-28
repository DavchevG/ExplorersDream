using ExplorersDream.Data;
using ExplorersDream.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    // За връщане към списък на потребителите, можем да използваме самия метод Index без изглед
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();

        var userList = new List<ApplicationUser>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            user.IsAdmin = roles.Contains("Admin"); // Временно свойство (НЕ се запазва в базата)
            userList.Add(user);
        }

        return View(userList);
    }


    // Изтриване на потребител
    [HttpPost]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        return RedirectToAction(nameof(Index)); // Тук ще може да се направи обновление, без да е необходимо изглед
    }

    // Промяна на статус на потребител
    [HttpPost]
    public async Task<IActionResult> ChangeStatus(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            user.UserStatus = !user.UserStatus; // Обърнато е състоянието на потребителя
            var result = await _userManager.UpdateAsync(user);
        }
        return RedirectToAction(nameof(Index)); // Ще пренасочим отново към индекс страницата
    }

    // Направи потребител администратор
    [HttpPost]
    public async Task<IActionResult> MakeAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            var roleExists = await _roleManager.RoleExistsAsync("Admin");
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Потребителят стана администратор.";
            }
            else
            {
                TempData["ErrorMessage"] = "Неуспешно добавяне на роля.";
            }
        }

        return RedirectToAction(nameof(Index)); // Пренасочва към същата страница след действие
    }

    // Премахване на администраторска роля от потребител
    [HttpPost]
    public async Task<IActionResult> RemoveAdmin(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user != null)
        {
            var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Ролята администратор беше премахната.";
            }
            else
            {
                TempData["ErrorMessage"] = "Неуспешно премахване на роля.";
            }
        }
        return RedirectToAction(nameof(Index)); // Пренасочване към същата страница след промяна
    }
}
