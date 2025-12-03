using eCommerce.Data;
using eCommerce.Helpers;
using eCommerce.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;


        public CartController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            return View(cart);
        }

        public IActionResult Add(int id, int quantity)
        {
            var product = _db.Products.Find(id);

            if (product == null)
                return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(c => c.ProductId == id);

            if (existing != null)
            {
                // product already exists → increase quantity
                existing.Quantity += quantity;
            }
            else
            {
                // new product in cart
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObject("cart", cart);

            return RedirectToAction("Details", "Products", new { id });
        }



        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart") ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
                cart.Remove(item);

            HttpContext.Session.SetObject("cart", cart);

            return RedirectToAction("Index", "Cart", new { id });
        }


        public IActionResult Toggle(int id)
        {
            var product = _db.Products.Find(id);
            if (product == null)
                return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(c => c.ProductId == id);

            if (existing == null)
            {
                // Item NOT in cart → Add it
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price,
                    Quantity = 1
                });
            }
            else
            {
                // Item already in cart → Remove it
                cart.Remove(existing);
            }

            HttpContext.Session.SetObject("cart", cart);

            // Return to the product details page
            return RedirectToAction("Details", "Products", new { id });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
            {
                if (quantity <= 0)
                {
                    // if user enters 0 or less → remove item
                    cart.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
            }

            HttpContext.Session.SetObject("cart", cart);

            return RedirectToAction("Index");
        }

        [Authorize]
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = CartHelper.GetCart(HttpContext.Session);

            if (!cart.Any())
                return RedirectToAction("Index");

            return View(new CheckoutViewModel());
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = CartHelper.GetCart(HttpContext.Session);

            if (!cart.Any())
                return RedirectToAction("Index");

            var userId = _userManager.GetUserId(User);

            // Create Order
            var order = new Order
            {
                UserId = userId,
                ShippingAddress = model.ShippingAddress,
                CreatedAt = DateTime.Now,
                Items = new List<OrderItem>(),
                Total = 0
            };

            decimal total = 0;

            foreach (var item in cart)
            {
                var product = await _db.Products.FindAsync(item.ProductId);
                if (product == null) continue;

                var orderItem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price
                };

                order.Items.Add(orderItem);

                total += product.Price * item.Quantity;
            }

            order.Total = total;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart
            CartHelper.ClearCart(HttpContext.Session);

            return RedirectToAction("Confirmation", new { id = order.Id });
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }



    }
}
