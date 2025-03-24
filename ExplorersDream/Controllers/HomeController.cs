using ExplorersDream.Data;
using ExplorersDream.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExplorersDream.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult OrderSuccess()
        {
            // Можеш да добавиш логика тук, например да съхраниш поръчката в базата
            // или да покажеш съобщение на потребителя, че плащането е успешно
            TempData["Message"] = "Вашето плащане беше успешно! Благодарим Ви!";
            return View(); // Ще отиде към view, което показва потвърждение
        }

        public IActionResult Index()
        {     
            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
