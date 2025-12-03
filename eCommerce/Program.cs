using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using eCommerce.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database context with Identity support
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // No email confirmation needed
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add IHttpContextAccessor so Razor @inject can use it
builder.Services.AddHttpContextAccessor();

// Session (for cart)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();        // session BEFORE auth is allowed
app.UseAuthentication(); // <--- IMPORTANT
app.UseAuthorization();  // <--- IMPORTANT

// Identity uses Razor Pages for Login/Register
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
