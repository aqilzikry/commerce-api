using CommerceAPI.Data;
using CommerceAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Security.Claims;

namespace CommerceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly CommerceAPIContext dBContext;

        public CartController(CommerceAPIContext dBContext, IConfiguration configuration)
        {
            this.dBContext = dBContext;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult GetCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Unauthorized access!" });
            }

            CartResponse response = GetCartResponse(userId);

            return Ok(new Response { Status = true, Data = response });
        }

        [HttpPost]
        public IActionResult AddToCart([FromBody] CartRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Unauthorized access!" });
            }

            Cart? cart = dBContext.Carts
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == request.ProductId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    ProductId = request.ProductId ?? 1,
                    Quantity = request.Quantity ?? 1,
                    OrderId = null,
                };

                dBContext.Carts.Add(cart);
            }
            else
            {
                cart.Quantity++;
            }

            dBContext.SaveChanges();

            CartResponse response = GetCartResponse(userId);

            return Ok(new Response { Status = true, Data = response });
        }

        [HttpPut]
        public IActionResult UpdateCartItem([FromBody] CartRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Unauthorized access!" });
            }

            Cart? cart = dBContext.Carts
                .FirstOrDefault(c => c.Id == request.CartId && c.UserId == userId);

            if (cart == null)
            {
                return NotFound(new Response { Status = false, Message = "Cart not found!" });
            }

            cart.Quantity = request.Quantity ?? 0;

            dBContext.SaveChanges();

            CartResponse response = GetCartResponse(userId);

            return Ok(new Response { Status = true, Data = response });
        }

        [HttpDelete("{productId}")]
        public IActionResult DeleteCart(int productId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Unauthorized access!" });
            }

            var cartItemToDelete = dBContext.Carts.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (cartItemToDelete == null)
            {
                return NotFound(new Response { Status = false, Message = "Product not found in the cart!" });
            }

            dBContext.Carts.Remove(cartItemToDelete);
            dBContext.SaveChanges();

            return Ok(new Response
            {
                Status = true,
                Message = "Product removed from the cart successfully!",
                Data = GetCartResponse(userId)
            });
        }

        [HttpDelete]
        public IActionResult DeleteCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return Unauthorized(new Response { Status = false, Message = "Unauthorized access!" });
            }

            List<Cart> cartsToDelete = dBContext.Carts
                .Where(c => c.UserId == userId)
                .ToList();

            dBContext.Carts.RemoveRange(cartsToDelete);
            dBContext.SaveChanges();

            return Ok(new Response
            {
                Status = true,
                Message = "Cart emptied successfully!",
                Data = new CartResponse
                {
                    Total = "0.00",
                    CartItems = new List<CartItem>()
                }
            });
        }

        [HttpPost]
        [Route("checkout")]
        public IActionResult Checkout()
        {
            StripeConfiguration.ApiKey = _configuration["StripeAPIKey"];

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            List<Cart> carts = dBContext.Carts
              .Include(c => c.Product)
              .Where(c => c.UserId == userId && c.OrderId == null)
              .ToList();

            long total = (long)(carts.Sum(item => item.Product?.Price * item.Quantity) * 100)!;

            // Generate a random 4-character alphanumeric string
            var random = new Random();
            var randomPart = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Append random string to current date
            var currentDatePart = DateTime.Now.ToString("ddMM");
            var orderId = $"{randomPart}-{currentDatePart}";

            var options = new PaymentIntentCreateOptions
            {
                Amount = total, // Amount in cents
                Currency = "myr",
                Description = $"Payment for Order {orderId}",
                PaymentMethodTypes = new List<string> { "card" },
                ReceiptEmail = userEmail,
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent = service.Create(options);

            var confirmOptions = new PaymentIntentConfirmOptions
            {
                PaymentMethod = "pm_card_visa",
            };
            var confirmService = new PaymentIntentService();
            PaymentIntent confirmedIntent = confirmService.Confirm(paymentIntent.Id, confirmOptions);

            return Ok(new { clientSecret = confirmedIntent.ClientSecret });
        }

        private CartResponse GetCartResponse(string userId)
        {
            List<Cart> carts = dBContext.Carts
              .Include(c => c.Product)
              .Where(c => c.UserId == userId && c.OrderId == null)
              .ToList();

            List<CartItem> items = carts.SelectMany(cart => new List<CartItem>
            {
                new CartItem
                {
                    Id = cart.Id,
                    Product = cart.Product,
                    Quantity = cart.Quantity,
                    Subtotal = string.Format("RM {0:0.00}", cart.Product?.Price * cart.Quantity),
                }
            }).ToList();

            return new CartResponse
            {
                Total = string.Format("{0:0.00}", carts.Sum(item => item.Product?.Price * item.Quantity)),
                CartItems = items,
            };
        }
    }
}
