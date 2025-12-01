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
        public async Task<IActionResult> Index(int? categoryId, int page = 1, int pageSize = 8)
        {
            var query = _db.Products.Include(p => p.Category).AsQueryable();

            // Filter by category if provided
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            // Pagination
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.CategoryId = categoryId;

            return View(products);
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
