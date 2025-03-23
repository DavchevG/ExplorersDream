using Microsoft.AspNetCore.Mvc;
using ExplorersDream.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using PayPal.Api;


public class CartController : Controller
{
    // Показване на количката
    public IActionResult Index()
    {
        var cart = GetCart();
        return View(cart);
    }

    // Добавяне на продукт в количката
    [HttpPost]
    public IActionResult AddToCart(int productId, string productName, decimal price, int quantity, string size, int categoryId)
    {
        var cart = GetCart();

        // Проверяваме по ProductId И Size
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId && item.Size == size);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartModel
            {
                ProductId = productId,
                ProductName = productName,
                Price = price,
                Quantity = quantity,
                Size = size,
                CategoryId = categoryId
            });
        }

        SaveCart(cart);
        return NoContent();
    }

    [HttpPost]
    public IActionResult UpdateQuantity([FromBody] UpdateCartRequestModel request)
    {
        var cart = HttpContext.Session.GetString("Cart");
        if (string.IsNullOrEmpty(cart))
        {
            return Json(new { success = false, message = "Количката е празна." });
        }

        var cartItems = JsonConvert.DeserializeObject<List<CartModel>>(cart);

        var item = cartItems.FirstOrDefault(i => i.ProductId == request.ProductId && i.Size == request.Size);
        if (item != null)
        {
            item.Quantity = request.Quantity;
            HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cartItems));
            return Json(new { success = true });
        }

        return Json(new { success = false, message = "Продуктът не е намерен." });
    }


    // Премахване на продукт от количката
    public IActionResult RemoveFromCart(int productId, string size)
    {
        var cart = GetCart();
        var item = cart.FirstOrDefault(i => i.ProductId == productId && i.Size == size);
        if (item != null)
        {
            cart.Remove(item);
        }
        SaveCart(cart);
        return RedirectToAction("Index");
    }

    public IActionResult Checkout()
    {
        // Проверка дали количката е празна
        var cart = GetCart();
        if (!cart.Any())
        {
            return RedirectToAction("Index");
        }

        // Връщане на изгледа за поръчка
        return View();
    }

    [HttpPost]
    public IActionResult CompleteOrder()
    {
        var cart = GetCart();
        if (!cart.Any())
        {
            return Json(new { success = false, message = "Количката е празна." });
        }

        // Филтрираме само продуктите с ID 1 или 2 и без размер
        var invalidItems = cart.Where(i => string.IsNullOrEmpty(i.Size) && (i.CategoryId == 1 || i.CategoryId == 2)).ToList();
        if (invalidItems.Any())
        {
            return Json(new { success = false, message = "Моля, изберете размер за всички дрехи и обувки!" });
        }

        return Json(new { success = true });
    }

    public IActionResult OrderConfirmation()
    {
        return View();
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

    // Записване на количката в сесията
    private void SaveCart(List<CartModel> cart)
    {
        HttpContext.Session.SetString("Cart", JsonConvert.SerializeObject(cart));
    }
}
