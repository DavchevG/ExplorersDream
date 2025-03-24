using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ExplorersDream.Models;

public class PayPalController : Controller
{
    private const decimal BGN_TO_USD = 0.55m; // Примерен курс BGN -> USD

    // Връща стойността на количката в левове
    [HttpGet]
    public IActionResult GetCartTotal()
    {
        var cart = GetCart();
        var totalInLev = cart.Sum(item => item.Price * item.Quantity);
        return Json(totalInLev);
    }

    // Метод за PayPal плащане (конвертира от BGN в USD)
    [HttpPost]
    public IActionResult CreatePayPalOrder()
    {
        var cart = GetCart();
        var totalInLev = cart.Sum(item => item.Price * item.Quantity);
        var totalInUsd = totalInLev * BGN_TO_USD; 

        return Json(new { totalInUsd = Math.Round(totalInUsd, 2) });
    }

    // Вземане на количката от сесията
    private List<CartModel> GetCart()
    {
        var cart = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cart))
        {
            return new List<CartModel>();
        }
        return JsonConvert.DeserializeObject<List<CartModel>>(cart) ?? new List<CartModel>();
    }
}
