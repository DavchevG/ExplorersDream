using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ExplorersDream.Models;
using ExplorersDream.ViewModels;  // Добавяме ViewModels за новите модели
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace ExplorersDream.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // 🔹 Логин
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Невалиден имейл или парола");
                return View(model);
            }

            // Проверка дали акаунтът е деактивиран
            if (!user.UserStatus)
            {
                ModelState.AddModelError(string.Empty, "Вашият акаунт е деактивиран. Моля, свържете се с администратора.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }


            ModelState.AddModelError(string.Empty, "Неуспешен опит за вход");
            return View(model);
        }

        // 🔹 Регистрация
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model); // Връща изгледа с текущите грешки
            }

            // Проверка дали имейлът вече е използван
            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail != null)
            {
                ModelState.AddModelError("Email", "Този имейл вече е използван.");
            }

            // Проверка дали потребителското име вече е заето
            var userByUserName = await _userManager.FindByNameAsync(model.UserName);
            if (userByUserName != null)
            {
                ModelState.AddModelError("UserName", "Това потребителско име вече е заето.");
            }

            if (!ModelState.IsValid)
            {
                return View(model); // Връща изгледа с грешките, ако имейлът или потребителското име са заети
            }

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            // Добавяне на грешки от резултата при създаване на потребителя
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model); // Връща изгледа с всички грешки
        }

        // 🔹 Логаут
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // 🔹 Профил на потребителя
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var model = new ProfileViewModel
            {
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName
            };

            return View(model);
        }

        // 🔹 Обновяване на профила
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Email = model.Email;
            user.UserName = model.UserName;

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Profile");
        }
    }
}
