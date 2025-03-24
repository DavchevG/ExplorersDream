using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using ExplorersDream.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace ExplorersDream.Controllers
{
    public class StripeController : Controller
    {
        private readonly IConfiguration _configuration;

        public StripeController(IConfiguration configuration)
        {
            _configuration = configuration;
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
        }

        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateCheckoutSession()
        {
            var cartItems = GetCart();

            if (cartItems.Count == 0)
            {
                return BadRequest("Количката е празна.");
            }

            var domain = $"{Request.Scheme}://{Request.Host}";

            var lineItems = cartItems.Select(cartItem => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "bgn",
                    UnitAmount = (long)(cartItem.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = string.IsNullOrEmpty(cartItem.Size)
                            ? cartItem.ProductName
                            : $"{cartItem.ProductName} ({cartItem.Size})",
                    }
                },
                Quantity = cartItem.Quantity
            }).ToList();

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = Url.Action("OrderSuccess", "Home", null, Request.Scheme),
                CancelUrl = Url.Action("Checkout", "Cart", null, Request.Scheme),
                Locale = "bg"

            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Json(new { id = session.Id });
        }

        public IActionResult Success()
        {
            HttpContext.Session.Remove("Cart");
            return View();
        }

        public IActionResult Cancel()
        {
            return View();
        }

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
}
