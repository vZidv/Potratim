using Potratim.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

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

builder.Services.AddDbContext<PotratimDbContext>(options =>
{
    options.UseNpgsql(fullConnectionString.ToString());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
