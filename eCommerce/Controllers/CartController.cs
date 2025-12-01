using eCommerce.Data;
using eCommerce.Models;
using eCommerce.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace eCommerce.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CartController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            return View(cart);
        }

        public IActionResult Add(int id)
        {
            // add item to session cart (this part is correct in your code)

            return RedirectToAction("Details", "Products", new { id });
              // show the cart
              // OR:
              // return RedirectToAction("Index", "Products"); // go back to products list
              // OR:
              // return RedirectToAction("Details", "Products", new { id }); // back to product
        }


        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart");

            if (cart == null) return RedirectToAction("Index");

            var item = cart.FirstOrDefault(c => c.ProductId == id);

            if (item != null)
                cart.Remove(item);

            HttpContext.Session.SetObject("cart", cart);

            return RedirectToAction("Index");
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

    }
}
