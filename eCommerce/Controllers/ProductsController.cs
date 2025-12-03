using eCommerce.Data;
using eCommerce.Helpers;
using eCommerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eCommerce.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _db;
        public ProductsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Products
        public async Task<IActionResult> Index(
     int? categoryId,
     decimal? minPrice,
     decimal? maxPrice,
     string query,
     int page = 1)
        {
            int pageSize = 8; // number of products per page

            var products = _db.Products
                              .Include(p => p.Category) // so category name works
                              .AsQueryable();

            var categories = await _db.Categories.ToListAsync();

            // CATEGORY FILTER
            if (categoryId.HasValue)
                products = products.Where(p => p.CategoryId == categoryId);

            // PRICE FILTER
            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);

            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            // SEARCH FILTER
            if (!string.IsNullOrWhiteSpace(query))
            {
                string lower = query.ToLower();
                products = products.Where(p =>
                    p.Name.ToLower().Contains(lower) ||
                    p.Description.ToLower().Contains(lower)
                );
            }

            // --------------------------
            // PAGINATION LOGIC
            // --------------------------

            int totalProducts = await products.CountAsync();
            int totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var items = await products
                .OrderBy(p => p.Id)                 // stable ordering
                .Skip((page - 1) * pageSize)         // jump pages
                .Take(pageSize)                      // take page size
                .ToListAsync();

            // PASS VALUES TO VIEW
            ViewBag.Categories = categories;

            ViewBag.Query = query;
            ViewBag.SelectedCategory = categoryId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }



        // GET: /Products/Details/5

        public async Task<IActionResult> Details(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Check if product is in cart
            var cart = HttpContext.Session.GetObject<List<CartItem>>("cart")
                       ?? new List<CartItem>();

            ViewBag.InCart = cart.Any(c => c.ProductId == id);

            return View(product);
        }




    }
}
