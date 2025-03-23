using Microsoft.AspNetCore.Mvc;
using PayPal.Api;
using Newtonsoft.Json;
using ExplorersDream.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using PayPal;

namespace ExplorersDream.Controllers
{
    public class PaymentController : Controller
    {
        private readonly APIContext _apiContext;

        public PaymentController(APIContext apiContext)
        {
            _apiContext = apiContext;
        }

        // Създаване на плащане
        [HttpPost]
        public ActionResult CreatePayment([FromBody] PaymentRequest paymentRequest)
        {
            var cartItems = GetCart();

            if (cartItems.Count == 0 || paymentRequest.TotalAmount <= 0)
            {
                return BadRequest("Количката е празна или сума за плащане е невалидна.");
            }

            var payer = new Payer { payment_method = "paypal" };

            var transactionList = new List<Transaction>
            {
                new Transaction
                {
                    amount = new Amount
                    {
                        currency = "BGN",
                        total = paymentRequest.TotalAmount.ToString("F2")
                    },
                    description = "Поръчка в Explorers Dream"
                }
            };

            var redirectUrls = new RedirectUrls
            {
                cancel_url = Url.Action("Cancel", "Payment", null, Request.Scheme),
                return_url = Url.Action("Success", "Payment", null, Request.Scheme)
            };

            var payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirectUrls
            };

            try
            {
                // Създаване на плащането
                var createdPayment = payment.Create(_apiContext);
                // Извличане на URL за одобрение
                var approvalUrl = createdPayment.links.FirstOrDefault(link => link.rel == "approval_url")?.href;

                if (string.IsNullOrEmpty(approvalUrl))
                {
                    return BadRequest("Не може да се получи URL за одобрение на плащането.");
                }

                // Връщане на URL за редирект към PayPal
                return Json(new { approvalUrl });
            }
            catch (PayPalException ex)
            {
                // Обработка на грешка
                return BadRequest($"Грешка при създаване на плащането: {ex.Message}");
            }
        }

        // Успешно плащане
        public IActionResult Success(string paymentId, string PayerID)
        {
            if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
            {
                return View("Error", new { message = "Недействителни параметри за плащане." });
            }

            var paymentExecution = new PaymentExecution { payer_id = PayerID };
            var payment = new Payment { id = paymentId };

            try
            {
                var executedPayment = payment.Execute(_apiContext, paymentExecution);
                if (executedPayment.state.ToLower() == "approved")
                {
                    ClearCart();
                    return View("Success");
                }
            }
            catch (PayPalException ex)
            {
                return View("Error", new { message = ex.Message });
            }

            return View("Error", new { message = "Неуспешно плащане." });
        }

        // Отказано плащане
        public IActionResult Cancel() => View("Cancel");

        // Изчистване на количката от сесията
        private void ClearCart()
        {
            HttpContext.Session.Remove("Cart");
        }

        // Вземане на количката от сесията
        private List<CartModel> GetCart()
        {
            var cart = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(cart) ? new List<CartModel>() : JsonConvert.DeserializeObject<List<CartModel>>(cart) ?? new List<CartModel>();
        }
    }

    // Модел за заявка за плащане
    public class PaymentRequest
    {
        public decimal TotalAmount { get; set; }
    }
}
