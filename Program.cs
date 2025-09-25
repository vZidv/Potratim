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


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<UserRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    UserRole[] userRoles = new[]
    {
        new UserRole { Id = Guid.NewGuid(), Name = "Admin" , Color = "ff5252"},
        new UserRole { Id = Guid.NewGuid(), Name = "User", Color = "b0b0b0" },
        new UserRole { Id = Guid.NewGuid(), Name = "Moderator", Color = "42caff" }
    };
    foreach (var userRole in userRoles)
    {
        if (!await roleManager.RoleExistsAsync(userRole.Name))
        {
            await roleManager.CreateAsync(userRole);
        }
    }

    // if (!userManager.Users.Any())
    {
        // Создаем администратора
        var adminUser = new User()
        {
            Id = Guid.NewGuid(),
            UserName = "admin",
            Email = "admin@example.com",
            Nickname = "Admin",
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(adminUser, "admin123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            System.Console.WriteLine($"Ошибка создания администратора: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        // Создаем модератора
        var moderatorUser = new User
        {
            Id = Guid.NewGuid(),
            UserName = "moderator@example.com",
            Email = "moderator@example.com",
            Nickname = "Moder",
            CreatedAt = DateTime.UtcNow,
        };
        var moderatorResult = await userManager.CreateAsync(moderatorUser, "moder123");
        if (moderatorResult.Succeeded)
        {
            await userManager.AddToRoleAsync(moderatorUser, "Moderator");
        }
        else
        {
            Console.WriteLine($"Ошибка создания модератора: {string.Join(", ", moderatorResult.Errors.Select(e => e.Description))}");
        }
    }
}


app.Run();
