using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Homeix.Data;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.FileProviders;
using Homeix.Hubs; // ✅ SignalR Hub

var builder = WebApplication.CreateBuilder(args);

// ======================
// LOGGING
// ======================
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// ======================
// DATABASE
// ======================
var cs = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string not found.");

builder.Services.AddDbContext<HOMEIXDbContext>(opt =>
    opt.UseSqlServer(cs)
);

// ======================
// AUTHENTICATION (COOKIE)
// ======================
builder.Services
    .AddAuthentication("HomeixAuth")
    .AddCookie("HomeixAuth", options =>
    {
        options.Cookie.Name = "Homeix.Auth";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = TimeSpan.FromHours(5);
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();

// ======================
// MVC + LOCALIZATION
// ======================
builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);

builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// ======================
// REQUEST LOCALIZATION
// ======================
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[]
    {
        new CultureInfo("en-US"),
        new CultureInfo("ar-JO")
    };

    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(),
        new CookieRequestCultureProvider()
    };
});

// ======================
// SIGNALR ✅
// ======================
builder.Services.AddSignalR();

var app = builder.Build();

// ======================
// LOCALIZATION MIDDLEWARE
// ======================
var locOptions = app.Services
    .GetRequiredService<Microsoft.Extensions.Options.IOptions<RequestLocalizationOptions>>();

app.UseRequestLocalization(locOptions.Value);

// ======================
// ERROR HANDLING
// ======================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ======================
// STATIC FILES
// ======================
app.UseHttpsRedirection();
app.UseStaticFiles();

// uploads folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(app.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});

// ======================
// ROUTING + AUTH
// ======================
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// ======================
// MVC ROUTES
// ======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// ======================
// SIGNALR HUB ROUTE ✅
// ======================
app.MapHub<ChatHub>("/chatHub");

// ======================
// RUN
// ======================
app.Run();
