using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<HOMEIXDbContext>(opt =>
    opt.UseSqlServer(cs));

// ======================
// AUTHENTICATION CONFIG
// ======================
builder.Services.AddAuthentication("HomeixAuth")
    .AddCookie("HomeixAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";

        // 🔥 DO NOT SPECIFY LogoutPath because your action is LogoutPost, not Logout.
        // options.LogoutPath = "/Account/Logout";  <-- REMOVE THIS LINE

        options.ExpireTimeSpan = TimeSpan.FromHours(5);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

// ROUTING
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
