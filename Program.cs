using Potratim.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Potratim.Models;
using Microsoft.AspNetCore.Identity;
using System.Xml.Serialization;
using Potratim.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddControllersWithViews();

var connectionString = configuration.GetConnectionString(nameof(PotratimDbContext));
var dbPassword = builder.Configuration["DatabasePassword"];
var fullConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
{
    Password = dbPassword
};

builder.Services.AddScoped<ICartService, CartService>();

builder.Services.AddDbContext<PotratimDbContext>(options =>
{
    options.UseNpgsql(fullConnectionString.ToString());
});

builder.Services.AddIdentity<User, UserRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;


    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<PotratimDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromDays(5); // Время жизни сессии
    options.Cookie.HttpOnly = true; 
    options.Cookie.IsEssential = true; 
    options.Cookie.SameSite = SameSiteMode.Strict; 
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
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

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "Admin",
    pattern: "{area:exists}/{controller=Panel}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


SeedData.SeedDatabaseAsync(app.Services).Wait();


app.Run();
